using System.Text;

namespace filemeta;

/// <summary>DateTime 'yyyy-MM-dd HH:mm:ss' line editor</summary>
public class DateTimeLineEditor
{
    public static DateTime Prompt(DateTime defaultValue)
    {
        var edit = new DateTimeLineEditor(defaultValue, Console.CursorLeft);
        edit.EnterEditor();
        return edit.GetValue();
    }

    #region Internal Static Formatting
    private static readonly CultureInfo _culture = CultureInfo.InvariantCulture;
    //                              0123456789012345678
    private const string _format = "yyyy-MM-dd HH:mm:ss";
    private static string Format(DateTime value) => value.ToString(_format, _culture);
    private static DateTime Parse(string value) => DateTime.ParseExact(value, _format, _culture);
    #endregion Internal Static Formatting

    // It may be a better idea to have a model of split field values - maybe even split field types that individually handle editing
    private DateTime _value;
    private string _valueText;
    private readonly int _editorPos;
    private int _cursorPos;

    private DateTimeLineEditor(DateTime value, int editorPosition)
    {
        _valueText = Format(value);
        _editorPos = editorPosition;
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

        RenderValue();

        SetValue(Parse(_valueText));
    }

    private void RenderValue()
    {
        using var cursorRecover = new CursorPosRecovery();

        Console.CursorLeft = _editorPos;
        Console.Write(_valueText);
    }

    private void SetChar(int index, char keyChar)
    {
        var sb = new StringBuilder(_valueText);
        sb[index] = keyChar;
        SetValueText(sb.ToString());
        MovePos(1);
    }

    private enum EditorField { Invalid, Separator, Year, Month, Day, Hour, Minute, Second, }
    //private sealed record FieldRange(Field Field, Range Range);
    //private static readonly FieldRange[] _fieldRanges = [
    //    new(Field.Year, 0..4),
    //    new(Field.Month, 5..7),
    //    new(Field.Day, 8..10),
    //    new(Field.Hour, 11..13),
    //    new(Field.Minute, 14..16),
    //    new(Field.Second, 17..19),
    //];

    #region Movement

    private void SetPos(int pos)
    {
        pos %= _format.Length;
        if (pos < 0) pos = _format.Length + pos;
        _cursorPos = pos;
        Console.CursorLeft = _cursorPos;
    }

    private void MovePos(int diff)
    {
        var newPos = GetNextPos(diff);
        SetPos(newPos);
    }

    private int GetNextPos(int diff)
    {
        diff %= _format.Length;

        var direction = Math.Sign(diff);
        var newPos = _cursorPos + diff;
        var fixedPos = newPos switch
        {
            < 0 => 18,
            > 18 => 0,
            0 or 1 or 2 or 3 or 5 or 6 or 8 or 9 or 11 or 12 or 14 or 15 or 17 or 18 => newPos,
            4 or 7 or 10 or 13 or 16 => newPos + direction,
        };

        return fixedPos;
    }

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

    #endregion Movement

    #region Field Value Modification

    private void ShiftCurrentFieldValue(int diff)
    {
        switch (GetIndexField(_cursorPos))
        {
            case EditorField.Year:
                SetValue(GetValue().AddYears(diff));
                break;
            case EditorField.Month:
                SetValue(GetValue().AddMonths(diff));
                break;
            case EditorField.Day:
                SetValue(GetValue().AddDays(diff));
                break;
            case EditorField.Hour:
                SetValue(GetValue().AddHours(diff));
                break;
            case EditorField.Minute:
                SetValue(GetValue().AddMinutes(diff));
                break;
            case EditorField.Second:
                SetValue(GetValue().AddSeconds(diff));
                break;
            default:
                throw new InvalidOperationException();
        }
    }

    #endregion Field Value Modification

    private static EditorField GetIndexField(int index) => index switch
    {
        0 or 1 or 2 or 3 => EditorField.Year,
        5 or 6 => EditorField.Month,
        8 or 9 => EditorField.Day,
        11 or 12 => EditorField.Hour,
        14 or 15 => EditorField.Minute,
        17 or 18 => EditorField.Second,
        4 or 7 or 10 or 13 or 16 => EditorField.Separator,
        _ => EditorField.Invalid,
    };

    #region Input Handling

    public void EnterEditor()
    {
        Console.Write(_valueText);
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
                case >= ConsoleKey.D0 and <= ConsoleKey.D9 or >= ConsoleKey.NumPad0 and <= ConsoleKey.NumPad9:
                    SetChar(_cursorPos, input.KeyChar);
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

    private static int GetShiftValue(ConsoleModifiers modifiers) => modifiers switch { ConsoleModifiers.Shift => 10, ConsoleModifiers.Alt => 100, _ => 1, };

    #endregion Input Handling
}
