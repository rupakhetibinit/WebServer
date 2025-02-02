using System;

namespace TestFramework;

public static class Assert
{
  public static void That(object expected, object actual)
  {
    Type expected_type = expected.GetType();
    Type actual_type = actual.GetType();

    if (expected_type != actual_type)
    {
      throw new ArgumentException($"Expected expected type {expected_type}, but the type was {actual_type}");
    }

    if (!expected.Equals(actual))
    {
      throw new ArgumentException($"Expected {expected} does not equal Actual {actual}");
    }

  }

  public static void That<T>(IEnumerable<T> expected, IEnumerable<T> actual)
  {
    if (!expected.SequenceEqual<T>(actual))
    {
      throw new ArgumentException($"Expected {string.Join(", ", expected)} does not equal Actual {string.Join(", ", actual)}");
    }
    Console.WriteLine("✅ Expected and Actual are the same");
  }
}
