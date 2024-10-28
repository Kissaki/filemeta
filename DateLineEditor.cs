using System.Text;

namespace filemeta;

/// <summary>Date only yyyy-MM-dd line editor</summary>
public class DateLineEditor
{
    public static DateOnly Prompt(DateOnly initValue)
    {
        var edit = new DateLineEditor(initValue, Console.CursorLeft);
        edit.EnterEditor();
        return edit.GetValue();
    }

    private static readonly CultureInfo _culture = CultureInfo.InvariantCulture;
    //                              0123456789
    private const string _format = "yyyy-MM-dd";
    private static string Format(DateOnly value) => value.ToString(_format, _culture);
    private static DateOnly Parse(string value) => DateOnly.ParseExact(value, _format, _culture);

    private string _model;
    private readonly int _editorPos;
    /// <summary>Cursor position</summary>
    private int _cursorPos;

    private DateLineEditor(DateOnly value, int editorPosition)
    {
        _model = Format(value);
        _editorPos = editorPosition;
    }

    public void SetValue(DateOnly value)
    {
        _model = Format(value);
        RenderValue();
    }

    private void RenderValue()
    {
        using var cursorRecover = new CursorPosRecovery();

        Console.CursorLeft = _editorPos;
        Console.Write(_model);
    }

    public DateOnly GetValue() => DateOnly.ParseExact(_model, _format, _culture);

    private enum EditorField { Year, Month, Day, }

    private EditorField GetCurrentField() => GetField(_cursorPos);

    private static EditorField GetField(int pos) => pos switch
    {
        0 or 1 or 2 or 3 => EditorField.Year,
        5 or 6 => EditorField.Month,
        8 or 9 => EditorField.Day,
        _ => throw new InvalidOperationException(),
    };

    #region Movement

    private void MovePos(int diff)
    {
        var newPos = (_cursorPos, diff) switch
        {
            (3, 1) => 5,
            (6, 1) => 8,
            (5, -1) => 3,
            (8, -1) => 6,
            _ => _cursorPos + diff,
        };

        SetPos(newPos);
    }

    private void SetPos(int pos)
    {
        pos %= _format.Length;
        if (pos < 0) pos = _format.Length + pos;
        _cursorPos = pos;
        Console.CursorLeft = _cursorPos;
    }

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

    #endregion Movement

    //private static Range GetFieldRange(EditorField field) => field switch
    //{
    //    EditorField.Year => 0..4,
    //    EditorField.Month => 5..7,
    //    EditorField.Day => 8..10,
    //    _ => throw new InvalidOperationException(),
    //};

    private void AddCurrentFieldValue(int diff)
    {
        switch (GetCurrentField())
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
            default:
                throw new InvalidOperationException();
        }
    }

    public void EnterEditor()
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
                    AddCurrentFieldValue(GetShiftValue(input.Modifiers));
                    break;
                case ConsoleKey.Subtract or ConsoleKey.OemMinus or ConsoleKey.DownArrow:
                    AddCurrentFieldValue(-GetShiftValue(input.Modifiers));
                    break;
            }
        } while (input.Key != ConsoleKey.Enter);
        Console.WriteLine();
    }

    private void WriteChar(char keyChar)
    {
        var sb = new StringBuilder(_model);
        sb[_cursorPos] = keyChar;
        _model = sb.ToString();
        Console.Write(keyChar);
        _cursorPos += 1;
    }

    private static int GetShiftValue(ConsoleModifiers modifiers) => modifiers switch { ConsoleModifiers.Shift => 10, ConsoleModifiers.Alt => 100, _ => 1, };
}
