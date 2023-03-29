namespace SRXDCustomVisuals.Core; 

public class VisualsTick {
    public int Beat { get; }
    
    public double Time { get; }
    
    public double TimeInBeat { get; }
    
    public double TimeInBeatNormalized { get; }

    public VisualsTick(int beat, double time, double timeInBeat, double timeInBeatNormalized) {
        Beat = beat;
        Time = time;
        TimeInBeat = timeInBeat;
        TimeInBeatNormalized = timeInBeatNormalized;
    }
}