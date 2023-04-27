#load ../data/*.cake
using System.Text.Json;

var mainRestoreTask = Task("restore");

foreach(var project in projects) {
  var task = Task(project.TaskName("restore")).Does(() => {
    DotNetRestore(project.Path.ToString(), new DotNetRestoreSettings {
      NoDependencies = true
    });
  });

  mainRestoreTask.IsDependentOn(task);
}

Task("list-projects").Does(() => {
  Information("Projects:\n{0}", JsonSerializer.Serialize(projects, new JsonSerializerOptions {WriteIndented = true}));
});
