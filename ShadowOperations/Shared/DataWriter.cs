using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ShadowOperations.Shared
{
    class DataWriter: BinaryWriter
    {
        public DataWriter(Stream stream)
            : base(stream, FileHandler.encoding)
        {
        }

        public void WriteInt(int x)
        {
            base.OutStream.Write(Utilities.IntToBytes(x), 0, 4);
        }

        public void WriteBytes(byte[] bits)
        {
            base.Write(bits, 0, bits.Length);
        }

        public void WriteFullString(string str)
        {
            byte[] data = FileHandler.encoding.GetBytes(str);
            WriteInt(data.Length);
            WriteBytes(data);
        }
    }
}
