#load ../data/*.cake

var mainPackTask = Task("pack");

foreach(var project in projects.Where(x => !x.IsTest)) {
  var task = Task(project.TaskName("pack"))
  .IsDependentOn(project.TaskName("build"))
  .Does(() => {
    DotNetPack(project.Path.ToString(), new DotNetPackSettings {
      Configuration = args.Configuration,
      NoBuild = true
    });

    if(args.Configuration is "Release" && !args.NoCopyArtifacts) {
      GetFiles(
        project.Directory.Combine("bin/Release")
                         .Combine("*.{nupkg,snupkg}")
                         .ToString()
      ).ToList().ForEach(package => {
        Verbose("Copying package {0} to artifacts folder", package);
        CopyFileToDirectory(package, paths.Packages);
      });
    }
  });

  mainPackTask.IsDependentOn(task);
}
