using System.Text;

namespace filemeta;

/// <summary>DateTime 'yyyy-MM-dd HH:mm:ss' line editor</summary>
public class DateTimeLineEditor
{
    public static DateTime Prompt(DateTime defaultValue)
    {
        var edit = new DateTimeLineEditor(defaultValue);
        edit.Enter();
        return edit.GetValue();
    }

    #region Internal Static Formatting
    private static readonly CultureInfo _culture = CultureInfo.InvariantCulture;
    //                              0123456789012345678
    private const string _format = "yyyy-MM-dd HH:mm:ss";
    private static string Format(DateTime value) => value.ToString(_format, _culture);
    private static DateTime Parse(string value) => DateTime.ParseExact(value, _format, _culture);
    #endregion Internal Static Formatting

    private DateTime _value;
    private string _valueText;
    private int _pos;

    private DateTimeLineEditor(DateTime value)
    {
        _valueText = Format(value);
    }

    public DateTime GetValue() => DateTime.ParseExact(_valueText, _format, _culture);

    public void SetValue(DateTime value)
    {
        if (_value == value) return;

        _value = value;
        SetValueText(Format(value));
    }

    private void SetValueText(string value)
    {
        if (_valueText == value) return;

        _valueText = value;

        var was = Console.CursorLeft;
        // We expect to render as a line editor from pos 0
        Console.CursorLeft = 0;
        Console.Write(_valueText);
        // Recover cursor position
        Console.CursorLeft = was;
        // Implemented around SetPos _pos - must remain consistent

        SetValue(Parse(_valueText));
    }

    private void SetChar(int index, char keyChar)
    {
        var sb = new StringBuilder(_valueText);
        sb[index] = keyChar;
        SetValueText(sb.ToString());
        MovePos(1);
    }

    private enum Field { Invalid, Separator, Year, Month, Day, Hour, Minute, Second, }
    private sealed record FieldRange(Field Field, Range Range);
    private static readonly FieldRange[] _fieldRanges = [
        new(Field.Year, 0..4),
        new(Field.Month, 5..7),
        new(Field.Day, 8..10),
        new(Field.Hour, 11..13),
        new(Field.Minute, 14..16),
        new(Field.Second, 17..19),
    ];

    private static Field GetIndexField(int index) => index switch
    {
        0 or 1 or 2 or 3 => Field.Year,
        5 or 6 => Field.Month,
        8 or 9 => Field.Day,
        11 or 12 => Field.Hour,
        14 or 15 => Field.Minute,
        17 or 18 => Field.Second,
        4 or 7 or 10 or 13 or 16 => Field.Separator,
        _ => Field.Invalid,
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
        var newPos = GetNextPos(diff);
        SetPos(newPos);
    }

    public void Enter()
    {
        Console.Write($"{_valueText:yyyy-MM-dd}");
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
                    SetChar(_pos, input.KeyChar);
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

    private int GetNextPos(int diff)
    {
        diff %= _format.Length;

        var direction = Math.Sign(diff);
        var newPos = _pos + diff;
        var fixedPos = newPos switch
        {
            < 0 => 18,
            > 18 => 0,
            0 or 1 or 2 or 3 or 5 or 6 or 8 or 9 or 11 or 12 or 14 or 15 or 17 or 18 => newPos,
            4 or 7 or 10 or 13 or 16 => newPos + direction,
        };

        return fixedPos;
    }

    private static int GetShiftValue(ConsoleModifiers modifiers) => modifiers switch { ConsoleModifiers.Shift => 10, ConsoleModifiers.Alt => 100, _ => 1, };

    private void HandleTab(ConsoleModifiers modifiers)
    {
        if (modifiers == ConsoleModifiers.Shift)
        {
            var prevPos = Console.CursorLeft switch
            {
                0 or 1 or 2 or 3 => 17,
                5 or 6 => 0,
                8 or 9 => 5,
                11 or 12 => 8,
                14 or 15 => 11,
                17 or 18 => 14,
                _ => throw new InvalidOperationException(),
            };
            SetPos(prevPos);
        }
        else
        {
            var nextPos = Console.CursorLeft switch
            {
                0 or 1 or 2 or 3 => 5,
                5 or 6 => 8,
                8 or 9 => 11,
                11 or 12 => 14,
                14 or 15 => 17,
                17 or 18 => 0,
                _ => throw new InvalidOperationException(),
            };
            SetPos(nextPos);
        }
    }
}
