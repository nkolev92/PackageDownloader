using NuGet.Common;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PackageDownload.DownloadCommand
{
    class DownloadCommand : ICommand
    {
        private string _currentDirectory;
        public string CurrentDirectory
        {
            get
            {
                return _currentDirectory ?? Directory.GetCurrentDirectory();
            }
            set
            {
                _currentDirectory = value;
            }
        }


        public Command GetCommand()
        {
            var downloadCommand = new Command("download", "download a package");


            var idOption = new Option<string?>(
                name: "--id",
                description: "Package id");


            var versionOption = new Option<string?>(
                name: "--version",
                description: "Package version");


            var sourcesOption = new Option<List<string?>>(
                name: "--source",
                description: "Sources");


            var outputOption = new Option<string?>(
                name: "--output",
                description: "Output rirectory");

            downloadCommand.AddOption(idOption);
            downloadCommand.AddOption(versionOption);
            downloadCommand.AddOption(sourcesOption);
            downloadCommand.AddOption(outputOption);

            downloadCommand.SetHandler(async (id, version, source, output) =>
            {
#if DEBUG
                System.Diagnostics.Debugger.Launch();
#endif
                await RunAsync(id, version, source, output);
            },
idOption, versionOption, sourcesOption, outputOption);

            return downloadCommand;
        }

        public async Task<int> RunAsync(string id, string version, IEnumerable<string> sources, string output)
        {
            Console.WriteLine("id {0}", id);
            Console.WriteLine("version {0}", version);
            Console.WriteLine("sources {0}", string.Join(";", sources));
            Console.WriteLine("output {0}", output);

            var nuGetVersion = version != null ? NuGetVersion.Parse(version) : null;
            var outputDirectory = output ?? CurrentDirectory;

            Console.WriteLine("The output directory is {0}", outputDirectory);
            var cancellationToken = CancellationToken.None;

            var sourceRepository = Repository.Factory.GetCoreV3(sources.First());
            var feed = await sourceRepository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);


            if (nuGetVersion == null)
            {
                Console.WriteLine("The version was not provided. Looking up the latest available.");
                var versions = await feed.GetAllVersionsAsync(id, new SourceCacheContext(), NullLogger.Instance, cancellationToken);
                nuGetVersion = versions.FirstOrDefault();
            }

            if (nuGetVersion == null)
            {
                Console.WriteLine("The package {0} is unavaiable", id);
            }

            var packageIdentity = new PackageIdentity(id, nuGetVersion);
            var filePath = Path.Combine(outputDirectory, packageIdentity.ToString());

            var downloader = await feed.GetPackageDownloaderAsync(packageIdentity, new SourceCacheContext(), NullLogger.Instance, cancellationToken);
            if (downloader != null)
            {
                await downloader.CopyNupkgFileToAsync(filePath, cancellationToken);
            }
            else
            {
                Console.WriteLine("Version: {0} not available.", nuGetVersion);
            }
            return 0;
        }

    }

    public class DownloadArgs
    {
        public DownloadArgs(string id, string version, IEnumerable<string> sources, string output)
        {
            Id = id;
            Version = version;
            Sources = sources;
            Output = output;
        }

        public string Id { get; }
        public string Version { get; }
        public IEnumerable<string> Sources { get; }
        public string Output { get; }

    }
}
