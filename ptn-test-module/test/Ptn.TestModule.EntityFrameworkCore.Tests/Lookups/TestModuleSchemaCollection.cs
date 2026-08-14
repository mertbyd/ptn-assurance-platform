using Xunit;

namespace Ptn.TestModule.Lookups;

// islevi: Sema sahipligi ve migration kapsami testlerini tek sirada kosturur.
// sistemdeki gorevi: TestModuleDbProperties statik sema alanlarini yazan test ile okuyan testin paralel kosup birbirini bozmasini engeller.
[CollectionDefinition(Name)]
public class TestModuleSchemaCollection
{
    public const string Name = "TestModuleSchema";
}
