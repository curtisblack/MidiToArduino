using System.IO;

namespace MidiToArduino
{
    public static class BinaryReaderExtensions
    {
        public static int ReadVariable(this BinaryReader reader)
        {
            int value = 0;

            while(true)
            {
                byte b = reader.ReadByte();
                value = (value << 7) | (b & 0x7f);
                if((b & 0x80) == 0)
                {
                    return value;
                }
            }
        }
    }
}
