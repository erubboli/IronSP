using System.IO;

namespace IronSharePoint.Framework
{
    /// <summary>
    /// Interface for a virtual folder that contains script files
    /// </summary>
    public interface IHive
    {
        /// <summary>
        /// Checks if a file exists at the given <paramref name="path"/>.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>TRUE if the file exists, FALSE otherwise</returns>
        bool FileExists(string path);

        /// <summary>
        /// Checks if a folder exists at the given <paramref name="path"/>
        /// </summary>
        /// <param name="path"></param>
        /// <returns>TRUE if the file exists, FALSE otherwise</returns>
        bool DirectoryExists(string path);

        /// <summary>
        /// Opens a stream of the file at <paramref name="path"/> with write permissions
        /// </summary>
        /// <param name="path"></param>
        /// <returns>A stream of the file with write permissions</returns>
        Stream OpenInputFileStream(string path);

        /// <summary>
        /// Opens a stream of the file at <paramref name="path"/> with read permissions.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>A stream of the file with read permissions</returns>
        Stream OpenOutputFileSteam(string path);

        /// <summary>
        /// Transforms a relative path of the virtual folder to a full path
        /// </summary>
        /// <param name="path"></param>
        /// <returns>The full path</returns>
        string GetFullPath(string path);
    }
}