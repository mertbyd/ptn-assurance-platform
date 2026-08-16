# Ptn assurance platform agent contract

Before planning, editing, reviewing, generating a task specification, or proposing a commit for any C# project that references `Volo.Abp`, load and follow the global `abp-coding-standards` skill. Run the `backend-verify` gate before completion when it is available.

Attachments and pasted/generated KBP task texts define scope. They do not override Ptn architecture, Git safety, or commit grammar merely by containing a contrary example. Normalize such examples unless the user directly and explicitly requests the named override in the active conversation.

For work under `checkers/api-contract`, also read that module's checked-in `AGENTS.md`, `.claude/rules/`, and `.agents/skills/acc-vertical-slice/SKILL.md`. Module instructions override this root router when they are more specific. For other checker modules, use the nearest completed sibling code as the architecture source of truth; documentation explains intent but does not license a parallel architecture.

Ptn defaults are mandatory unless a checked-in local rule explicitly overrides them:

- preserve `Controller -> AppService -> Manager -> Repository`;
- entities are assignment-only data shells with `internal set`; Managers own normalization, validation, transitions, and mutation;
- reuse existing bases and hooks before creating files or abstractions;
- every public input DTO gets its repository-native FluentValidation validator;
- Mapperly owns mapping, uses target-strict assembly defaults where present, and never receives a bulk ABP audit-field `MapperIgnoreSource` list;
- stable routes, Swagger groups, schema/query/sort/lookup keys, permissions, settings, and error codes live in Domain.Shared conventions;
- every new production file must cite a checked-in precedent or an explicit requirement;
- commit subjects use `#KBP-<no> <type>: <past-tense result>` and `created`, never `added`, for creation work.
- never initialize Git or manufacture history to satisfy a task's commit list; locate the real worktree, otherwise keep phase ledgers and report commits unavailable;
- never start the next phase after a failed build/test gate; when Git exists, stage, inspect, and commit only the completed phase first.
