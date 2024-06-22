namespace filemeta.Commands;

internal sealed class FileInspectCommand : AsyncCommand<FileInspectCommandSettings>
{
    //public static readonly ImmutableArray<string> EditableFileAttributes = [nameof(FileInfo.Name), nameof(FileInfo.create)];
    //public enum FileAttribute { Name, }

    public override Task<int> ExecuteAsync(CommandContext context, FileInspectCommandSettings settings)
    {
        var fi = new FileInfo(settings.FilePath);
        AnsiConsole.MarkupLineInterpolated($"File {fi.Name}");

        var choices = FileInspector.GetAttributes(fi).ToArray();
        var choicesStr = choices.Select(x => string.Join("", x.Name, x.Value)).ToArray();
        var choice = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("Attributes")
            .AddChoices(choicesStr)
            );
        var choiceIndex = Array.FindIndex(choicesStr, x => x == choice);

        switch (choiceIndex)
        {
            case 0:
                var was0 = choices[choiceIndex];
                fi.MoveTo(PromptFileName(was0.Name, was0.Value));
                break;
            case 1:
                var was1 = choices[choiceIndex];
                fi.CreationTime = PromptDateTime(was1.Name, was1.Value);
                AnsiConsole.MarkupLineInterpolated($"Changed {nameof(fi.CreationTime)} to {fi.CreationTime}");
                break;
            case 2:
                var was2 = choices[choiceIndex];
                fi.LastAccessTime = PromptDateTime(was2.Name, was2.Value);
                AnsiConsole.MarkupLineInterpolated($"Changed {nameof(fi.LastAccessTime)} to {fi.LastAccessTime}");
                break;
            case 3:
                var was3 = choices[choiceIndex];
                fi.LastWriteTime = PromptDateTime(was3.Name, was3.Value);
                break;
            default:
                // TODO throw error
                return Task.FromResult(1);
        }

        return Task.FromResult(0);
    }

    private static string PromptFileName(string name, string currentValue)
    {
        return AnsiConsole.Prompt(
            new TextPrompt<string>($"Replace [teal]{name}[/] [yellow]{currentValue}[/]")
            .PromptStyle("green")
            );
    }

    private static DateTime PromptDateTime(string name, string currentValue)
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
