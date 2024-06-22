using static filemeta.PromptSpecifics;

namespace filemeta
{
    internal static class FileInspector
    {
        // Opinionated format overriding user system setting
        public static string DateTimeFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";

        public static int Run(FileInfo fi)
        {
            var choices = GetAttributes(fi).ToArray();
            var choicesStr = choices.Select(x => string.Join(" ", x.Name, x.Value)).ToArray();
            var choice = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title($"File {fi.Name} Attributes")
                .AddChoices(choicesStr)
                );
            var choiceIndex = Array.FindIndex(choicesStr, x => x == choice);

            switch (choiceIndex)
            {
                case 0:
                    var was0 = choices[choiceIndex];
                    var newFileName = PromptFileName(was0.Name, was0.Value);
                    var newFilePath = Path.Combine(fi.DirectoryName ?? throw new NotImplementedException("Non-directory file paths are not supported for rename"), newFileName);
                    fi.MoveTo(newFilePath);
                    var newFileInfo = new FileInfo(newFilePath);
                    AnsiConsole.MarkupLineInterpolated($"Changed {nameof(newFileInfo.Name)} to {newFileInfo.Name}");
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
                    AnsiConsole.MarkupLineInterpolated($"Changed {nameof(fi.LastWriteTime)} to {fi.LastWriteTime}");
                    break;
                case 4:
                    var was4 = choices[choiceIndex];
                    var newValue = PromptBool(was4.Name, was4.Value);
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

        public static IEnumerable<(string Name, string Value)> GetAttributes(FileInfo fi)
        {
            yield return ("FileName", fi.Name);
            yield return ("CreationTime", fi.CreationTime.ToString(DateTimeFormat));
            yield return ("LastAccessTime", fi.LastAccessTime.ToString(DateTimeFormat));
            yield return ("LastWriteTime", fi.LastWriteTime.ToString(DateTimeFormat));
            yield return ("IsReadOnly", fi.IsReadOnly ? "yes" : "no");
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
}
