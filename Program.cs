// See https://aka.ms/new-console-template for more information

using filemeta;
using filemeta.Commands;

//var date = DateLineEditor.Prompt(DateOnly.FromDateTime(DateTime.Now));
//Console.WriteLine($"Result: {date}");

var datetime = DateTimeLineEditor.Prompt(defaultValue: DateTime.Now);
Console.WriteLine($"Result: {datetime}");

var app = new CommandApp<FileInspectCommand>();
//app.Configure(c => c.PropagateExceptions());
return await app.RunAsync(args);
