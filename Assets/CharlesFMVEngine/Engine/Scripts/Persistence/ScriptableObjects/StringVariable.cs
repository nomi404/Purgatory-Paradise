using CharlesEngine;

public class StringVariable : Variable<string>
{
    public bool Empty()
    {
        return string.IsNullOrEmpty(RuntimeValue);
    }
}
