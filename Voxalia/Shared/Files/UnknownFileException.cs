using System.IO;

namespace Voxalia.Shared.Files
{
    /// <summary>
    /// Wraps a System.IO.FileNotFoundException.
    /// </summary>
    class UnknownFileException : FileNotFoundException
    {
        /// <summary>
        /// Constructs an UnknownFileException.
        /// </summary>
        /// <param name="filename">The name of the unknown file.</param>
        public UnknownFileException(string filename)
            : base("file not found", filename)
        {
        }
    }
}
