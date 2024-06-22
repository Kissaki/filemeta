// See https://aka.ms/new-console-template for more information

using filemeta.Commands;

var app = new CommandApp<FileInspectCommand>();
return await app.RunAsync(args);
