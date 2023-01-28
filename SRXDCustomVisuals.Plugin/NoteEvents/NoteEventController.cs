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
        
        for (int i = 0, j = Constants.IndexCount - Count; i < Count; i++, j++) {
            if (hits[i]) {
                visualsEventManager.SendEvent(new VisualsEvent(VisualsEventType.On, j, Constants.MaxEventValue));
                visualsEventManager.SendEvent(new VisualsEvent(VisualsEventType.Off, j, Constants.MaxEventValue));
                
                continue;
            }
            
            bool before = holdsBefore[i];
            bool after = holdsAfter[i];
            
            if (!before && after)
                visualsEventManager.SendEvent(new VisualsEvent(VisualsEventType.On, j, Constants.MaxEventValue));
            else if (before && !after)
                visualsEventManager.SendEvent(new VisualsEvent(VisualsEventType.Off, j, Constants.MaxEventValue));
            
            holdsBefore[i] = holdsAfter[i];
        }
    }
}