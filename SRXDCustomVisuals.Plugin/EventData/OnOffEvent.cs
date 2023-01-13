using Newtonsoft.Json;

namespace SRXDCustomVisuals.Plugin; 

public class OnOffEvent {
    public long Time { get; set; }
    
    public bool On { get; set; }
    
    public byte Index { get; set; }
    
    public byte Value { get; set; }

    public OnOffEvent(long time, bool on, byte index, byte value) {
        Time = time;
        On = on;
        Index = index;
        Value = value;
    }
}