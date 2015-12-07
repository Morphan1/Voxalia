using System.IO;

namespace Voxalia.Shared.Files
{
    public class DataWriter: BinaryWriter
    {
        public DataWriter(Stream stream)
            : base(stream, FileHandler.encoding)
        {
        }

        public void WriteInt(int x)
        {
            base.Write(Utilities.IntToBytes(x), 0, 4);
        }

        public void WriteFloat(float x)
        {
            base.Write(Utilities.FloatToBytes(x), 0, 4);
        }

        public void WriteBytes(byte[] bits)
        {
            base.Write(bits, 0, bits.Length);
        }

        public void WriteFullBytes(byte[] data)
        {
            WriteInt(data.Length);
            WriteBytes(data);
        }

        public void WriteFullString(string str)
        {
            byte[] data = FileHandler.encoding.GetBytes(str);
            WriteInt(data.Length);
            WriteBytes(data);
        }
    }
}
