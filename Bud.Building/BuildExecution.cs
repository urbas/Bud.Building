using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Bud {
  internal static class BuildExecution {
    private const string BuildMetaDirName = ".bud";
    
    internal static void RunBuild(IEnumerable<BuildTask> tasks,
                                  TextWriter stdout,
                                  string baseDir,
                                  string metaDir) {
      var buildTasks = tasks as IList<BuildTask> ?? tasks.ToList();
      var buildStopwatch = new Stopwatch();
      buildStopwatch.Start();
      var taskNumberAssigner = new BuildTaskNumberAssigner(CountTasks(buildTasks));
      baseDir = baseDir ?? Directory.GetCurrentDirectory();
      metaDir = metaDir ?? Path.Combine(baseDir, BuildMetaDirName);
      var task2TaskGraphs = new Dictionary<BuildTask, TaskGraph>();
      var builtTaskGraphs = buildTasks.Select(task => ToTaskGraph(task2TaskGraphs, stdout, task, taskNumberAssigner,
                                                                  buildStopwatch, baseDir, metaDir));
      new TaskGraph(builtTaskGraphs).Run();
    }

    private static int CountTasks(ICollection<BuildTask> tasks)
      => tasks.Count + tasks.Select(task => CountTasks(task.Dependencies)).Sum();

    private static TaskGraph ToTaskGraph(IDictionary<BuildTask, TaskGraph> task2TaskGraphs,
                                         TextWriter stdout,
                                         BuildTask buildTask,
                                         BuildTaskNumberAssigner buildTaskNumberAssigner,
                                         Stopwatch buildStopwatch,
                                         string baseDir,
                                         string metaDir) {
      TaskGraph taskGraph;
      return task2TaskGraphs.TryGetValue(buildTask, out taskGraph) ?
               taskGraph :
               CreateTaskGraph(task2TaskGraphs, stdout, buildTask, buildTaskNumberAssigner, buildStopwatch, baseDir,
                               metaDir);
    }

    private static TaskGraph CreateTaskGraph(IDictionary<BuildTask, TaskGraph> task2TaskGraphs,
                                             TextWriter stdout,
                                             BuildTask buildTask,
                                             BuildTaskNumberAssigner buildTaskNumberAssigner,
                                             Stopwatch buildStopwatch,
                                             string baseDir,
                                             string metaDir) {
      var taskGraphs = buildTask.Dependencies
                                .Select(dependencyBuildTask => ToTaskGraph(task2TaskGraphs, stdout, dependencyBuildTask,
                                                                           buildTaskNumberAssigner, buildStopwatch, baseDir,
                                                                           metaDir))
                                .ToImmutableArray();
      var buildContext = new BuildContext(stdout, buildStopwatch, buildTaskNumberAssigner.AssignNumber(),
                                          buildTaskNumberAssigner.TotalTasks, baseDir);
      var taskGraph = new TaskGraph(() => buildTask.Execute(buildContext), taskGraphs);
      task2TaskGraphs.Add(buildTask, taskGraph);
      return taskGraph;
    }
    
  }
}