using System.IO;
using NUnit.Framework;
using static Bud.Building;
using static Bud.Exec;
using static Bud.TesterAppPath;

namespace Bud {
  public class BuildGlobToExtTaskTest {
    [Test]
    public void Build_produces_glob_to_ext_task() {
      using (var dir = new TmpDir()) {
        dir.CreateFile("  foo  ", "src", "foo.txt");
        dir.CreateFile("  bar  ", "src", "subdir", "bar.txt");

        var task = Build(command: ctx => ctx.Command(TesterApp, $"--rootDir {Arg(ctx.RootDir)} --outDir {Arg(ctx.OutputDir)} {Args(ctx.Sources)}"),
                         sourceDir: "src",
                         sourceExt: ".txt",
                         outputDir: "build",
                         outputExt: ".txt.nospace");

        RunBuild(task, stdout: new StringWriter(), baseDir: dir.Path);

        FileAssert.AreEqual(dir.CreateFile("foo", "foo.expected"),
                            dir.CreatePath("build", "foo.txt.nospace"));

        FileAssert.AreEqual(dir.CreateFile("bar", "bar.expected"),
                            dir.CreatePath("build", "subdir", "bar.txt.nospace"));
      }
    }

    [Test]
    public void Command_not_reinvoked() {
      using (var dir = new TmpDir()) {
        dir.CreateFile("  foo  ", "src", "foo.txt");
        var outputFile = dir.CreatePath("build", "foo.txt.nospace");

        RunBuild(TrimTxtFiles(), stdout: new StringWriter(), baseDir: dir.Path);
        var modificationTime = File.GetLastWriteTimeUtc(outputFile);
        RunBuild(TrimTxtFiles(), stdout: new StringWriter(), baseDir: dir.Path);

        Assert.AreEqual(modificationTime, File.GetLastWriteTimeUtc(outputFile));
      }
    }

    [Test]
    public void Command_reinvoked_on_change() {
      using (var dir = new TmpDir()) {
        dir.CreateFile("  foo  ", "src", "foo.txt");

        RunBuild(TrimTxtFiles(), stdout: new StringWriter(), baseDir: dir.Path);
        dir.CreateFile("  foo2  ", "src", "foo.txt");
        RunBuild(TrimTxtFiles(), stdout: new StringWriter(), baseDir: dir.Path);

        FileAssert.AreEqual(dir.CreateFile("foo2", "foo.expected"),
                            dir.CreatePath("build", "foo.txt.nospace"));
      }
    }

    [Test]
    public void Extraneous_files_deleted() {
      using (var dir = new TmpDir()) {
        var srcFile = dir.CreateFile("  foo  ", "src", "foo.txt");

        RunBuild(TrimTxtFiles(), stdout: new StringWriter(), baseDir: dir.Path);
        File.Delete(srcFile);
        RunBuild(TrimTxtFiles(), stdout: new StringWriter(), baseDir: dir.Path);

        FileAssert.DoesNotExist(dir.CreatePath("build", "foo.txt.nospace"));
      }
    }

    private static BuildGlobToExtTask TrimTxtFiles()
      => new BuildGlobToExtTask(
        command: ctx => ctx.Command(TesterApp, $"--rootDir {Arg(ctx.RootDir)} --outDir {Arg(ctx.OutputDir)} {Args(ctx.Sources)}"),
        sourceDir: "src",
        sourceExt: ".txt",
        outputDir: "build",
        outputExt: ".txt.nospace");
  }
}