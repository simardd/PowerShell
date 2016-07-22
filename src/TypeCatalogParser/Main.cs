using System;
using System.IO;
using System.Linq;
using NuGet.Frameworks;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.ProjectModel;
using Microsoft.DotNet.ProjectModel.Graph;
using Microsoft.Extensions.DependencyModel.Resolution;

namespace TypeCatalogParser
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException(
                    "Usage: ./TypeCatalogParser <path-to-top-level-powershell-project>\n" +
                    "Valid options are '../powershell-unix' and '../powershell-windows'");
            }

            // These are packages that are not part of .NET Core and must be excluded
            string[] excludedPackages = {
                "Microsoft.Management.Infrastructure",
                "Microsoft.Management.Infrastructure.Native"
                // We need to exclude Newtonsoft.Json once the ALC story is figured out
                // "Newtonsoft.Json"
            };

            // The TypeCatalogGen project takes this as input
            var outputPath = "../TypeCatalogGen/powershell.inc";

            // Get a context for our top level project
            var context = ProjectContext.Create(args[0], NuGetFramework.Parse("netcoreapp1.0"));

            System.IO.File.WriteAllLines(outputPath,
                                         // Get the target for the current runtime
                                         from t in context.LockFile.Targets where t.RuntimeIdentifier == context.RuntimeIdentifier
                                         // Get the packages (not projects)
                                         from x in t.Libraries where (x.Type == "package" && !excludedPackages.Contains(x.Name))
                                         // Get the real reference assemblies
                                         from y in x.CompileTimeAssemblies where y.Path.EndsWith(".dll")
                                         // Construct the path to the assemblies
                                         select $"{context.PackagesDirectory}/{x.Name}/{x.Version}/{y.Path};");

            Console.WriteLine($"List of reference assemblies written to {outputPath}");
        }
    }
}
