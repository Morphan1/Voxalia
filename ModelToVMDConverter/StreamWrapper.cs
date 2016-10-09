//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ModelToVMDConverter
{
    public class StreamWrapper
    {
        public Stream BaseStream;

        public StreamWrapper(Stream stream)
        {
            BaseStream = stream;
        }

        public void WriteInt(int i)
        {
            byte[] bits = BitConverter.GetBytes(i);
            if (!BitConverter.IsLittleEndian)
            {
                bits = bits.Reverse().ToArray();
            }
            BaseStream.Write(bits, 0, bits.Length);
        }

        public void WriteFloat(float f)
        {
            byte[] bits = BitConverter.GetBytes(f);
            if (!BitConverter.IsLittleEndian)
            {
                bits = bits.Reverse().ToArray();
            }
            BaseStream.Write(bits, 0, bits.Length);
        }
    }
}
