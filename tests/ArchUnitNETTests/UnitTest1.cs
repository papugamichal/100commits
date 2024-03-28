namespace ArchUnitNETTests;

public class Tests
{
    private static readonly System.Reflection.Assembly CoreAssembly = typeof(ArchUnit.Core.Class1).Assembly;
    private static readonly System.Reflection.Assembly ApplicationAssembly = typeof(ArchUnit.Application.Class1).Assembly;
    private static readonly System.Reflection.Assembly InfraAssembly = typeof(ArchUnit.Infra.Class1).Assembly;
    private static readonly System.Reflection.Assembly WebAssembly = typeof(ArchUnit.Web.Class1).Assembly;
    
    private static readonly Architecture Architecture = new ArchLoader().LoadAssemblies(
        CoreAssembly,
        ApplicationAssembly,
        InfraAssembly,
        WebAssembly
        // System.Reflection.Assembly.Load("ExampleClassAssemblyName"),
        // System.Reflection.Assembly.Load("ForbiddenClassAssemblyName")
    ).Build();
    // replace <ExampleClass> and <ForbiddenClass> with classes from the assemblies you want to test

    //declare variables you'll use throughout your tests up here
    //use As() to give them a custom description
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
    public void AggregateDefinedInApplicationViolatesTheRule()
    {
        var applicationLayerDependsOnCore = 
            ArchRuleDefinition.Classes().That().HaveNameEndingWith("Aggregate").Should().Be(CoreLayer)
        .Because("Aggregates should live in the CORE layer");

        applicationLayerDependsOnCore.Check(Architecture);
    }
}