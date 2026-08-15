using System.Diagnostics;
using Ptn.TestModule.Constants.Compilation;
using Xunit;
using Xunit.Sdk;

namespace Ptn.TestModule.Application.Tests.LiveInfrastructure;

// islevi: Docker gerektiren canli kanit testlerini tek ve seri bir xUnit koleksiyonunda toplar.
// sistemdeki gorevi: Pinli Redocly imaji bulunmayan ortamlari varsayilan unit suite'ine sizdirmadan acikca atlar.
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class LiveInfrastructureCollection
{
    public const string Name = "LiveInfrastructure";

    // Docker Engine ve pinli Redocly imaji yerelde yoksa canli testi gerekcesiyle atlar.
    public static void EnsurePinnedRedoclyImageIsAvailable()
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = ArazzoCompilationConsts.DockerExecutable,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        process.StartInfo.ArgumentList.Add("image");
        process.StartInfo.ArgumentList.Add("inspect");
        process.StartInfo.ArgumentList.Add(ArazzoCompilationConsts.RedoclyCliImage);

        try
        {
            if (!process.Start())
            {
                throw SkipException.ForSkip("Docker could not start for the live infrastructure preflight.");
            }
        }
        catch (System.ComponentModel.Win32Exception)
        {
            throw SkipException.ForSkip("Docker is not installed for the live infrastructure tests.");
        }

        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            throw SkipException.ForSkip(
                $"Pinned image {ArazzoCompilationConsts.RedoclyCliImage} is not available locally.");
        }
    }
}
