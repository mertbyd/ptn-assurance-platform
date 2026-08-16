using System.Collections.Generic;
using Ptn.DatabaseChecker.Models.Comparison;

namespace Ptn.DatabaseChecker.Repository.Comparison;

// islevi: Bir snapshot okumasinda tablo kimligine gore gruplanan kolon, index, constraint ve trigger haritalarini adlandirilmis tek degerde tasir.
// sistemdeki gorevi: Dort degerli tuple yerine repository-ici tip guvenli detay kontrati saglar.
internal sealed class SchemaTableDetailMaps<TKey>
    where TKey : struct
{
    internal required Dictionary<TKey, List<SchemaColumnModel>> ColumnsByTable { get; init; }

    internal required Dictionary<TKey, List<SchemaIndexModel>> IndexesByTable { get; init; }

    internal required Dictionary<TKey, List<SchemaConstraintModel>> ConstraintsByTable { get; init; }

    internal required Dictionary<TKey, List<SchemaTriggerModel>> TriggersByTable { get; init; }
}
