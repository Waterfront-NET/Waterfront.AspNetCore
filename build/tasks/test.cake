#load ../data/*.cake

var mainTestTask = Task("test");

foreach(var project in projects.Where(p => p.IsTest)) {
  var task = Task(project.TaskName("test")).Does(() => {
    DotNetTest(project.Path.ToString(), new DotNetTestSettings {
      Configuration = args.Configuration,
      NoBuild = true
    });
  }).IsDependentOn(project.TaskName("build"));

  mainTestTask.IsDependentOn(task);
}
