using System.Reflection;

namespace TestFramework;

public static class TestRunner
{
    public static void Run()
    {
        var assembly = Assembly.GetCallingAssembly();

        foreach (var type in assembly.GetTypes())
        {
            if (type.GetCustomAttribute<TestClassAttribute>() != null)
            {
                Console.WriteLine($"Running tests in {type.Name}");

                var testMethods = type.GetMethods()
                .Where(method => method.GetCustomAttribute<TestMethodAttribute>() != null);

                var testClassInstance = Activator.CreateInstance(type);

                foreach (var method in testMethods)
                {
                    try
                    {
                        Console.Write($"- Running {method.Name}... ");
                        method.Invoke(testClassInstance, null);
                        Console.WriteLine("✅ Passed");
                    }
                    catch (Exception ex)
                    {
                        var targetEx = ex.InnerException ?? ex;

                        Console.WriteLine($"❌ Failed: {targetEx}");
                    }
                }
            }
        }
    }
}