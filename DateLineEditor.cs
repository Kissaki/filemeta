using System.Text;

namespace filemeta;

/// <summary>Date only yyyy-MM-dd line editor</summary>
public class DateLineEditor
{
    public static DateOnly ReadDateOnly(DateOnly defaultValue)
    {
        var edit = new DateLineEditor(defaultValue);
        edit.Enter();
        return edit.GetValue();
    }

    private static readonly CultureInfo _culture = CultureInfo.InvariantCulture;
    //                              0123456789
    private const string _format = "yyyy-MM-dd";
    private static string Format(DateOnly value) => value.ToString(_format, _culture);
    public static DateOnly Parse(string value) => DateOnly.ParseExact(value, _format, _culture);

    private string _model;
    private int _pos;

    public DateLineEditor(DateOnly value)
    {
        _model = Format(value);
    }

    public void SetValue(DateOnly value)
    {
        var was = Console.CursorLeft;
        _model = Format(value);
        Console.CursorLeft = 0;
        Console.Write(_model);
        Console.CursorLeft = was;
        // Implemented around SetPos _pos - must remain consistent
    }

    public DateOnly GetValue() => DateOnly.ParseExact(_model, _format, _culture);

    private enum Field { Year, Month, Day, }

    private static Range GetFieldRange(Field field) => field switch
    {
        Field.Year => 0..4,
        Field.Month => 5..7,
        Field.Day => 8..10,
        _ => throw new InvalidOperationException(),
    };

    private static Field GetIndexField(int index) => index switch
    {
        0 or 1 or 2 or 3 => Field.Year,
        5 or 6 => Field.Month,
        8 or 9 => Field.Day,
        _ => throw new InvalidOperationException(),
    };

    private void ShiftCurrentFieldValue(int diff)
    {
        switch (GetIndexField(_pos))
        {
            case Field.Year:
                SetValue(GetValue().AddYears(diff));
                break;
            case Field.Month:
                SetValue(GetValue().AddMonths(diff));
                break;
            case Field.Day:
                SetValue(GetValue().AddDays(diff));
                break;
            default:
                throw new InvalidOperationException();
        }
    }

    private void SetPos(int pos)
    {
        pos %= _format.Length;
        if (pos < 0) pos = _format.Length + pos;
        _pos = pos;
        Console.CursorLeft = _pos;
    }

    private void MovePos(int diff)
    {
        var newPos = (_pos, diff) switch
        {
            (3, 1) => 5,
            (6, 1) => 8,
            (5, -1) => 3,
            (8, -1) => 6,
            _ => _pos + diff,
        };

        SetPos(newPos);
    }

    public void Enter()
    {
        Console.Write($"{_model:yyyy-MM-dd}");
        SetPos(0);

        ConsoleKeyInfo input;
        do
        {
            input = Console.ReadKey(intercept: true);

            switch (input.Key)
            {
                case ConsoleKey.LeftArrow:
                    MovePos(-1);
                    break;
                case ConsoleKey.RightArrow:
                    MovePos(1);
                    break;
                case ConsoleKey.Tab:
                    HandleTab(input.Modifiers);
                    break;
                case >= ConsoleKey.D0 and <= ConsoleKey.D9 or >= ConsoleKey.NumPad0 and ConsoleKey.NumPad9:
                    WriteChar(input.KeyChar);
                    break;
                case ConsoleKey.Add or ConsoleKey.OemPlus or ConsoleKey.UpArrow:
                    ShiftCurrentFieldValue(GetShiftValue(input.Modifiers));
                    break;
                case ConsoleKey.Subtract or ConsoleKey.OemMinus or ConsoleKey.DownArrow:
                    ShiftCurrentFieldValue(-GetShiftValue(input.Modifiers));
                    break;
            }

        } while (input.Key != ConsoleKey.Enter);
        Console.WriteLine();
    }

    private void WriteChar(char keyChar)
    {
        var sb = new StringBuilder(_model);
        sb[_pos] = keyChar;
        _model = sb.ToString();
        Console.Write(keyChar);
        _pos += 1;
    }

    private int GetShiftValue(ConsoleModifiers modifiers) => modifiers switch { ConsoleModifiers.Shift => 10, ConsoleModifiers.Alt => 100, _ => 1, };

    private void HandleTab(ConsoleModifiers modifiers)
    {
        if (modifiers == ConsoleModifiers.Shift)
        {
            switch (Console.CursorLeft)
            {
                case 0 or 1 or 2 or 3 or 4:
                    SetPos(8);
                    break;
                case 5 or 6 or 7:
                    SetPos(0);
                    break;
                case 8 or 9 or > 9:
                    SetPos(5);
                    break;
            }
        }
        else
        {
            switch (Console.CursorLeft)
            {
                case 0 or 1 or 2 or 3 or 4:
                    SetPos(5);
                    break;
                case 5 or 6 or 7:
                    SetPos(8);
                    break;
                case 8 or 9 or > 9:
                    SetPos(0);
                    break;
            }
        }
    }
}
