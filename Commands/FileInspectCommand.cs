using Spectre.Console;
using static filemeta.Commands.FileInspectCommand;

namespace filemeta.Commands
{
    internal sealed class FileInspectCommand : AsyncCommand<Settings>
    {
        public sealed class Settings : CommandSettings
        {
            [CommandArgument(position: 0, template: "<filepath>")]
            public required FileInfo FileInfo { get; set; }

            public override ValidationResult Validate()
            {
                return FileInfo.Exists ? ValidationResult.Success() : ValidationResult.Error("File does not exist");
            }
        }

        public override Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            var err = FileInspector.OpenFileInfo(settings.FileInfo, null);
            return Task.FromResult(err ? 1 : 0);
        }
    }
}
