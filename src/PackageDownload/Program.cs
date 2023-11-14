using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;

namespace PackageDownload
{
    public class Program
    {
        static int Main(string[] args)
        {
            // Create a root command with some options
            var rootCommand = new RootCommand();
            foreach (Type type in 
                Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(mytype => mytype.GetInterfaces().Contains(typeof(ICommand)))) 
            {
                var instance = (ICommand) Activator.CreateInstance(type);
                rootCommand.AddCommand(instance.GetCommand());
            }

            rootCommand.Description = "package utility commands";
            // Parse the incoming args and invoke the handler
            return rootCommand.InvokeAsync(args).Result;
        }
    }
}
