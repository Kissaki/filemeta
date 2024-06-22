// See https://aka.ms/new-console-template for more information

using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Globalization;

var rootCommand = new RootCommand("Sample app for System.CommandLine");
var fileArg = new Argument<FileInfo?>(name: "file", description: "File path");
rootCommand.AddArgument(fileArg);

rootCommand.SetHandler((InvocationContext iv) =>
{
    var fi = iv.ParseResult.GetValueForArgument(fileArg);
    if (fi == null) return Task.FromResult(1);

    var cToken = iv.GetCancellationToken();
    // Opportunity: Use IConsole iv.Console - but does not support Read[Line] - so how?

    var err = OpenFileInfo(fi, cToken);

    return Task.FromResult(err ? 1 : 0);
});

return await rootCommand.InvokeAsync(args);

// Apparently built-in FileInfo
//static FileInfo? ParseFileArgument(ArgumentResult result)
//{
//    var value = result.Tokens.Single().Value;
//    var fi = new FileInfo(value);
//    return fi.Exists ? fi : null;
//}

/// <returns>success</returns>
bool OpenFileInfo(FileInfo fi, CancellationToken cToken1)
{
    if (fi.IsReadOnly)
    {
        WriteFileMeta(fi);
        Console.WriteLine("File is read-only. Metadata can not be changed.");
        return false;
    }

    using var cTokenSource = new CancellationTokenSource();
    var cToken = cTokenSource.Token;

    while (!cToken1.IsCancellationRequested || !cToken.IsCancellationRequested)
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

static void WriteFileMeta(FileInfo fi)
{
    Console.WriteLine($"File {fi.Name}:");
    Console.WriteLine($"  [C]reationTime  : {fi.CreationTime}");
    Console.WriteLine($"  Last[A]ccessTime: {fi.LastAccessTime}");
    Console.WriteLine($"  Last[W]riteTime : {fi.LastWriteTime}");
    Console.WriteLine($"  Information     :");
    Console.WriteLine($"    LinkTarget  : {fi.LinkTarget}");
    Console.WriteLine($"    IsReadOnly  : {fi.IsReadOnly}");
    Console.WriteLine($"    Length      : {fi.Length}");
    Console.WriteLine($"    UnixFileMode: {fi.UnixFileMode}");
}

static void ChangeProperty(FileInfo fi, Func<FileInfo, DateTime> get, Action<FileInfo, DateTime> set)
{
    var dtFormat = "yyyy-MM-dd HH:mm:ss";
    Console.WriteLine(get(fi).ToString(dtFormat));

    var line = Console.ReadLine();
    if (line == null) return;

    if (!DateTime.TryParseExact(line, dtFormat, provider: CultureInfo.InvariantCulture, DateTimeStyles.None, out var newValue))
    {
        Console.WriteLine("Input could not be read as a date and time. Invalid format?");
        return;
    }

    set(fi, newValue);
}
