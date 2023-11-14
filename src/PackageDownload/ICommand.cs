using System.CommandLine;

namespace PackageDownload
{
    internal interface ICommand
    {
        Command GetCommand();  
    }
}
