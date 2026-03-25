using AbbaFleet.Shared;
using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using Microsoft.AspNetCore.Components;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace AbbaFleet.Architectural.Tests;

public class ArchitectureRuleTests
{
    private static readonly Lazy<Architecture> ArchLazy =
        new(() => new ArchLoader()
                  .LoadAssembly(typeof(Permission).Assembly)
                  .Build());

    private static Architecture Arch => ArchLazy.Value;

    public static TheoryData<string, string> FeatureNamespacePairs()
    {
        const string prefix = "AbbaFleet.Features.";

        var featureNamespaces = Arch.Types
                                    .Where(t => t.Namespace.FullName.StartsWith(prefix))
                                    .Select(t =>
                                    {
                                        var afterPrefix = t.Namespace.FullName[prefix.Length..];
                                        var dotIndex = afterPrefix.IndexOf('.');

                                        return dotIndex >= 0
                                            ? prefix + afterPrefix[..dotIndex]
                                            : t.Namespace.FullName;
                                    })
                                    .Distinct()
                                    .ToList();

        var data = new TheoryData<string, string>();

        foreach (var a in featureNamespaces)
        {
            foreach (var b in featureNamespaces)
            {
                if (a != b)
                {
                    data.Add(a, b);
                }
            }
        }

        return data;
    }

    // --- Component boundaries ---

    [Fact]
    public void Components_ShouldNotDependOn_Features()
    {
        Types()
            .That()
            .ResideInNamespace("AbbaFleet.Components", useRegularExpressions: false)
            .Should()
            .NotDependOnAnyTypesThat()
            .ResideInNamespace("AbbaFleet.Features", useRegularExpressions: false)
            .Check(Arch);
    }

    [Fact]
    public void Components_ShouldNotDependOn_Infrastructure()
    {
        Types()
            .That()
            .ResideInNamespace("AbbaFleet.Components", useRegularExpressions: false)
            .Should()
            .NotDependOnAnyTypesThat()
            .ResideInNamespace("AbbaFleet.Infrastructure", useRegularExpressions: false)
            .Check(Arch);
    }

    // --- Feature isolation ---
    [Theory]
    [MemberData(nameof(FeatureNamespacePairs))]
    public void Features_ShouldNotDependOn_OtherFeatures(string featureNamespace, string otherFeatureNamespace)
    {
        Types()
            .That()
            .ResideInNamespace(featureNamespace, useRegularExpressions: false)
            .Should()
            .NotDependOnAnyTypesThat()
            .ResideInNamespace(otherFeatureNamespace, useRegularExpressions: false)
            .Check(Arch);
    }

    // --- Infrastructure boundaries ---
    // Infrastructure implements abstractions defined in Features (dependency inversion),
    // so it may depend on domain entities, DTOs, and repository interfaces.
    // It must NOT depend on Razor components.

    [Fact]
    public void Infrastructure_ShouldNotDependOn_RazorComponents()
    {
        Types()
            .That()
            .ResideInNamespace("AbbaFleet.Infrastructure", useRegularExpressions: false)
            .Should()
            .NotDependOnAnyTypesThat()
            .AreAssignableTo(typeof(ComponentBase))
            .Check(Arch);
    }

    // --- Data access boundaries ---

    [Fact]
    public void RazorComponents_ShouldNotDependOn_DbContext()
    {
        Types()
            .That()
            .AreAssignableTo(typeof(ComponentBase))
            .Should()
            .NotDependOnAnyTypesThat()
            .HaveFullNameContaining("AppDbContext")
            .Check(Arch);
    }

    [Fact]
    public void RazorComponents_ShouldNotDependOn_EntityFrameworkCore()
    {
        Types()
            .That()
            .AreAssignableTo(typeof(ComponentBase))
            .Should()
            .NotDependOnAnyTypesThat()
            .ResideInNamespace("Microsoft.EntityFrameworkCore", useRegularExpressions: false)
            .Check(Arch);
    }
}
