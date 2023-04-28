#load build/data/*.cake
#load build/tasks/*.cake

Setup(ctx => {
  EnsureDirectoryExists(paths.Libraries);
  EnsureDirectoryExists(paths.Packages);

  Information("Setting version environment variables...");
  Environment.SetEnvironmentVariable("SEMVER", version.SemVer);
  Environment.SetEnvironmentVariable("INFO_VER", version.InformationalVersion);
});

RunTarget(args.Target);
