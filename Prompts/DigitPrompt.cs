namespace filemeta.Prompts;

internal sealed class DigitPrompt : PromptBase
{
    public class ValueOverflowEventArgs(int _overflow) : EventArgs
    {
        public int Overflow { get; } = _overflow;
    }

    public event EventHandler<ValueOverflowEventArgs>? Overflow;
    public event EventHandler<ValueOverflowEventArgs>? Underflow;
    public event EventHandler<int>? ValueChanged;

    public byte Value { get => _value; private set { _value = value; ValueChanged?.Invoke(this, _value); } }

    public DigitPrompt(int pos, byte value) : base(pos, length: 1)
    {
        _value = value;
        Render();
    }

    private byte _value;

    internal override void HandleInput(ConsoleKeyInfo input)
    {
        switch (input.Key)
        {
            case ConsoleKey.UpArrow:
                ShiftValue(1);
                break;
            case ConsoleKey.DownArrow:
                ShiftValue(-1);
                break;
        }
    }

    protected override void Render(int pos)
    {
        using CursorPosRecovery cursorPosRecovery = new();
        Console.CursorLeft = pos;
        Console.Write($"{_value:0}");
    }


    /// <param name="shift">One of 0, 1, -1</param>
    private void ShiftValue(int shift)
    {
        var newValue = _value + shift;

        if (newValue < 0)
        {
            Value = 9;
            Underflow?.Invoke(this, new(newValue));
        }
        else if (newValue > 9)
        {
            Value = 0;
            Overflow?.Invoke(this, new(newValue));
        }
        else
        {
            Value = (byte)newValue;
        }

        Render();
    }
}
