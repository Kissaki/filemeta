namespace filemeta.Prompts;

/// <seealso cref="ConfirmationPrompt"/>
internal class BoolPrompt(string name) : IPrompt<bool>
{
    public string Name { get; set; } = name;
    public bool? DefaultValue { get; set; }

    private string GetPromptText() => DefaultValue switch
    {
        null => $"{Name} [y/n]: ",
        true => $"{Name} [Y/n]: ",
        false => $"{Name} [y/N]: ",
    };

    public bool Show(IAnsiConsole console)
    {
        console.Write(GetPromptText());
        while (true)
        {
            var keyInfo = console.Input.ReadKey(intercept: true);
            if (keyInfo is null) continue;

            switch (keyInfo.Value.KeyChar)
            {
                case 'y':
                    console.WriteLine("y");
                    return true;
                case 'n':
                    console.WriteLine("n");
                    return false;
                default:
                    if (DefaultValue.HasValue)
                    {
                        console.WriteLine(DefaultValue.Value ? "y" : "n");
                        return DefaultValue.Value;
                    }
                    // fall through into next loop iteration
                    break;
            }
        }
    }

    public Task<bool> ShowAsync(IAnsiConsole console, CancellationToken cancellationToken)
    {
        return console.ExclusivityMode.RunAsync(async () =>
        {
            console.Write(GetPromptText());
            while (true)
            {
                var keyInfo = await console.Input.ReadKeyAsync(intercept: true, cancellationToken);
                if (keyInfo is null) continue;

                switch (keyInfo.Value.KeyChar)
                {
                    case 'y':
                        console.WriteLine("y");
                        return true;
                    case 'n':
                        console.WriteLine("n");
                        return false;
                    default:
                        // fall through into next loop iteration
                        break;
                }
            }
        });
    }
}
