using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.Fluent;
using ArchUnitNET.NUnit;
using Domain.Common;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ProjectTests.Domain;

[TestFixture]
public class DependencyTests
{
    private static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(typeof(IEntity).Assembly) 
        .Build();
    
    [Test]
    public void Domain_ShouldNotDependOn_AnythingExternal()
    {
        IArchRule rule = Classes()
            .That().ResideInNamespaceMatching("Domain.Entities.*")
            .Should().NotDependOnAnyTypesThat()
            .ResideInNamespaceMatching("^(?!(Domain|System|Nullable)).*");

        rule.Check(Architecture);
    }
}