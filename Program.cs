using filemeta;
using filemeta.Commands;

//var date = DateLineEditor.Prompt(DateOnly.FromDateTime(DateTime.Now));
//Console.WriteLine($"Result: {date}");

var datetime = DateTimeLineEditor.Prompt(defaultValue: DateTime.Now);
Console.WriteLine($"Result: {datetime}");

var app = new Spectre.Console.Cli.CommandApp<FileInspectCommand>();
//app.Configure(c => c.PropagateExceptions());
return await app.RunAsync(args);
