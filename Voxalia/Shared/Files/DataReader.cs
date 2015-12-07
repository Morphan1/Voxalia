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
