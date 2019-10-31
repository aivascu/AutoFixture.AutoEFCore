#tool "nuget:?package=xunit.runner.console&version=2.4.1"
#tool "nuget:?package=OpenCover&version=4.6.519"
#tool "nuget:?package=ReportGenerator&version=4.3.3"
#tool "nuget:?package=GitVersion.CommandLine&version=5.1.1"

#addin "nuget:?package=Cake.Coverlet&version=2.3.4"

#load "build/paths.cake"

var target = Argument("target", "Build");
var configuration = Argument("configuration", "Release");
var packageVersion = "0.1.0";
var reportTypes = "HtmlInline_AzurePipelines";
var solutionPath = "EntityFrameworkCore.AutoFixture.sln";
var cleanupSettings = new DeleteDirectorySettings {
   Recursive = true,
   Force = true
};

Setup(context => {
   if(DirectoryExists(Paths.CoverageDir))
   {
      DeleteDirectory(Paths.CoverageDir, cleanupSettings);
      Verbose("Removed coverage folder");
   }

   var binDirs = GetDirectories(Paths.BinPattern);
   if(binDirs.Count > 0)
   {
      DeleteDirectories(binDirs, cleanupSettings);
      Verbose("Removed {0} \"bin\" directories", binDirs.Count);
   }

   var objDirs = GetDirectories(Paths.ObjPattern);
   if (objDirs.Count > 0)
   {
      DeleteDirectories(objDirs, cleanupSettings);
      Verbose("Removed {0} \"obj\" directories", objDirs.Count);
   }

   var testResultsDirs = GetDirectories(Paths.TestResultsPattern);
   if(testResultsDirs.Count > 0)
   {
      DeleteDirectories(testResultsDirs, cleanupSettings);
      Verbose("Removed {0} \"TestResults\" directories", testResultsDirs.Count);
   }

   var artifactDir = GetDirectories(Paths.ArtifactsPattern);
   if(artifactDir.Count > 0)
   {
      DeleteDirectories(artifactDir, cleanupSettings);
      Verbose("Removed {0} artifact directories", artifactDir.Count);
   }
});

// TASKS

Task("Clean")
   .Does(() => {
      DeleteDirectory(Paths.CoverageDir, cleanupSettings);
   });

Task("Restore")
   .IsDependentOn("Version")
   .Does(() => DotNetCoreRestore());

Task("Build")
   .IsDependentOn("Restore")
   .Does(() => {
      DotNetCoreBuild(
         Paths.SolutionFile.ToString(),
         new DotNetCoreBuildSettings
         { 
            Configuration = configuration,
            ArgumentCustomization = args => args.Append($"/p:Version={packageVersion}")
         });
   });

Task("Test")
   .IsDependentOn("Restore")
   .Does(() => {
      EnsureDirectoryExists(Paths.CoverageDir);
      var testSettings = new DotNetCoreTestSettings {
         ResultsDirectory = Directory("TestResults"),
         ArgumentCustomization = args => args.Append($"--logger trx")
      };
      var coverletSettings = new CoverletSettings {
         CollectCoverage = true,
         CoverletOutputDirectory = Paths.CoverageDir,
         CoverletOutputFormat = CoverletOutputFormat.cobertura,
         CoverletOutputName = $"{Guid.NewGuid().ToString("D")}.cobertura.xml"
      };
      DotNetCoreTest(Paths.TestProjectDirectory, testSettings, coverletSettings);
   });

Task("Report")
   .IsDependentOn("Test")
   .Does(() => {
      var reportSettings = new ReportGeneratorSettings {
         ArgumentCustomization = args => args.Append($"-reportTypes:{reportTypes}")
      };

      ReportGenerator("./coverage/*.xml", Paths.CoverageDir, reportSettings);
   });

Task("Version")
   .Does(() => {
      var version = GitVersion();
      Information($"Calculated semantic version: {version.SemVer}");

      packageVersion = version.NuGetVersion;
      Information($"Corresponding package version: {packageVersion}");

      if(BuildSystem.IsLocalBuild)
      {
         return;
      }

      packageVersion = GitVersion(new GitVersionSettings {
         OutputType = GitVersionOutput.BuildServer,
         UpdateAssemblyInfo = false
      }).NuGetVersionV2.Replace("unstable", "preview");
      Information($"Determined build server version: {packageVersion}");
   });

Task("Package")
   .IsDependentOn("Build")
   .Does(() => {
      EnsureDirectoryExists("./artifacts");
      var projects = ParseSolution(solutionPath).Projects
      .Where(p => p.GetType() != typeof(SolutionFolder) && !p.Name.EndsWith("Tests"));

      foreach(var project in projects)
      {
         Information($"Packaging project {project.Name}...");
         DotNetCorePack(project.Path.ToString(), new DotNetCorePackSettings {
            Configuration = configuration,
            OutputDirectory = Directory("./artifacts/"),
            ArgumentCustomization = args => args.Append($"/p:Version={packageVersion}")
         });
      }
   });

RunTarget(target);