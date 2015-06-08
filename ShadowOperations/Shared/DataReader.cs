using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ShadowOperations.Shared
{
    class DataReader: BinaryReader
    {
        public DataReader(Stream stream)
            : base(stream, FileHandler.encoding)
        {
        }

        public int ReadInt()
        {
            return Utilities.BytesToInt(base.ReadBytes(4));
        }

        public string ReadString(int length)
        {
            return FileHandler.encoding.GetString(base.ReadBytes(length));
        }
    }
}
