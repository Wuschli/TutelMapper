using Zio;

namespace TutelMapper.Util
{
    public class MapStorage
    {
        public IFileSystem FileSystem { get; }

        public MapStorage()
        {
            //var physicalFileSystem = new PhysicalFileSystem(Assembly.GetExecutingAssembly().Location);
            //var subFileSystem = new SubFileSystem(physicalFileSystem, FileSystemPath.Parse("Tiles"));
            //FileSystem = new SeamlessSevenZipFileSystem(subFileSystem);
        }
    }
}