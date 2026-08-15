using System;
using System.IO;

namespace Ptn.TestModule.Application.Tests.Composition;

// islevi: Kompozisyon testlerinin modul kok yolunu ve kaynak dosya okumasini ortaklastirir.
// sistemdeki gorevi: Her kompozisyon testinin ayni kok bulma dongusunu yeniden yazmasini engeller.
internal static class TestModuleSourceReader
{
    private const string SolutionFileName = "Ptn.TestModule.slnx";

    // Modul kokunden goreli yoldaki kaynak dosyanin tam metnini getirir.
    internal static string Read(params string[] relativeSegments)
    {
        var path = Path.Combine(FindModuleRoot().FullName, Path.Combine(relativeSegments));
        return File.ReadAllText(path);
    }

    // Test kosucusunun bin klasorunden solution kokune cikar.
    internal static DirectoryInfo FindModuleRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, SolutionFileName)))
        {
            directory = directory.Parent;
        }

        return directory ?? throw new DirectoryNotFoundException(SolutionFileName);
    }
}
