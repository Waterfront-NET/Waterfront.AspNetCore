#load build/data/*.cake
#load build/tasks/*.cake

Setup(ctx => {
  EnsureDirectoryExists(paths.Libraries);
  EnsureDirectoryExists(paths.Packages);
});

RunTarget(args.Target);
