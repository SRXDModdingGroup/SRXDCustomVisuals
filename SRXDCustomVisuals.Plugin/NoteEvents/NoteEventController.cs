using SRXDCustomVisuals.Core;

namespace SRXDCustomVisuals.Plugin; 

public class NoteEventController {
    public int Count { get; }
    
    private bool[] hits;
    private bool[] holdsBefore;
    private bool[] holdsAfter;

    public NoteEventController(int count) {
        Count = count;
        hits = new bool[count];
        holdsBefore = new bool[count];
        holdsAfter = new bool[count];
    }

    public void Hit(int index) => hits[index] = true;

    public void Hold(int index) => holdsAfter[index] = true;

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
        
        for (int i = 0, j = 256 - Count; i < Count; i++, j++) {
            if (hits[i]) {
                visualsEventManager.SendEvent(new VisualsEvent(VisualsEventType.On, j, 255L));
                visualsEventManager.SendEvent(new VisualsEvent(VisualsEventType.Off, j, 255L));
                
                continue;
            }
            
            bool before = holdsBefore[i];
            bool after = holdsAfter[i];
            
            if (!before && after)
                visualsEventManager.SendEvent(new VisualsEvent(VisualsEventType.On, j, 255L));
            else if (before && !after)
                visualsEventManager.SendEvent(new VisualsEvent(VisualsEventType.Off, j, 255L));
            
            holdsBefore[i] = holdsAfter[i];
        }
    }
}