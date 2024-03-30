using System.ComponentModel.DataAnnotations;

namespace ArchUnitNETTests;

public class Tests
{
    private static readonly System.Reflection.Assembly CoreAssembly = typeof(ArchUnit.Core.Class1).Assembly;
    private static readonly System.Reflection.Assembly ApplicationAssembly = typeof(ArchUnit.Application.SomeStruct).Assembly;
    private static readonly System.Reflection.Assembly InfraAssembly = typeof(ArchUnit.Infra.InfraType).Assembly;
    private static readonly System.Reflection.Assembly WebAssembly = typeof(ArchUnit.Web.Class1).Assembly;
    
    private static readonly Architecture Architecture = new ArchLoader().LoadAssemblies(
        CoreAssembly,
        ApplicationAssembly,
        InfraAssembly,
        WebAssembly
    ).Build();
    
    private readonly IObjectProvider<IType> CoreLayer = 
        ArchRuleDefinition.Types().That().ResideInAssembly(CoreAssembly).As("Core Layer");
    private readonly IObjectProvider<IType> ApplicationLayer = 
        ArchRuleDefinition.Types().That().ResideInAssembly(ApplicationAssembly).As("Application Layer");
    private readonly IObjectProvider<IType> InfraLayer = 
        ArchRuleDefinition.Types().That().ResideInAssembly(InfraAssembly).As("Infra Layer");
    private readonly IObjectProvider<IType> WebLayer = 
        ArchRuleDefinition.Types().That().ResideInAssembly(WebAssembly).As("Web Layer");

    private readonly IObjectProvider<Class> AggreagatesClasses =
        ArchRuleDefinition.Classes().That().ImplementInterface("IExampleInterface").As("Aggregate Classes");

    private readonly IObjectProvider<IType> ForbiddenLayer =
        ArchRuleDefinition.Types().That().ResideInNamespace("ForbiddenNamespace").As("Forbidden Layer");

    private readonly IObjectProvider<Interface> ForbiddenInterfaces =
        ArchRuleDefinition.Interfaces().That().HaveFullNameContaining("forbidden").As("Forbidden Interfaces");
    
    [Test]
    public void TypesShouldBeInCorrectLayer()
    {
        // //you can use the fluent API to write your own rules
        // IArchRule exampleClassesShouldBeInExampleLayer =
        //     ArchRuleDefinition.Classes().That().Are(ExampleClasses).Should().Be(ExampleLayer);
        // IArchRule forbiddenInterfacesShouldBeInForbiddenLayer =
        //     ArchRuleDefinition.Interfaces().That().Are(ForbiddenInterfaces).Should().Be(ForbiddenLayer);
        //
        // var applicationLayerDependsOnCore = ArchRuleDefinition.Types().That().Are(ApplicationLayer).Should().DependOnAny(CoreLayer)
        //     .Because("Application should depends on Core");
        //
        // //check if your architecture fulfils your rules
        // exampleClassesShouldBeInExampleLayer.Check(Architecture);
        // forbiddenInterfacesShouldBeInForbiddenLayer.Check(Architecture);
        //
        // //you can also combine your rules
        // IArchRule combinedArchRule =
        //     exampleClassesShouldBeInExampleLayer.And(forbiddenInterfacesShouldBeInForbiddenLayer);
        // combinedArchRule.Check(Architecture);
    }

    [Test]
    public void AggregatesShouldBeDefinedInCore()
    {
        var rule = 
            ArchRuleDefinition.Classes().That().HaveNameEndingWith("Aggregate").Should().Be(CoreLayer)
        .Because("Aggregates should live in the Core layer");

        rule.Check(Architecture);
    }
    
    [Test]
    public void InfraTypesShouldNotBeAccessedInCore()
    {
        var rule = 
            ArchRuleDefinition.Types().That().ResideInAssembly(CoreAssembly).Should().NotDependOnAny(InfraLayer)
                .Because("Infra types should not be accessed in Core");

        rule.Check(Architecture);
    }
    
    [Test]
    public void ApplicationTypesShouldNotBePublic()
    {
        var rule = 
            ArchRuleDefinition.Classes().That().ResideInAssembly(ApplicationAssembly).Should().NotBePublic()
                .Because("Application types should not be public");

        rule.Check(Architecture);
    }

    [Test]
    public void ViewModelsAreDefinedInWeb()
    {
        var rule = ArchRuleDefinition.Classes().That().HaveNameEndingWith("ViewModel").Should()
            .ResideInAssembly(WebAssembly).Because("ViewModel types should be defined in Web");
        
        rule.Check(Architecture);
    }

    [Test]
    public void ViewModelsShouldBeDecoratedWithDisplayAttribute()
    {
        var rule = ArchRuleDefinition.Classes().That().HaveNameEndingWith("ViewModel").Should()
            .HaveAnyAttributes(typeof(DisplayAttribute)).Because("ViewModel need Display attribute");
        rule.Check(Architecture);
    }
    
}