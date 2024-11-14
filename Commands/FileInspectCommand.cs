using Spectre.Console.Cli;

namespace filemeta.Commands;

internal sealed class FileInspectCommand : AsyncCommand<FileInspectCommandSettings>
{
    //public static readonly ImmutableArray<string> EditableFileAttributes = [nameof(FileInfo.Name), nameof(FileInfo.create)];
    //public enum FileAttribute { Name, }

    public override Task<int> ExecuteAsync(CommandContext context, FileInspectCommandSettings settings)
    {
        /// File exists was validated in <see cref="FileInspectCommandSettings.Validate"/>
        var fi = new FileInfo(settings.FilePath);
        FileInspector.Run(fi);

        return Task.FromResult(0);
    }
}
