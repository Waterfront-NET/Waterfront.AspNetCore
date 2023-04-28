#load ../data/*.cake

var packages = GetFiles(paths.Packages.Combine("*.nupkg").ToString());

Task("artifacts/push/nuget").WithCriteria(packages.Any())
.DoesForEach(packages, package => {
  Information("Pushing package {0} to nuget.org package registry", package.GetFilename());
  DotNetNuGetPush(package, new DotNetNuGetPushSettings {
    ApiKey = apikeys.Nuget,
    WorkingDirectory = paths.Packages,
    Source = "nuget.org"
  });
});

Task("artifacts/push/github").WithCriteria(packages.Any())
.DoesForEach(packages, package => {
  Information("Pushing package {0} to github.com package registry", package.GetFilename());
  throw new NotImplementedException();
});
