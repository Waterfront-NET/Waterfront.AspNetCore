#load ../data/*.cake

Task("clean")
.Does(() => {
  projects.ForEach(project => {
    Verbose("Cleaning bin directory of project {0}", project.Name);
    CleanDirectory(project.Directory.Combine("bin"));
  });

  Verbose("Cleaning library artifacts directory");
  CleanDirectory(paths.Libraries);
  Verbose("Cleaning package artifacts directory");
  CleanDirectory(paths.Packages);
});
