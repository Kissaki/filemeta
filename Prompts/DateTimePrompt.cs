namespace filemeta.Prompts;

public class DateTimePrompt : IPrompt<DateTime>
{
    //                              0123456789012345678
    private const string _format = "yyyy-MM-dd HH:mm:ss";
    private static readonly CultureInfo _culture = CultureInfo.InvariantCulture;
    private static string Format(DateTime value) => value.ToString(_format, _culture);
    private static DateTime Parse(string value) => DateTime.ParseExact(value, _format, _culture);

    private enum Field { Year, Month, Day, Hour, Minute, Second, };

    public DateTime Value { get; init; }
    private string _model;

    public DateTime Show(IAnsiConsole console) => ShowAsync(console, CancellationToken.None).GetAwaiter().GetResult();

    public Task<DateTime> ShowAsync(IAnsiConsole console, CancellationToken cancellationToken)
    {
        if (console is null) throw new ArgumentNullException(nameof(console));

        console.RunExclusive(async () =>
        {

            ConsoleKeyInfo? input;
            do
            {
                var asString = Format(Value);
                _model = asString;
                console.Markup(asString);

                input = console.Input.ReadKey(intercept: true);
                console.Cursor.SetPosition()


            } while (input is not null && input.Value.Key != ConsoleKey.Enter);

            Value = Parse(_model);
        });
    }
}
