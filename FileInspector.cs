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
            var choicesStr = choices.Select(x => string.Join("", x.Name, x.Value)).ToArray();
            var choice = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title($"File {fi.Name} Attributes")
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
                    AnsiConsole.MarkupLineInterpolated($"Changed {nameof(fi.LastWriteTime)} to {fi.LastWriteTime}");
                    break;
                case 4:
                    var was4 = choices[choiceIndex];
                    fi.IsReadOnly = PromptBool(was4.Name, was4.Value);
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

        /// <returns>success</returns>
        [Obsolete]
        public static bool OpenFileInfo(FileInfo fi, CancellationToken? cToken1)
        {
            if (fi.IsReadOnly)
            {
                WriteFileMeta(fi);
                Console.WriteLine("File is read-only. Metadata can not be changed.");
                return false;
            }

            using var cTokenSource = new CancellationTokenSource();
            var cToken = cTokenSource.Token;

            while (!(cToken1?.IsCancellationRequested ?? false) || !cToken.IsCancellationRequested)
            {
                WriteFileMeta(fi);
                Console.WriteLine("[q]uit");
                Console.WriteLine("Use the marked keys to navigate...");
                var c = Console.ReadKey();
                Console.WriteLine();

                switch (c.KeyChar)
                {
                    case 'q':
                        cTokenSource.Cancel();
                        break;
                    case 'c':
                        ChangeProperty(fi, x => x.CreationTime, (x, v) => x.CreationTime = v);
                        break;
                    case 'a':
                        ChangeProperty(fi, x => x.LastAccessTime, (x, v) => x.LastAccessTime = v);
                        break;
                    case 'w':
                        ChangeProperty(fi, x => x.LastWriteTime, (x, v) => x.LastWriteTime = v);
                        break;
                    default:
                        // Break out into interactivity loop
                        break;
                }
            }

            return true;
        }

        [Obsolete]
        public static void WriteFileMeta(FileInfo fi)
        {
            AnsiConsole.MarkupInterpolated($"File {fi.Name}:");
            AnsiConsole.MarkupInterpolated($"  [C]reationTime  : {fi.CreationTime}");
            AnsiConsole.MarkupInterpolated($"  Last[A]ccessTime: {fi.LastAccessTime}");
            AnsiConsole.MarkupInterpolated($"  Last[W]riteTime : {fi.LastWriteTime}");
            AnsiConsole.MarkupInterpolated($"  Information     :");
            AnsiConsole.MarkupInterpolated($"    LinkTarget  : {fi.LinkTarget}");
            AnsiConsole.MarkupInterpolated($"    IsReadOnly  : {fi.IsReadOnly}");
            AnsiConsole.MarkupInterpolated($"    Length      : {fi.Length}");
            AnsiConsole.MarkupInterpolated($"    UnixFileMode: {fi.UnixFileMode}");
        }

        [Obsolete]
        public static void ChangeProperty(FileInfo fi, Func<FileInfo, DateTime> get, Action<FileInfo, DateTime> set)
        {
            Console.WriteLine(get(fi).ToString(DateTimeFormat));

            var line = Console.ReadLine();
            if (line == null) return;

            if (!DateTime.TryParseExact(line, DateTimeFormat, provider: CultureInfo.InvariantCulture, DateTimeStyles.None, out var newValue))
            {
                Console.WriteLine("Input could not be read as a date and time. Invalid format?");
                return;
            }

            set(fi, newValue);
        }
    }
}
