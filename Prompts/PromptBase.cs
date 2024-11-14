namespace filemeta.Prompts;

internal abstract class PromptBase
{
    private int _pos;

    public int Length { get; private set; }

    internal abstract void HandleInput(ConsoleKeyInfo input);

    protected void Render() => Render(_pos);

    protected abstract void Render(int pos);

    protected PromptBase(int pos, int length)
    {
        _pos = pos;
        Length = length;
    }

    public event EventHandler? LengthChanged;
    public event EventHandler? PosChanged;

    public void SetPosition(int pos)
    {
        _pos = pos;
        PosChanged?.Invoke(this, new());
    }

    protected void SetLength(int value)
    {
        Length = value;
        LengthChanged?.Invoke(this, new());
    }
}
