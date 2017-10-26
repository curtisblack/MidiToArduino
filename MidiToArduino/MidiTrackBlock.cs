using System;
using System.Collections.Generic;
using System.IO;

namespace MidiToArduino
{
    public sealed class MidiTrackBlock
    {
        public MidiTrackBlock(MidiFile file, BinaryReader reader)
        {
            File = file;
            Identifier = string.Concat((char)reader.ReadByte(), (char)reader.ReadByte(), (char)reader.ReadByte(), (char)reader.ReadByte());
            Length = (reader.ReadByte() << 24) | (reader.ReadByte() << 16) | (reader.ReadByte() << 8) | reader.ReadByte();

            var events = new List<Event>();

            long end = reader.BaseStream.Position + Length;
            while(reader.BaseStream.Position < end)
            {
                events.Add(Event.Read(file, reader));
            }
            Events = events.ToArray();
        }

        public readonly MidiFile File;
        public readonly string Identifier;
        public readonly int Length;
        public readonly Event[] Events;

        public void Print()
        {
            Console.WriteLine($"  MidiHeaderBlock");
            Console.WriteLine($"    Identifier: {Identifier}");
            Console.WriteLine($"    Length: {Length}");
            Console.WriteLine($"    Events: {Events.Length}");
            for(int i = 0; i < Events.Length; i++)
            {
                Console.Write(Events[i].DeltaTime);
                Events[i].Print();
            }
        }
    }
}
