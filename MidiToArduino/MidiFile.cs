using System;
using System.Collections.Generic;
using System.IO;

namespace MidiToArduino
{
    public sealed class MidiFile
    {
        public MidiHeaderBlock Header;
        public MidiTrackBlock[] Tracks;

        public void Print()
        {
            Console.WriteLine("MidiFile");
            Header.Print();
            for(int i = 0; i < Tracks.Length; i++)
            {
                Tracks[i].Print();
            }
        }

        public IEnumerable<EventTime> Play()
        {
            if(Header.Format == MidiTrackFormat.SingleMultiChannelTrack || Header.Format == MidiTrackFormat.MultipleSimultaneousTracks)
            {
                double time = 0;
                double[] lastEventTime = new double[Tracks.Length];
                int[] nextEvents = new int[Tracks.Length];

                double tempo = 0;

                for(int i = 0; i < Tracks[0].Length; i++)
                {
                    nextEvents[0] = i;
                    if(Tracks[0].Events[i].DeltaTime != 0)
                    {
                        break;
                    }
                    if(Tracks[0].Events[i] is MetaEvent m)
                    {
                        if(m.IsSetTempo(out int t))
                        {
                            tempo = 6e7 / t;
                        }
                    }
                    yield return new EventTime(0.0, Tracks[0].Events[i]);
                }

                while(true)
                {
                    int track = -1;
                    double next = float.PositiveInfinity;
                    for(int i = 0; i < Tracks.Length; i++)
                    {
                        if(nextEvents[i] < Tracks[i].Events.Length)
                        {
                            double s = (60 * Tracks[i].Events[nextEvents[i]].DeltaTime) / (tempo * Header.DeltaTimeUnit);
                            s += lastEventTime[i];
                            if(s < next)
                            {
                                next = s;
                                track = i;
                            }
                        }
                    }
                    if(track == -1)
                    {
                        break;
                    }
                    time = next;
                    lastEventTime[track] = time;
                    var t = Tracks[track];
                    var e = t.Events[nextEvents[track]];
                    if(e is MetaEvent m)
                    {
                        if(m.IsSetTempo(out int te))
                        {
                            tempo = 6e7 / te;
                        }
                    }
                    nextEvents[track]++;
                    yield return new EventTime(time, e);
                }
            }
        }

        public static MidiFile FromStream(Stream stream)
        {
            MidiFile file = new MidiFile();
            using(var reader = new BinaryReader(stream))
            {
                file.Header = new MidiHeaderBlock(file, reader);
                file.Tracks = new MidiTrackBlock[file.Header.NumberOfTrackBlocks];
                for(int i = 0; i < file.Tracks.Length; i++)
                {
                    file.Tracks[i] = new MidiTrackBlock(file, reader);
                }
            }
            return file;
        }
    }
}
