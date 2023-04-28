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

  DotNetNuGetPush(package, new DotNetNuGetPushSettings {
    ApiKey = apikeys.Github,
    WorkingDirectory = paths.Packages,
    Source = "github.com"
  });
}).IsDependentOn("artifacts/push/github/setup-source");

Task("artifacts/push/github/setup-source").Does(() => {

  if(DotNetNuGetHasSource("github.com")) return;

  DotNetNuGetAddSource("github.com", new DotNetNuGetSourceSettings {
    Source = "https://nuget.pkg.github.com/Waterfront-NET/index.json",
    UserName = "USERNAME",
    Password = apikeys.Github,
    StorePasswordInClearText = true
  });
});
