namespace EngineKit;

public readonly struct Label
{
    public static readonly Label Empty = new Label(string.Empty);

    private readonly string _value;

    public Label(string value)
    {
        _value = value;
    }

    public override string ToString()
    {
        return _value;
    }

    public static implicit operator Label(string label)
    {
        return new Label(label);
    }

    public static implicit operator string(Label label)
    {
        return label._value;
    }
}