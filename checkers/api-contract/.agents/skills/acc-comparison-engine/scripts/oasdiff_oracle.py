#!/usr/bin/env python3
"""oasdiff oracle gate for the comparison engine.

Runs oasdiff over every checked-in fixture pair, folds its 500+ check catalog down to
our closed DifferenceKind catalog, and compares the result against the finding set the
engine is locked to by SpecComparisonOracle_Tests.

Every surviving disagreement must be listed in accepted-deviations.json with a reason.
Anything else fails the gate, so a silent engine regression cannot pass CI.

Usage:
  python .agents/skills/acc-comparison-engine/scripts/oasdiff_oracle.py [--oasdiff PATH]

Exit codes: 0 = agreed, 1 = unexplained deviation, 2 = setup problem.
"""
import argparse
import json
import os
import shutil
import subprocess
import sys

REPO_ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "..", ".."))
FIXTURE_ROOT = os.path.join(
    REPO_ROOT, "test", "Ptn.ApiContractChecker.EntityFrameworkCore.Tests",
    "EntityFrameworkCore", "Comparison", "Fixtures")
LEDGER_PATH = os.path.join(os.path.dirname(os.path.abspath(__file__)), "accepted-deviations.json")

# Our closed catalog. Kept in sync with DifferenceKindCodes.All by hand; the gate
# verifies the count so an added code cannot go unnoticed here.
OUR_KINDS = {
    "new-required-request-property", "request-property-became-required",
    "request-property-type-changed", "request-parameter-enum-value-removed",
    "request-body-became-required", "response-property-became-optional",
    "response-property-became-nullable", "response-success-status-removed",
    "response-media-type-removed", "required-response-header-removed",
    "endpoint-added", "endpoint-removed", "schema-added", "api-schema-removed",
    "schema-renamed", "description-changed",
}

# oasdiff check id -> our DifferenceKind code. Ids absent here are outside our catalog
# and are dropped before comparison, which is what keeps 500+ checks from producing noise.
OASDIFF_TO_OUR_KIND = {
    "new-required-request-property": "new-required-request-property",
    "request-property-became-required": "request-property-became-required",
    "request-property-type-changed": "request-property-type-changed",
    "response-property-type-changed": "request-property-type-changed",
    "request-parameter-enum-value-removed": "request-parameter-enum-value-removed",
    "request-body-became-required": "request-body-became-required",
    "response-property-became-optional": "response-property-became-optional",
    "response-property-became-nullable": "response-property-became-nullable",
    "response-success-status-removed": "response-success-status-removed",
    "response-media-type-removed": "response-media-type-removed",
    "required-response-header-removed": "required-response-header-removed",
    "endpoint-added": "endpoint-added",
    "api-removed-without-deprecation": "endpoint-removed",
    "api-path-removed-without-deprecation": "endpoint-removed",
    "api-removed-before-sunset": "endpoint-removed",
    "api-schema-removed": "api-schema-removed",
}

# oasdiff changelog levels are numeric: 3 = error, 2 = warning, 1 = info. Our catalog has
# no warning code, so anything below error folds into the honest "does not break" bucket.
def severity_of(level):
    return "breaking" if level >= 3 else "non-breaking"


def find_oasdiff(explicit):
    if explicit:
        return explicit if os.path.exists(explicit) else None
    found = shutil.which("oasdiff")
    if found:
        return found
    candidate = os.path.expanduser(os.path.join("~", "go", "bin", "oasdiff.exe"))
    if os.path.exists(candidate):
        return candidate
    candidate = os.path.expanduser(os.path.join("~", "go", "bin", "oasdiff"))
    return candidate if os.path.exists(candidate) else None


def run_oasdiff(binary, case_dir):
    result = subprocess.run(
        [binary, "changelog", os.path.join(case_dir, "base.json"),
         os.path.join(case_dir, "target.json"), "-f", "json"],
        capture_output=True, text=True)
    if result.returncode != 0:
        raise RuntimeError(result.stderr.strip()[:400])
    return json.loads(result.stdout) if result.stdout.strip() else []


def oracle_kinds(changes):
    """Folds oasdiff output into {our_kind: severity}, dropping out-of-catalog checks."""
    folded = {}
    for change in changes:
        kind = OASDIFF_TO_OUR_KIND.get(change["id"])
        if kind is None:
            continue
        severity = severity_of(change["level"])
        # Breaking wins when the same kind arrives at more than one level.
        if folded.get(kind) != "breaking":
            folded[kind] = severity
    return folded


def engine_kinds(case_dir):
    """Reads the finding set the engine is locked to and folds it the same way."""
    with open(os.path.join(case_dir, "expected.json"), encoding="utf-8") as handle:
        lines = json.load(handle)
    folded = {}
    for line in lines:
        kind, severity = (part.strip() for part in line.split("|")[:2])
        if folded.get(kind) != "breaking":
            folded[kind] = severity
    return folded


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--oasdiff", help="path to the oasdiff binary")
    args = parser.parse_args()

    binary = find_oasdiff(args.oasdiff)
    if binary is None:
        print("SETUP: oasdiff not found. Install it with:")
        print("  go install github.com/oasdiff/oasdiff@latest")
        return 2

    with open(LEDGER_PATH, encoding="utf-8") as handle:
        ledger = json.load(handle)
    accepted = {(entry["case"], entry["kind"]): entry["reason"] for entry in ledger["deviations"]}

    cases = sorted(name for name in os.listdir(FIXTURE_ROOT)
                   if os.path.isdir(os.path.join(FIXTURE_ROOT, name)))
    if not cases:
        print("SETUP: no fixture pairs found under", FIXTURE_ROOT)
        return 2

    unexplained = []
    stale = set(accepted)
    print("%-38s %-34s %-10s %-10s %s" % ("CASE", "KIND", "ENGINE", "OASDIFF", "VERDICT"))
    print("-" * 108)

    for case in cases:
        case_dir = os.path.join(FIXTURE_ROOT, case)
        try:
            ours = engine_kinds(case_dir)
            theirs = oracle_kinds(run_oasdiff(binary, case_dir))
        except (OSError, RuntimeError, ValueError) as error:
            print("%-38s %s" % (case, "ERROR: %s" % error))
            unexplained.append((case, "<run>"))
            continue

        for kind in sorted(set(ours) | set(theirs)):
            ours_severity = ours.get(kind, "-")
            theirs_severity = theirs.get(kind, "-")
            if ours_severity == theirs_severity:
                verdict = "agree"
            elif (case, kind) in accepted:
                verdict = "accepted: " + accepted[(case, kind)]
                stale.discard((case, kind))
            else:
                verdict = "*** UNEXPLAINED ***"
                unexplained.append((case, kind))
            print("%-38s %-34s %-10s %-10s %s" % (case, kind, ours_severity, theirs_severity, verdict))

    print("-" * 108)
    if len(OUR_KINDS) != 16:
        print("SETUP: OUR_KINDS drifted from the 16-code catalog.")
        return 2

    for case, kind in sorted(stale):
        print("STALE ledger entry (no longer deviates): %s / %s" % (case, kind))

    if unexplained:
        print("FAIL: %d unexplained deviation(s). Fix the engine or record the reason in %s."
              % (len(unexplained), os.path.basename(LEDGER_PATH)))
        return 1

    print("PASS: %d cases; every deviation is either agreement or a recorded deliberate difference."
          % len(cases))
    return 0


if __name__ == "__main__":
    sys.exit(main())
