//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

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
