
namespace ArchUnitNETTests;

public class Tests
{    
    private static readonly Architecture Architecture = new ArchLoader().LoadAssemblies(
        System.Reflection.Assembly.Load("ExampleClassAssemblyName"),
        System.Reflection.Assembly.Load("ForbiddenClassAssemblyName")
    ).Build();
    // replace <ExampleClass> and <ForbiddenClass> with classes from the assemblies you want to test

    //declare variables you'll use throughout your tests up here
    //use As() to give them a custom description
    private readonly IObjectProvider<IType> ExampleLayer = 
        ArchRuleDefinition.Classes().That().ResideInAssembly("ExampleAssembly").As("Example Layer");

    private readonly IObjectProvider<Class> ExampleClasses =
        ArchRuleDefinition.Classes().That().ImplementInterface("IExampleInterface").As("Example Classes");

    private readonly IObjectProvider<IType> ForbiddenLayer =
        ArchRuleDefinition.Types().That().ResideInNamespace("ForbiddenNamespace").As("Forbidden Layer");

    private readonly IObjectProvider<Interface> ForbiddenInterfaces =
        ArchRuleDefinition.Interfaces().That().HaveFullNameContaining("forbidden").As("Forbidden Interfaces");
    
    [Test]
    public void TypesShouldBeInCorrectLayer()
    {
        //you can use the fluent API to write your own rules
        IArchRule exampleClassesShouldBeInExampleLayer =
            ArchRuleDefinition.Classes().That().Are(ExampleClasses).Should().Be(ExampleLayer);
        IArchRule forbiddenInterfacesShouldBeInForbiddenLayer =
            ArchRuleDefinition.Interfaces().That().Are(ForbiddenInterfaces).Should().Be(ForbiddenLayer);

        //check if your architecture fulfils your rules
        exampleClassesShouldBeInExampleLayer.Check(Architecture);
        forbiddenInterfacesShouldBeInForbiddenLayer.Check(Architecture);

        //you can also combine your rules
        IArchRule combinedArchRule =
            exampleClassesShouldBeInExampleLayer.And(forbiddenInterfacesShouldBeInForbiddenLayer);
        combinedArchRule.Check(Architecture);
    }
}