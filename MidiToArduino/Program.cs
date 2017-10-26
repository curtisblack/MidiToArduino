using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MidiToArduino
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if(args.Length < 2)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("MidiToArduino.exe input/midi/file.mid path/to/output/directory/ [startButtonPin=2] [buzzerPin=3]");
                return;
            }
            int startButtonPin = 2;
            int buzzerPin = 3;

            if(args.Length > 2)
            {
                startButtonPin = int.Parse(args[2]);
            }
            if(args.Length > 3)
            {
                buzzerPin = int.Parse(args[3]);
            }

            string input = Path.GetFullPath(args[0]);
            string output = Path.GetFullPath(args[1]);
            
            MidiFile file;
            using(var stream = File.OpenRead(input))
            {
                file = MidiFile.FromStream(stream);
            }
            
            var tracks = CreateTimeLine(file);
            for(int i = 0; i < tracks.Count; i++)
            {
                string dir = Path.Combine(output, i.ToString());
                Directory.CreateDirectory(dir);
                File.WriteAllText(Path.Combine(dir, $"{i}.ino"), CreateMultipleSketch(tracks[i], buzzerPin, startButtonPin));
            }
        }

        private static string CreateSingleSketch(MidiFile file)
        {
            var sb = new StringBuilder();
            sb.AppendLine("void loop()");
            sb.AppendLine("{");

            List<byte> currentlyPlaying = new List<byte>();
            double time = 0;
            foreach(var item in file.Play())
            {
                int dt = (int)(1e3 * (item.Time - time));
                if(dt > 0)
                {
                    sb.AppendLine($"    delay({dt});");
                }
                time = item.Time;
                if(item.Event is KeyPressEvent p)
                {
                    bool added = false;
                    for(int i = 0; i < currentlyPlaying.Count; i++)
                    {
                        if(currentlyPlaying[i] == 0)
                        {
                            currentlyPlaying[i] = p.Key;
                            sb.AppendLine($"    tone(buzzer{i}, {NoteFrequency(p.Key)});");
                            added = true;
                            break;
                        }
                    }
                    if(!added)
                    {
                        currentlyPlaying.Add(p.Key);
                        sb.AppendLine($"    tone(buzzer{currentlyPlaying.Count - 1}, {NoteFrequency(p.Key)});");
                    }
                }
                else if(item.Event is KeyReleaseEvent r)
                {
                    for(int i = 0; i < currentlyPlaying.Count; i++)
                    {
                        if(currentlyPlaying[i] == r.Key)
                        {
                            currentlyPlaying[i] = 0;
                            sb.AppendLine($"    noTone(buzzer{i});");
                            break;
                        }
                    }
                }
            }

            var loop = sb.ToString();

            sb.Clear();

            for(int i = 0; i < currentlyPlaying.Count; i++)
            {
                sb.AppendLine($"int buzzer{i} = {i};");
            }
            sb.AppendLine();
            sb.AppendLine("void setup()");
            sb.AppendLine("{");
            for(int i = 0; i < currentlyPlaying.Count; i++)
            {
                sb.AppendLine($"    pinMode(buzzer{i}, OUTPUT);");
            }
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine(loop);
            sb.AppendLine($"    delay(10000);");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private static string CreateSingleSketch1(MidiFile file)
        {
            var sb = new StringBuilder();
            var tracks = CreateTimeLine(file);
            for(int i = 0; i < tracks.Count; i++)
            {
                sb.AppendLine($"int buzzer{i} = {i};");
            }
            sb.AppendLine();
            sb.AppendLine("void setup()");
            sb.AppendLine("{");
            for(int i = 0; i < tracks.Count; i++)
            {
                sb.AppendLine($"    pinMode(buzzer{i}, OUTPUT);");
            }
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("void loop()");
            sb.AppendLine("{");
            double time = 0;
            while(true)
            {
                int t = -1;
                double d = double.PositiveInfinity;
                for(int i = 0; i < tracks.Count; i++)
                {
                    if(tracks[i].Count > 0 && tracks[i][0].Start < d)
                    {
                        d = tracks[i][0].Start;
                        t = i;
                    }
                }
                if(t < 0)
                {
                    break;
                }
                var n = tracks[t][0];
                tracks[t].RemoveAt(0);
                double dt = Math.Max(0, n.Start - time);
                if(dt > 0)
                {
                    sb.AppendLine($"    delay({(int)(dt * 1000)});");
                }
                sb.AppendLine($"    tone(buzzer{t}, {NoteFrequency(n.Note)}, {(int)((n.End - n.Start) * 1000)});");
                time = n.Start;
            }
            sb.AppendLine($"    delay(10000);");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private static string CreateMultipleSketch(List<NoteDuration> track, int buzzerPin, int startButtonPin)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"int button = {startButtonPin};");
            sb.AppendLine($"int buzzer = {buzzerPin};");
            sb.AppendLine();
            sb.AppendLine("void setup()");
            sb.AppendLine("{");
            sb.AppendLine($"    pinMode(button, INPUT_PULLUP);");
            sb.AppendLine($"    pinMode(buzzer, OUTPUT);");
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("void loop()");
            sb.AppendLine("{");
            sb.AppendLine("    while(digitalRead(button));");
            double time = 0;
            for(int i = 0; i < track.Count; i++)
            {
                var n = track[i];
                double dt = (int)(1e3 * Math.Max(0, n.Start - time));
                if(dt > 0)
                {
                    sb.AppendLine($"    delay({dt});");
                }
                sb.AppendLine($"    tone(buzzer, {NoteFrequency(n.Note)}, {(int)((n.End - n.Start) * 1000)});");
                time = n.Start;
            }
            sb.AppendLine($"    delay(10000);");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private static List<List<NoteDuration>> CreateTimeLine(MidiFile file)
        {
            List<List<NoteDuration>> tracks = new List<List<NoteDuration>>();
            void StartNote(byte note, double start)
            {
                foreach(var list in tracks)
                {
                    var last = list.Last();
                    if(last.End >= 0 && start > last.End)
                    {
                        list.Add(new NoteDuration { Note = note, Start = start });
                        return;
                    }
                }
                var l = new List<NoteDuration>();
                l.Add(new NoteDuration { Note = note, Start = start });
                tracks.Add(l);
            }
            void EndNote(byte note, double end)
            {
                foreach(var list in tracks)
                {
                    var last = list.Last();
                    if(last.Note == note && last.End < 0)
                    {
                        last.End = end;
                        return;
                    }
                }
            }
            foreach(var item in file.Play())
            {
                if(item.Event is KeyPressEvent p)
                {
                    StartNote(p.Key, item.Time);
                }
                else if(item.Event is KeyReleaseEvent r)
                {
                    EndNote(r.Key, item.Time);
                }
            }
            return tracks;
        }

        private class NoteDuration
        {
            public byte Note;
            public double Start = -1;
            public double End = -1;
        }

        public static int NoteFrequency(byte note)
        {
            switch(note)
            {
                case 0x15: return 27;
                case 0x16: return 29;
                case 0x17: return 31;
                case 0x18: return 33;
                case 0x19: return 35;
                case 0x1A: return 37;
                case 0x1B: return 39;
                case 0x1C: return 41;
                case 0x1D: return 44;
                case 0x1E: return 46;
                case 0x1F: return 49;
                case 0x20: return 52;
                case 0x21: return 55;
                case 0x22: return 58;
                case 0x23: return 62;
                case 0x24: return 65;
                case 0x25: return 69;
                case 0x26: return 73;
                case 0x27: return 78;
                case 0x28: return 82;
                case 0x29: return 87;
                case 0x2A: return 93;
                case 0x2B: return 98;
                case 0x2C: return 104;
                case 0x2D: return 110;
                case 0x2E: return 117;
                case 0x2F: return 123;
                case 0x30: return 131;
                case 0x31: return 139;
                case 0x32: return 147;
                case 0x33: return 156;
                case 0x34: return 165;
                case 0x35: return 175;
                case 0x36: return 185;
                case 0x37: return 196;
                case 0x38: return 208;
                case 0x39: return 220;
                case 0x3A: return 233;
                case 0x3B: return 247;
                case 0x3C: return 262;
                case 0x3D: return 277;
                case 0x3E: return 294;
                case 0x3F: return 311;
                case 0x40: return 330;
                case 0x41: return 349;
                case 0x42: return 370;
                case 0x43: return 392;
                case 0x44: return 415;
                case 0x45: return 440;
                case 0x46: return 466;
                case 0x47: return 494;
                case 0x48: return 523;
                case 0x49: return 554;
                case 0x4A: return 587;
                case 0x4B: return 622;
                case 0x4C: return 659;
                case 0x4D: return 698;
                case 0x4E: return 740;
                case 0x4F: return 784;
                case 0x50: return 831;
                case 0x51: return 880;
                case 0x52: return 932;
                case 0x53: return 988;
                case 0x54: return 1047;
                case 0x55: return 1109;
                case 0x56: return 1175;
                case 0x57: return 1245;
                case 0x58: return 1319;
                case 0x59: return 1397;
                case 0x5A: return 1480;
                case 0x5B: return 1568;
                case 0x5C: return 1661;
                case 0x5D: return 1760;
                case 0x5E: return 1865;
                case 0x5F: return 1976;
                case 0x60: return 2093;
                case 0x61: return 2217;
                case 0x62: return 2349;
                case 0x63: return 2489;
                case 0x64: return 2637;
                case 0x65: return 2794;
                case 0x66: return 2960;
                case 0x67: return 3136;
                case 0x68: return 3322;
                case 0x69: return 3520;
                case 0x6A: return 3729;
                case 0x6B: return 3951;
                case 0x6C: return 4186;
                default: return 0;
            }
        }
    }
}
