//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System.IO;

namespace Voxalia.Shared.Files
{
    public class DataReader: BinaryReader
    {
        public DataReader(Stream stream)
            : base(stream, FileHandler.encoding)
        {
        }

        public int ReadInt()
        {
            return Utilities.BytesToInt(base.ReadBytes(4));
        }

        public long ReadLong()
        {
            return Utilities.BytesToLong(base.ReadBytes(8));
        }

        public float ReadFloat()
        {
            return Utilities.BytesToFloat(base.ReadBytes(4));
        }

        public string ReadString(int length)
        {
            return FileHandler.encoding.GetString(base.ReadBytes(length));
        }

        public byte[] ReadFullBytes()
        {
            int len = ReadInt();
            return ReadBytes(len);
        }

        public string ReadFullString()
        {
            int len = ReadInt();
            return ReadString(len);
        }
    }
}
