using Spectre.Console;
using Spectre.Console.Cli;

namespace filemeta.Commands;

internal class FileInspectCommandSettings : CommandSettings
{
    [CommandArgument(position: 0, template: "<filepath>")]
    public required string FilePath { get; set; }

    public override ValidationResult Validate()
    {
        return File.Exists(FilePath) ? ValidationResult.Success() : ValidationResult.Error($"File does not exist at {FilePath}");
    }
}
