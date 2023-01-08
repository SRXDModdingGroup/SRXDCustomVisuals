using SRXDCustomVisuals.Core;

namespace SRXDCustomVisuals.Plugin; 

public class NoteEventController {
    public byte Channel { get; }
    
    public byte Count { get; }
    
    private bool[] hits;
    private bool[] holdsBefore;
    private bool[] holdsAfter;

    public NoteEventController(byte channel, byte count) {
        Channel = channel;
        Count = count;
        hits = new bool[count];
        holdsBefore = new bool[count];
        holdsAfter = new bool[count];
    }

    public void Hit(byte index) => hits[index] = true;

    public void Hold(byte index) => holdsAfter[index] = true;

    public void Reset() {
        for (int i = 0; i < Count; i++) {
            hits[i] = false;
            holdsBefore[i] = false;
            holdsAfter[i] = false;
        }
    }

    public void Listen() {
        for (int i = 0; i < Count; i++) {
            hits[i] = false;
            holdsAfter[i] = false;
        }
    }

    public void Send() {
        var visualsEventManager = VisualsEventManager.Instance;
        
        for (byte i = 0; i < Count; i++) {
            if (hits[i]) {
                visualsEventManager.SendEvent(new VisualsEvent(VisualsEventType.NoteOn, Channel, i));
                visualsEventManager.SendEvent(new VisualsEvent(VisualsEventType.NoteOff, Channel, i));
                
                continue;
            }
            
            bool before = holdsBefore[i];
            bool after = holdsAfter[i];
            
            if (!before && after)
                visualsEventManager.SendEvent(new VisualsEvent(VisualsEventType.NoteOn, Channel, i));
            else if (before && !after)
                visualsEventManager.SendEvent(new VisualsEvent(VisualsEventType.NoteOff, Channel, i));
            
            holdsBefore[i] = holdsAfter[i];
        }
    }
}