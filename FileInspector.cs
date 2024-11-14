using Spectre.Console;
using static filemeta.PromptSpecifics;

namespace filemeta;

internal static class FileInspector
{
    // Opinionated format overriding user system setting
    public static string DateTimeFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";

    public static int Run(FileInfo fi)
    {
        var choices = GetAttributes(fi).ToArray();
        var choice = AnsiConsole.Prompt(new SelectionPrompt<FileAttribute>()
            .Title(Markup.Escape($"File {fi.Name} Attributes"))
            .AddChoices(choices)
            .UseConverter(x => $"{x.Key} {x.Value}")
            );

        switch (choice.Key)
        {
            case FileAttributes.FileName:
                var newFileName = PromptFileName(choice.Key.ToString(), choice.Value);
                var newFilePath = Path.Combine(fi.DirectoryName ?? throw new NotImplementedException("Non-directory file paths are not supported for rename"), newFileName);
                fi.MoveTo(newFilePath);
                var newFileInfo = new FileInfo(newFilePath);
                AnsiConsole.MarkupLineInterpolated($"Changed {nameof(newFileInfo.Name)} to {newFileInfo.Name}");
                break;
            case FileAttributes.CreationTime:
                fi.CreationTime = PromptDateTime(choice.Key.ToString(), choice.Value);
                AnsiConsole.MarkupLineInterpolated($"Changed {nameof(fi.CreationTime)} to {fi.CreationTime}");
                break;
            case FileAttributes.LastAccessTime:
                fi.LastAccessTime = PromptDateTime(choice.Key.ToString(), choice.Value);
                AnsiConsole.MarkupLineInterpolated($"Changed {nameof(fi.LastAccessTime)} to {fi.LastAccessTime}");
                break;
            case FileAttributes.LastWriteTime:
                fi.LastWriteTime = PromptDateTime(choice.Key.ToString(), choice.Value);
                AnsiConsole.MarkupLineInterpolated($"Changed {nameof(fi.LastWriteTime)} to {fi.LastWriteTime}");
                break;
            case FileAttributes.IsReadOnly:
                var newValue = PromptBool(choice.Key.ToString(), choice.Value);
                if (fi.IsReadOnly == newValue)
                {
                    AnsiConsole.MarkupLineInterpolated($"No change of {nameof(fi.IsReadOnly)}");
                    break;
                }

                fi.IsReadOnly = newValue;
                AnsiConsole.MarkupLineInterpolated($"Changed {nameof(fi.IsReadOnly)} to {fi.IsReadOnly}");
                break;
            default:
                throw new InvalidOperationException();
        }

        return 0;
    }

    private enum FileAttributes { FileName, CreationTime, LastAccessTime, LastWriteTime, IsReadOnly, }
    private sealed record FileAttribute(FileAttributes Key, string Value);

    private static IEnumerable<FileAttribute> GetAttributes(FileInfo fi)
    {
        yield return new(FileAttributes.FileName, Markup.Escape(fi.Name));
        yield return new(FileAttributes.CreationTime, fi.CreationTime.ToString(DateTimeFormat));
        yield return new(FileAttributes.LastAccessTime, fi.LastAccessTime.ToString(DateTimeFormat));
        yield return new(FileAttributes.LastWriteTime, fi.LastWriteTime.ToString(DateTimeFormat));
        yield return new(FileAttributes.IsReadOnly, fi.IsReadOnly ? "yes" : "no");
    }

    //public static IEnumerable<FileAttributes> GetAttributes2(FileInfo fi)
    //{
    //    yield return new FileStringAttribute("FileName", fi.Name, (fi, v) => fi.Name = v);
    //    yield return ("FileName", fi.Name);
    //    yield return ("CreationTime", fi.CreationTime.ToString(DateTimeFormat));
    //    yield return ("LastAccessTime", fi.LastAccessTime.ToString(DateTimeFormat));
    //    yield return ("LastWriteTime", fi.LastWriteTime.ToString(DateTimeFormat));
    //    yield return ("IsReadOnly", fi.IsReadOnly ? "yes" : "no");
    //}

    //public record FileStringAttribute(string Name, string Value, Action<FileInfo, string> Setter);
    //public record FileBoolAttribute(string Name, string Value, Action<FileInfo, bool> Setter);
    //public record FileDateTimeAttribute(string Name, string Value, Action<FileInfo, DateTime> Setter);

    public static IEnumerable<(string Name, string Value)> GetReadOnlyAttributes(FileInfo fi, bool includeUnset = false)
    {
        if (includeUnset || fi.LinkTarget != null) yield return ("LinkTarget", fi.LinkTarget ?? "<unset>");

        yield return ("Length", fi.Length.ToString());
        yield return ("UnixFileMode", fi.UnixFileMode.ToString());
    }
}
