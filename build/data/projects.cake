#addin nuget:?package=Cake.Incubator&version=8.0.0
#load paths.cake
#load args.cake

static List<BuildProject> projects;

projects = new(ParseSolution(paths.Solution).Projects
.Where(project => project.Type is "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}" /* cs project */)
.Select(project => {
  var fullProjectPath = project.Path.MakeAbsolute(paths.Solution.GetDirectory());
  Verbose("Parsing project {0}", fullProjectPath);
  var parsedProject = ParseProject(fullProjectPath, args.Configuration);
  var references = parsedProject.ProjectReferences.Select(reference => {
    var fullReferencePath = FilePath.FromString(reference.RelativePath).MakeAbsolute(fullProjectPath.GetDirectory());
    var referenceBp = new BuildProject {
      Name = fullReferencePath.GetFilenameWithoutExtension().ToString(),
      Path = fullReferencePath
    };
    return referenceBp;
  });


  var bp = new BuildProject {
    Name = project.Name,
    Path = fullProjectPath
  };
  bp.Dependencies.AddRange(references);

  return bp;
}));

class BuildProject {
  public string Name { get; init; }
  public string ShortName => Name.Replace("Waterfront.", string.Empty).ToLowerInvariant();
  public bool IsTest => Name.EndsWith(".Tests");
  public FilePath Path { get; init; }
  public DirectoryPath Directory => Path.GetDirectory();
  public List<BuildProject> Dependencies { get; } = new List<BuildProject>();

  public string TaskName(string task) {
    return $":{ShortName}:{task}";
  }
}
