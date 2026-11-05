using Rocks.Wasabee.Core.Adapters;

namespace Rocks.Wasabee.Maui.Core.Adapters
{
    public class FileSystemAdapter : IFileSystemAdapter
    {
        public string AppDataDirectory => FileSystem.AppDataDirectory;
    }
}