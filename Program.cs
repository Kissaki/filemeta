// See https://aka.ms/new-console-template for more information

using filemeta.Commands;

var app = new CommandApp<FileInspectCommand>();
app.Configure(c => c.PropagateExceptions());
return await app.RunAsync(args);
