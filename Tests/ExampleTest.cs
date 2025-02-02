using TestFramework;

namespace Tests;

[TestClass]
public class ExampleTest
{
  [TestMethod]
  public void TestSomething()
  {
    Assert.That(1, 1);
  }

  [TestMethod]
  public void Test_Hello_World_Throws_Exception()
  {
    var hello = "Hello";
    var world = "World";

    Assert.ThrowsException<ArgumentException>(delegate
    {
      Assert.That(hello, world);
    });
  }

  [TestMethod]
  public void Test_Is_NotNull()
  {
    List<int>? ints = [1, 2, 3];

    Assert.IsNotNull(ints);
  }

  [TestMethod]
  public void Test_ThrowsException()
  {
    List<int>? ints = [1, 2, 3];

    var exception = Assert.ThrowsException<ArgumentException>(delegate
    {
      Assert.IsNull(ints);
    }
    );

    Assert.IsNotNull(exception);

  }

}
