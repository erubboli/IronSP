using System;
using System.Collections.Generic;
using System.IO;

namespace IronSharePoint
{
    /// <summary>
    /// Interface for a virtual folder that contains script files
    /// </summary>
    public interface IHive : IDisposable
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
        /// Opens a stream of the file at <paramref name="path"/> with read permissions
        /// </summary>
        /// <param name="path"></param>
        /// <returns>A stream of the file with write permissions</returns>
        Stream OpenInputFileStream(string path);

        /// <summary>
        /// Opens or creates a stream of the file at <paramref name="path"/> with write permissions.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>A stream of the file with read permissions</returns>
        Stream OpenOutputFileStream(string path);

        /// <summary>
        /// Transforms a relative path of the virtual folder to a full path
        /// </summary>
        /// <param name="path"></param>
        /// <returns>The full path</returns>
        string GetFullPath(string path);

        /// <summary>
        /// Checks if a path is an absolute path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        bool IsAbsolutePath(string path);

        /// <summary>
        /// Combines <paramref name="path1"/> and <paramref name="path2"/> to a single path
        /// </summary>
        /// <param name="path1"></param>
        /// <param name="path2"></param>
        /// <returns></returns>
        string CombinePath(string path1, string path2);

        /// <summary>
        /// Returns all partial paths for files in <paramref name="path"/> matching the <paramref name="searchPattern"/>.
        /// Returns absolute paths when <paramref name="absolutePaths"/> is TRUE
        /// </summary>
        /// <param name="path"></param>
        /// <param name="searchPattern"></param>
        /// <param name="absolutePaths"></param>
        /// <returns></returns>
        IEnumerable<string> GetFiles(string path, string searchPattern, bool absolutePaths = false);

        /// <summary>
        /// Returns all partial paths for directories in <paramref name="path"/> matching the <paramref name="searchPattern"/>.
        /// Returns absolute paths when <paramref name="absolutePaths"/> is TRUE
        /// </summary>
        /// <param name="path"></param>
        /// <param name="searchPattern"></param>
        /// <param name="absolutePaths"></param>
        /// <returns></returns>
        IEnumerable<string> GetDirectories(string path, string searchPattern, bool absolutePaths = false);

        /// <summary>
        /// Name of the hive
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Description for the hive
        /// </summary>
        string Description { get; }
    }
}