using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ptn.TestModule.Managers.Catalog;
using Ptn.TestModule.Services.Catalog;
using Shouldly;
using Xunit;

namespace Ptn.TestModule.Application.Tests.Composition;

// islevi: Her Manager'in kendi klasoru disindan en az bir kez cagrildigini statik olarak dogrular.
// sistemdeki gorevi: Yazilmis ama disaridan ulasilamayan domain davranisini derlenmeden yakalar (house profile: wire the chain to the outside).
public class ManagerReachabilityTests
{
    // Her somut Manager, Application katmanindan dogrudan veya baska bir Manager uzerinden tuketilmelidir.
    [Fact]
    public void Every_manager_should_be_reachable_from_the_application_layer()
    {
        var managers = GetManagerTypes();
        var reachable = GetApplicationReachableManagers(managers);

        var unreachable = managers
            .Where(manager => !reachable.Contains(manager))
            .Select(manager => manager.FullName)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        unreachable.ShouldBeEmpty();
    }

    // Olcumun bos kumede kazara yesil kalmasini engeller; Manager grafigi daralirsa test uyarir.
    [Fact]
    public void Manager_surface_should_stay_populated()
    {
        GetManagerTypes().Count.ShouldBeGreaterThan(20);
    }

    // Domain assembly'sindeki somut Manager tiplerini toplar.
    private static IReadOnlyList<Type> GetManagerTypes()
    {
        return typeof(TestScenarioManager).Assembly
            .GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && type.Name.EndsWith("Manager", StringComparison.Ordinal))
            .ToList();
    }

    // Application tuketimlerinden baslayip Manager-Manager kompozisyonu uzerinden kapanisi hesaplar.
    private static HashSet<Type> GetApplicationReachableManagers(IReadOnlyList<Type> managers)
    {
        var applicationTypes = typeof(TestScenarioAppService).Assembly.GetTypes();
        var reachable = managers.Where(manager => IsConsumedByAny(applicationTypes, manager)).ToHashSet();

        bool grew;
        do
        {
            var next = managers
                .Where(manager => !reachable.Contains(manager))
                .Where(manager => IsConsumedByAny(reachable, manager))
                .ToArray();
            grew = next.Length > 0;
            reachable.UnionWith(next);
        }
        while (grew);

        return reachable;
    }

    // Verilen tuketici kumesinin Manager'i ctor parametresi veya taban generic argumani olarak tasiyip tasimadigini bildirir.
    private static bool IsConsumedByAny(IEnumerable<Type> consumers, Type manager)
    {
        return consumers.Any(consumer => consumer != manager &&
                                         (TakesAsConstructorParameter(consumer, manager) ||
                                          UsesAsBaseGenericArgument(consumer, manager)));
    }

    // Constructor injection ile tuketimi yakalar.
    private static bool TakesAsConstructorParameter(Type consumer, Type manager)
    {
        return consumer
            .GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .SelectMany(constructor => constructor.GetParameters())
            .Any(parameter => parameter.ParameterType == manager);
    }

    // Foundation taban siniflarina generic arguman olarak verilen Manager tuketimini yakalar.
    private static bool UsesAsBaseGenericArgument(Type consumer, Type manager)
    {
        for (var current = consumer.BaseType; current is not null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericArguments().Contains(manager))
            {
                return true;
            }
        }

        return false;
    }
}
