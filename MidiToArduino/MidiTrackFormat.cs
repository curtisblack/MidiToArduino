namespace MidiToArduino
{
    public enum MidiTrackFormat : short
    {
        SingleMultiChannelTrack = 0, // - one, single multi-channel track
        MultipleSimultaneousTracks = 1, // - one or more simultaneous tracks
        MultipleSequentialTracks = 2, // - one or more sequentially independent single-track patterns
    }
}
