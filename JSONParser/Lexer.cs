namespace JSONParser;

public class Lexer : IDisposable
{
    private string Input { get; set; }
    private int CurrentPosition { get; set; }
    private char CurrentChar { get; set; }
    private readonly CharEnumerator _enumerator;

    public Lexer(string input)
    {
        Input = input;
        _enumerator = Input.GetEnumerator();
        CurrentPosition = 0;
        CurrentChar = Input[CurrentPosition];
    }

    void IDisposable.Dispose()
    {
        GC.SuppressFinalize(this);
        _enumerator.Dispose();
    }

    public char ReadChar()
    {
        var isNext = _enumerator.MoveNext();
        
        if (isNext)
        {
            CurrentChar = _enumerator.Current;
            CurrentPosition += 1;
        }
        else
        {
            return '\0';
        }

        return CurrentChar;
    }
}
