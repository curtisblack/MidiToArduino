using System;
using System.IO;

namespace MidiToArduino
{
    public sealed class MidiHeaderBlock
    {
        public MidiHeaderBlock(MidiFile file, BinaryReader reader)
        {
            File = file;
            Identifier = string.Concat((char)reader.ReadByte(), (char)reader.ReadByte(), (char)reader.ReadByte(), (char)reader.ReadByte());
            Length = (reader.ReadByte() << 24) | (reader.ReadByte() << 16) | (reader.ReadByte() << 8) | reader.ReadByte();
            Format = (MidiTrackFormat)((reader.ReadByte() << 8) | reader.ReadByte());
            NumberOfTrackBlocks = (short)((reader.ReadByte() << 8) | reader.ReadByte());
            DeltaTimeUnit = (short)((reader.ReadByte() << 8) | reader.ReadByte());
        }

        public readonly MidiFile File;
        public string Identifier;
        public int Length;
        public MidiTrackFormat Format;
        public short NumberOfTrackBlocks;
        public short DeltaTimeUnit;

        public void Print()
        {
            Console.WriteLine($"  MidiHeaderBlock");
            Console.WriteLine($"    Identifier: {Identifier}");
            Console.WriteLine($"    Length: {Length}");
            Console.WriteLine($"    Format: {Format}");
            Console.WriteLine($"    NumberOfTrackBlocks: {NumberOfTrackBlocks}");
            Console.WriteLine($"    DeltaTimeUnit: {DeltaTimeUnit}");
        }
    }

}
