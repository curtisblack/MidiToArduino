using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiToArduino
{
    public abstract class Event
    {
        public Event(MidiFile file, int deltaTime)
        {
            File = file;
            DeltaTime = deltaTime;
        }

        public readonly MidiFile File;
        public readonly int DeltaTime;

        protected string NoteValue(byte note)
        {
            switch(note)
            {
                case 0x15: return "A 0";
                case 0x16: return "A# 0";
                case 0x17: return "B 0";
                case 0x18: return "C 1";
                case 0x19: return "C# 1";
                case 0x1A: return "D 1";
                case 0x1B: return "D# 1";
                case 0x1C: return "E 1";
                case 0x1D: return "F 1";
                case 0x1E: return "F# 1";
                case 0x1F: return "G 1";
                case 0x20: return "G# 1";
                case 0x21: return "A 1";
                case 0x22: return "A# 1";
                case 0x23: return "B 1";
                case 0x24: return "C 2";
                case 0x25: return "C# 2";
                case 0x26: return "D 2";
                case 0x27: return "D# 2";
                case 0x28: return "E 2";
                case 0x29: return "F 2";
                case 0x2A: return "F# 2";
                case 0x2B: return "G 2";
                case 0x2C: return "G# 2";
                case 0x2D: return "A 2";
                case 0x2E: return "A# 2";
                case 0x2F: return "B 2";
                case 0x30: return "C 3";
                case 0x31: return "C# 3";
                case 0x32: return "D 3";
                case 0x33: return "D# 3";
                case 0x34: return "E 3";
                case 0x35: return "F 3";
                case 0x36: return "F# 3";
                case 0x37: return "G 3";
                case 0x38: return "G# 3";
                case 0x39: return "A 3";
                case 0x3A: return "A# 3";
                case 0x3B: return "B 3";
                case 0x3C: return "C 4";
                case 0x3D: return "C# 4";
                case 0x3E: return "D 4";
                case 0x3F: return "D# 4";
                case 0x40: return "E 4";
                case 0x41: return "F 4";
                case 0x42: return "F# 4";
                case 0x43: return "G 4";
                case 0x44: return "G# 4";
                case 0x45: return "A 4";
                case 0x46: return "A# 4";
                case 0x47: return "B 4";
                case 0x48: return "C 5";
                case 0x49: return "C# 5";
                case 0x4A: return "D 5";
                case 0x4B: return "D# 5";
                case 0x4C: return "E 5";
                case 0x4D: return "F 5";
                case 0x4E: return "F# 5";
                case 0x4F: return "G 5";
                case 0x50: return "G# 5";
                case 0x51: return "A 5";
                case 0x52: return "A# 5";
                case 0x53: return "B 5";
                case 0x54: return "C 6";
                case 0x55: return "C# 6";
                case 0x56: return "D 6";
                case 0x57: return "D# 6";
                case 0x58: return "E 6";
                case 0x59: return "F 6";
                case 0x5A: return "F# 6";
                case 0x5B: return "G 6";
                case 0x5C: return "G# 6";
                case 0x5D: return "A 6";
                case 0x5E: return "A# 6";
                case 0x5F: return "B 6";
                case 0x60: return "C 7";
                case 0x61: return "C# 7";
                case 0x62: return "D 7";
                case 0x63: return "D# 7";
                case 0x64: return "E 7";
                case 0x65: return "F 7";
                case 0x66: return "F# 7";
                case 0x67: return "G 7";
                case 0x68: return "G# 7";
                case 0x69: return "A 7";
                case 0x6A: return "A# 7";
                case 0x6B: return "B 7";
                case 0x6C: return "C 8";
                default: return null;
            }
        }

        public abstract void Print();

        public static Event Read(MidiFile file, BinaryReader reader)
        {
            int deltaTime = reader.ReadVariable();
            byte b = reader.ReadByte();
            if(MetaEvent.IsValid(b))
            {
                return new MetaEvent(file, deltaTime, b, reader);
            }
            else if(SystemExclusiveEvent.IsValid(b))
            {
                return new SystemExclusiveEvent(file, deltaTime, b, reader);
            }
            else if(KeyReleaseEvent.IsValid(b))
            {
                return new KeyReleaseEvent(file, deltaTime, b, reader);
            }
            else if(KeyPressEvent.IsValid(b))
            {
                return new KeyPressEvent(file, deltaTime, b, reader);
            }
            else if(KeyPressureEvent.IsValid(b))
            {
                return new KeyPressureEvent(file, deltaTime, b, reader);
            }
            else if(ControllerChange.IsValid(b))
            {
                return new ControllerChange(file, deltaTime, b, reader);
            }
            else if(ProgramChangeEvent.IsValid(b))
            {
                return new ProgramChangeEvent(file, deltaTime, b, reader);
            }
            else if(ChannelKeyPressureEvent.IsValid(b))
            {
                return new ChannelKeyPressureEvent(file, deltaTime, b, reader);
            }
            else if(PitchBendEvent.IsValid(b))
            {
                return new PitchBendEvent(file, deltaTime, b, reader);
            }
            return null;
        }
    }

    public sealed class KeyReleaseEvent : Event
    {
        public KeyReleaseEvent(MidiFile file, int deltaTime, byte b, BinaryReader reader) : base(file, deltaTime)
        {
            Channel = (byte)(b & 0xF);
            Key = reader.ReadByte();
            Velocity = reader.ReadByte();
        }

        public readonly byte Channel;
        public readonly byte Key;
        public readonly byte Velocity;

        public override void Print() => Console.WriteLine($"      Key Release: {NoteValue(Key)}. Velocity={Velocity}. Channel={Channel}.");

        public static bool IsValid(byte b) => (b & 0xF0) == 0x80;
    }

    public sealed class KeyPressEvent : Event
    {
        public KeyPressEvent(MidiFile file, int deltaTime, byte b, BinaryReader reader) : base(file, deltaTime)
        {
            Channel = (byte)(b & 0xF);
            Key = reader.ReadByte();
            Velocity = reader.ReadByte();
        }

        public readonly byte Channel;
        public readonly byte Key;
        public readonly byte Velocity;

        public override void Print() => Console.WriteLine($"      Key Press: {NoteValue(Key)}. Velocity={Velocity}. Channel={Channel}.");

        public static bool IsValid(byte b) => (b & 0xF0) == 0x90;
    }

    public sealed class KeyPressureEvent : Event
    {
        public KeyPressureEvent(MidiFile file, int deltaTime, byte b, BinaryReader reader) : base(file, deltaTime)
        {
            Channel = (byte)(b & 0xF);
            Key = reader.ReadByte();
            Pressure = reader.ReadByte();
        }

        public readonly byte Channel;
        public readonly byte Key;
        public readonly byte Pressure;

        public override void Print() => Console.WriteLine($"      Key Pressure: {NoteValue(Key)}. Pressure={Pressure}. Channel={Channel}.");

        public static bool IsValid(byte b) => (b & 0xF0) == 0xA0;
    }

    public sealed class ControllerChange : Event
    {
        public ControllerChange(MidiFile file, int deltaTime, byte b, BinaryReader reader) : base(file, deltaTime)
        {
            Channel = (byte)(b & 0xF);
            Controller = reader.ReadByte();
            Value = reader.ReadByte();
        }

        public readonly byte Channel;
        public readonly byte Controller;
        public readonly byte Value;

        public override void Print() => Console.WriteLine($"      Controller Change: {Controller}. Value={Value}.");

        public static bool IsValid(byte b) => (b & 0xF0) == 0xB0;
    }

    public sealed class ProgramChangeEvent : Event
    {
        public ProgramChangeEvent(MidiFile file, int deltaTime, byte b, BinaryReader reader) : base(file, deltaTime)
        {
            Channel = (byte)(b & 0xF);
            Number = reader.ReadByte();
        }

        public readonly byte Channel;
        public readonly byte Number;

        public override void Print() => Console.WriteLine($"      Program Change: {Number}.");

        public static bool IsValid(byte b) => (b & 0xF0) == 0xC0;
    }

    public sealed class ChannelKeyPressureEvent : Event
    {
        public ChannelKeyPressureEvent(MidiFile file, int deltaTime, byte b, BinaryReader reader) : base(file, deltaTime)
        {
            Channel = (byte)(b & 0xF);
            Pressure = reader.ReadByte();
        }

        public readonly byte Channel;
        public readonly byte Pressure;

        public override void Print() => Console.WriteLine($"      Channel Pressure: {Pressure}.");

        public static bool IsValid(byte b) => (b & 0xF0) == 0xD0;
    }

    public sealed class PitchBendEvent : Event
    {
        public PitchBendEvent(MidiFile file, int deltaTime, byte b, BinaryReader reader) : base(file, deltaTime)
        {
            Channel = (byte)(b & 0xF);
            Bend1 = reader.ReadByte();
            Bend2 = reader.ReadByte();
        }

        public readonly byte Channel;
        public readonly byte Bend1;
        public readonly byte Bend2;

        public override void Print() => Console.WriteLine($"      Pitch Bend: {Bend1} {Bend2}.");

        public static bool IsValid(byte b) => (b & 0xF0) == 0xD0;
    }

    public sealed class MetaEvent : Event
    {
        public MetaEvent(MidiFile file, int deltaTime, byte b, BinaryReader reader) : base(file, deltaTime)
        {
            Type = reader.ReadByte();
            Length = reader.ReadVariable();
            Data = new byte[Length];
            for(int i = 0; i < Data.Length; i++)
            {
                Data[i] = reader.ReadByte();
            }
        }

        public readonly byte Type;
        public readonly int Length;
        public readonly byte[] Data;

        public override void Print()
        {
            switch(Type)
            {
                case 0x00:
                    Console.WriteLine($"      Sequence Number = {Data[0]} {Data[1]}");
                    break;
                case 0x01:
                    Console.WriteLine($"      Text: {DataString}");
                    break;
                case 0x02:
                    Console.WriteLine($"      Copyright: {DataString}");
                    break;
                case 0x03:
                    Console.WriteLine($"      Sequence Name: {DataString}");
                    break;
                case 0x04:
                    Console.WriteLine($"      Instrument Name: {DataString}");
                    break;
                case 0x05:
                    Console.WriteLine($"      Lyric: {DataString}");
                    break;
                case 0x06:
                    Console.WriteLine($"      Marker: {DataString}");
                    break;
                case 0x07:
                    Console.WriteLine($"      Cue: {DataString}");
                    break;
                case 0x20:
                    Console.WriteLine($"      MIDI Channel Association: {Data[0]}");
                    break;
                case 0x2F:
                    Console.WriteLine($"      End of Track");
                    break;
                case 0x51:
                    Console.WriteLine($"      Set Tempo: {DataInt}us per quater note ({60000000 / DataInt} bpm)");
                    break;
                case 0x54:
                    Console.WriteLine($"      SMTPE Offset: {Data[0]}:{Data[1]}:{Data[2]}.{Data[3]}/{Data[4]}");
                    break;
                case 0x58:
                    Console.WriteLine($"      Time Signature: {Data[0]}/{(int)Math.Pow(2, Data[1])}. {Data[2]} MIDI clocks per metronome tick. {Data[3]} 1/32 notes per 24 MIDI clock ticks.");
                    break;
                case 0x59:
                    if(Data[1] == 0)
                    {
                        Console.WriteLine($"      Key Signature: {KeySignature} major");
                    }
                    else
                    {
                        Console.WriteLine($"      Key Signature: {KeySignature} minor");
                    }
                    break;
                case 0x7F:
                    Console.WriteLine($"      Sequence Specific Meta Event");
                    break;

            }
        }

        public bool IsSetTempo(out int usPerBeat)
        {
            if(Type == 0x51)
            {
                usPerBeat = DataInt;
                return true;
            }
            usPerBeat = 0;
            return false;
        }

        private string DataString
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                for(int i = 0; i < Data.Length; i++)
                {
                    sb.Append((char)Data[i]);
                }
                return sb.ToString();
            }
        }
        private int DataInt => (Data[0] << 16) | (Data[1] << 8) | Data[2];
        private string KeySignature
        {
            get
            {
                switch(Data[0])
                {
                    case 256 - 7:
                        return "F";
                    case 256 - 6:
                        return "Gb";
                    case 256 - 5:
                        return "G";
                    case 256 - 4:
                        return "Ab";
                    case 256 - 3:
                        return "A";
                    case 256 - 2:
                        return "Bb";
                    case 256 - 1:
                        return "B";
                    case 0:
                        return "C";
                    case 1:
                        return "C#";
                    case 2:
                        return "D";
                    case 3:
                        return "D#";
                    case 4:
                        return "E";
                    case 5:
                        return "F";
                    case 6:
                        return "F#";
                    case 7:
                        return "G";
                    default:
                        return null;
                }
            }
        }

        public static bool IsValid(byte b) => b == 0xFF;
    }

    public sealed class SystemExclusiveEvent : Event
    {
        public SystemExclusiveEvent(MidiFile file, int deltaTime, byte b, BinaryReader reader) : base(file, deltaTime)
        {
            Length = reader.ReadVariable();
            Data = new byte[Length];
            for(int i = 0; i < Data.Length; i++)
            {
                Data[i] = reader.ReadByte();
            }
        }

        public readonly int Length;
        public readonly byte[] Data;

        public override void Print()
        {
            Console.WriteLine($"      SysExEvent");
        }

        public static bool IsValid(byte b) => b == 0xF0 || b == 0xF7;
    }
}
