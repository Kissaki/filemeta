using filemeta;

//var date = DateLineEditor.Prompt(DateOnly.FromDateTime(DateTime.Now));
//Console.WriteLine($"Result: {date}");

var datetime = DateTimeLineEditor.Prompt(defaultValue: DateTime.Now);
Console.WriteLine($"Result: {datetime}");

//var app = new Spectre.Console.Cli.CommandApp<filemeta.SpectreIntegrations.Commands.FileInspectCommand>();
//app.Configure(c => c.PropagateExceptions());
//return await app.RunAsync(args);
