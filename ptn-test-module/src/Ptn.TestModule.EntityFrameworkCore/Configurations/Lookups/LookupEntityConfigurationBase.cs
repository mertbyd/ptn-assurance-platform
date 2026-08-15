using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexum.Abp.Foundation.EntityFrameworkCore.Configurations.Lookups;
using Nexum.Abp.Foundation.Lookups;

namespace Ptn.TestModule.EntityFrameworkCore.Configurations.Lookups;

// islevi: Bes lookup tablosunun ortak eslemesini ve sema baglamasini tek yerde kurar.
// sistemdeki gorevi: Kolon, uzunluk ve Code unique index'i Foundation'in ortak lookup eslemesinden gelir; bu taban yalniz tablo adini ve sema sahipligini baglar (RULE-0002).
public abstract class LookupEntityConfigurationBase<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : LookupEntity<Guid>
{
    // Concrete lookup yalnizca kendi kararli tablo adini verir.
    protected abstract string TableName { get; }

    // Sema adi daima TestModuleDbProperties uzerinden okunur; ortam bazli override tek noktadan calisir.
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.ConfigureLookup<TEntity, Guid>(TableName, TestModuleDbProperties.LookupSchema);
    }
}
