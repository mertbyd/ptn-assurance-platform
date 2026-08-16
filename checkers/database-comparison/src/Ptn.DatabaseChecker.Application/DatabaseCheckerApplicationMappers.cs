using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

[assembly: MapperDefaults(RequiredMappingStrategy = RequiredMappingStrategy.Target)]

namespace Ptn.DatabaseChecker;

[Mapper]
public partial class DatabaseCheckerApplicationMappers
{
    /* You can configure your Mapperly mapping configuration here.
     * Alternatively, you can split your mapping configurations
     * into multiple mapper classes for a better organization. */
}
