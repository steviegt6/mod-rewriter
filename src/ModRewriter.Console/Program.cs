using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using ModRewriter.Core.Impl;
using SysConsole = System.Console;

namespace ModRewriter.Console
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            SysConsole.WriteLine($"Working directory: {Directory.GetCurrentDirectory()}");
            SysConsole.WriteLine($"Launch arguments: {string.Join(",", args)}");

            string? projPath = GetArgument("--project-path", args);
            string? threadCount = GetArgument("--thread-count", args);

            using MSBuildWorkspace workspace = MSBuildWorkspace.Create();

            workspace.WorkspaceFailed += (sender, eventArgs) =>
            {
                SysConsole.ForegroundColor = eventArgs.Diagnostic.Kind == WorkspaceDiagnosticKind.Warning 
                    ? ConsoleColor.Yellow 
                    : ConsoleColor.Red;

                string display = eventArgs.Diagnostic.Kind == WorkspaceDiagnosticKind.Warning
                    ? "[WORKSPACE] WARNING: "
                    : "[WORKSPACE] FATAL: ";
                
                SysConsole.WriteLine(display + eventArgs.Diagnostic.Message);
                SysConsole.ResetColor();

                if (eventArgs.Diagnostic.Kind != WorkspaceDiagnosticKind.Failure)
                    return;
                
                SysConsole.WriteLine("Press any key to exit...");
                SysConsole.ReadKey(true);
            };

            string projectPath = ResolveProjectPath(projPath);

            if (!double.TryParse(threadCount, out double threads))
            {
                SysConsole.WriteLine(
                    "Specify thread count with the \"--thread-count\" argument. Using 8 threads by default."
                );

                threads = 8D;
            }
            
            ActionableReporter<ProjectLoadProgress> loadReporter = new();
            loadReporter.OnReport += value =>
            {
                SysConsole.WriteLine(
                    $"Performed task \"{value.Operation}\" with project \"{value.FilePath}\" ({value.TargetFramework}) in {value.ElapsedTime:g}"
                );
            };

            Project proj = await workspace.OpenProjectAsync(projectPath, loadReporter);
            SysConsole.WriteLine("Finished loading project.");

            RewriteHandler handler = new();
            SysConsole.WriteLine("Initialized syntax rewrite handler.");

            double chunkSize = Math.Min(threads, proj.Documents.Count());
            int i = 0;

            IEnumerable<IEnumerable<Document>> chunks =
                from document in proj.Documents
                group document by i++ % chunkSize
                into part
                select part.AsEnumerable();

            IEnumerable<IEnumerable<Document>> chunkList = chunks.ToList();
            SysConsole.WriteLine($"Running tasks for {chunkList.Count()} chunk(s).");

            IEnumerable<Task> tasks = chunkList.Select(x => Task.Run(async () =>
            {
                foreach (Document doc in x)
                    await handler.RewriteDocument(doc);
            }));
            await Task.WhenAll(tasks);
            
            SysConsole.WriteLine("Finished all tasks.");
        }

        public static string ResolveProjectPath(string? path)
        {
            if (path is not null && File.Exists(Path.ChangeExtension(path, ".csproj")))
            {
                SysConsole.WriteLine($"Using project path: {path}");
                return path;
            }
            
            if (path is null)
                SysConsole.WriteLine("Specify the .csproj project file location \"--project-path\" argument.");

            SysConsole.WriteLine("No path specified/path could not be resolved, please enter a .csproj path:");
            string? userPath = Path.ChangeExtension(SysConsole.ReadLine(), ".csproj");

            while (userPath is null || !File.Exists(userPath))
            {
                SysConsole.WriteLine("Could not resolve the given path, please enter a valid path:");
                userPath = Path.ChangeExtension(SysConsole.ReadLine(), ".csproj");
            }
            
            SysConsole.WriteLine($"Using path: {userPath}");
            return userPath;
        }

        public static string? GetArgument(string arg, string[] args)
        {
            if (args.Length < 2)
                return null;
            
            int index = Array.IndexOf(args, arg);

            if (index == -1 || index == args.Length - 1)
                return null;

            return args[index + 1];
        }
    }
}