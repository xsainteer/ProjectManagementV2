using Application.Interfaces;
using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET.NUnit;
using Domain.Common;
using Infrastructure.Data;
using Presentation.Endpoints;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ProjectTests.Common;

[TestFixture]
public class DependencyTests
{
    private static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(
            typeof(IEntity).Assembly,      // Domain
            typeof(IRepository<>).Assembly, // Application
            typeof(AppDbContext).Assembly,  // Infrastructure
            typeof(ResultMapper).Assembly   // Presentation
        ) 
        .Build();

    private static readonly IObjectProvider<IType> DomainLayer = 
        Types().That().ResideInAssembly(typeof(IEntity).Assembly).As("Domain Layer");

    private static readonly IObjectProvider<IType> ApplicationLayer = 
        Types().That().ResideInAssembly(typeof(IRepository<>).Assembly).As("Application Layer");

    private static readonly IObjectProvider<IType> InfrastructureLayer = 
        Types().That().ResideInAssembly(typeof(AppDbContext).Assembly).As("Infrastructure Layer");

    private static readonly IObjectProvider<IType> PresentationLayer = 
        Types().That().ResideInAssembly(typeof(ResultMapper).Assembly).As("Presentation Layer");

    [Test]
    public void Domain_ShouldNotDependOn_ExternalLibraries()
    {
        // Запрещаем всё, что не является частью ваших сборок или системных библиотек
        IArchRule rule = Types().That().Are(DomainLayer)
            .Should().NotDependOnAnyTypesThat()
            .ResideInNamespaceMatching("^(?!(Domain|System|Nullable|Microsoft.CSharp)).*");

        rule.Check(Architecture);
    }
    
    [Test]
    public void Domain_ShouldNotDependOn_AnyOtherLayer()
    {
        IArchRule rule = Types().That().Are(DomainLayer)
            .Should().NotDependOnAny(ApplicationLayer)
            .AndShould().NotDependOnAny(InfrastructureLayer)
            .AndShould().NotDependOnAny(PresentationLayer);

        rule.Check(Architecture);
    }

    [Test]
    public void Application_ShouldOnlyDependOn_DomainLayer()
    {
        IArchRule rule = Types().That().Are(ApplicationLayer)
            .Should().NotDependOnAny(InfrastructureLayer)
            .AndShould().NotDependOnAny(PresentationLayer);

        rule.Check(Architecture);
    }

    [Test]
    public void Infrastructure_ShouldNotDependOn_PresentationLayer()
    {
        IArchRule rule = Types().That().Are(InfrastructureLayer)
            .Should().NotDependOnAny(PresentationLayer);

        rule.Check(Architecture);
    }
}
