namespace filemeta;

internal static class PromptSpecifics
{
    internal static string PromptFileName(string name, string currentValue)
    {
        return AnsiConsole.Prompt(
            new TextPrompt<string>($"Replace [teal]{name}[/] [yellow]{currentValue}[/]")
            .PromptStyle("green")
            );
    }

    internal static bool PromptBool(string name, string currentValue)
    {
        var defaultValue = currentValue == "yes";
        return AnsiConsole.Prompt(new BoolPrompt(name) { DefaultValue = defaultValue });
    }

    internal static DateTime PromptDateTime(string name, string currentValue)
    {
        string[] acceptedFormats = ["HH:mm:ss", "yyyy-MM-dd", FileInspector.DateTimeFormat];

        var unparsed = AnsiConsole.Prompt(
            new TextPrompt<string>($"Replace [teal]{name}[/] [yellow]{currentValue}[/]")
            .PromptStyle("green")
            .ValidationErrorMessage("Invalid date time format")
            .Validate(x =>
            {
                return Array.Exists(acceptedFormats, f => DateTime.TryParseExact(x, f, CultureInfo.InvariantCulture, DateTimeStyles.None, out _));
            })
            );

        foreach (var format in acceptedFormats)
        {
            if (DateTime.TryParseExact(unparsed, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var value)) return value;
        }

        throw new InvalidOperationException();
    }
}
