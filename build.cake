#load build/data/*.cake
#load build/tasks/*.cake

Setup(ctx => {
  EnsureDirectoryExists(paths.Libraries);
  EnsureDirectoryExists(paths.Packages);

  Information("Setting version environment variables...");
  Environment.SetEnvironmentVariable("GitVersion_SemVer", version.SemVer);
  Environment.SetEnvironmentVariable("GitVersion_InformationalVersion", version.InformationalVersion);
});

RunTarget(args.Target);
