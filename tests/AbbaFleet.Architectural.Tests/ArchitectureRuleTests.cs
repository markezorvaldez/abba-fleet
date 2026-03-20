using AbbaFleet.Shared;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using Microsoft.AspNetCore.Components;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace AbbaFleet.Architectural.Tests;

public class ArchitectureRuleTests
{
    private static readonly System.Lazy<ArchUnitNET.Domain.Architecture> ArchLazy =
        new(() => new ArchLoader()
            .LoadAssembly(typeof(Permission).Assembly)
            .Build());

    private static ArchUnitNET.Domain.Architecture Arch => ArchLazy.Value;

    // --- Feature isolation ---

    [Fact]
    public void AuthFeature_ShouldNotDependOn_UsersFeature()
    {
        Types().That().ResideInNamespace("AbbaFleet.Features.Auth", useRegularExpressions: false)
            .Should().NotDependOnAnyTypesThat()
            .ResideInNamespace("AbbaFleet.Features.Users", useRegularExpressions: false)
            .Check(Arch);
    }

    [Fact]
    public void UsersFeature_ShouldNotDependOn_AuthFeature()
    {
        Types().That().ResideInNamespace("AbbaFleet.Features.Users", useRegularExpressions: false)
            .Should().NotDependOnAnyTypesThat()
            .ResideInNamespace("AbbaFleet.Features.Auth", useRegularExpressions: false)
            .Check(Arch);
    }

    [Fact]
    public void DashboardFeature_ShouldNotDependOn_OtherFeatures()
    {
        Types().That().ResideInNamespace("AbbaFleet.Features.Dashboard", useRegularExpressions: false)
            .Should().NotDependOnAnyTypesThat()
            .ResideInNamespace("AbbaFleet.Features.Auth", useRegularExpressions: false)
            .AndShould().NotDependOnAnyTypesThat()
            .ResideInNamespace("AbbaFleet.Features.Users", useRegularExpressions: false)
            .Check(Arch);
    }

    // --- Component boundaries ---

    [Fact]
    public void Components_ShouldNotDependOn_Features()
    {
        Types().That().ResideInNamespace("AbbaFleet.Components", useRegularExpressions: false)
            .Should().NotDependOnAnyTypesThat()
            .ResideInNamespace("AbbaFleet.Features", useRegularExpressions: false)
            .Check(Arch);
    }

    [Fact]
    public void Components_ShouldNotDependOn_Infrastructure()
    {
        Types().That().ResideInNamespace("AbbaFleet.Components", useRegularExpressions: false)
            .Should().NotDependOnAnyTypesThat()
            .ResideInNamespace("AbbaFleet.Infrastructure", useRegularExpressions: false)
            .Check(Arch);
    }

    // --- Infrastructure boundaries ---

    [Fact]
    public void Infrastructure_ShouldNotDependOn_Features()
    {
        Types().That().ResideInNamespace("AbbaFleet.Infrastructure", useRegularExpressions: false)
            .Should().NotDependOnAnyTypesThat()
            .ResideInNamespace("AbbaFleet.Features", useRegularExpressions: false)
            .Check(Arch);
    }

    // --- Data access boundaries ---

    [Fact]
    public void RazorComponents_ShouldNotDependOn_DbContext()
    {
        Types().That().AreAssignableTo(typeof(ComponentBase))
            .Should().NotDependOnAnyTypesThat()
            .HaveFullNameContaining("AppDbContext")
            .Check(Arch);
    }

    [Fact]
    public void RazorComponents_ShouldNotDependOn_EntityFrameworkCore()
    {
        Types().That().AreAssignableTo(typeof(ComponentBase))
            .Should().NotDependOnAnyTypesThat()
            .ResideInNamespace("Microsoft.EntityFrameworkCore", useRegularExpressions: false)
            .Check(Arch);
    }
}
