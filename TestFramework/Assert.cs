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
    }

    public static void IsNull(object actual)
    {
        if (actual != null)
        {
            throw new ArgumentException($"Expected {actual.GetType().Name} to be null, got {actual.GetType()}");
        }
    }

    public static void IsNotNull(object actual)
    {
        if (actual == null)
        {
            throw new ArgumentException($"Expected {actual?.GetType().Name} to be not null");
        }
    }

    public static TException ThrowsException<TException>(Action action) where TException : Exception
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            if (ex is TException exception)
            {
                return exception;
            }
            else
            {

                throw new ArgumentException($"Expected exception of type {typeof(TException).Name} to be thrown but exception {ex.GetType().Name} as thrown", ex);
            }

        }

        throw new ArgumentException($"Expected exception {typeof(TException).Name} to be thrown but exception was not thrown");
    }
}
