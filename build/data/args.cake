static BuildArguments args;
args = new BuildArguments(Context);

class BuildArguments {
  private readonly ICakeContext _context;

  public string Target => _context.Argument("target", _context.Argument("t", string.Empty));
  public string Configuration => _context.Argument("configuration", _context.Argument("c", "Debug"));
  public bool NoBuild => _context.HasArgument("no-build");
  public bool NoCopyArtifacts => _context.HasArgument("no-copy-artifacts");

  public BuildArguments(ICakeContext context) {
    _context = context;
  }
}
