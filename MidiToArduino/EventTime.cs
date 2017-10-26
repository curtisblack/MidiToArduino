namespace MidiToArduino
{
    public sealed class EventTime
    {
        public EventTime(double timeSinceBeginning, Event e)
        {
            Time = timeSinceBeginning;
            Event = e;
        }

        public readonly double Time;
        public readonly Event Event;
    }
}
