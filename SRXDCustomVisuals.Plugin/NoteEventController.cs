using SRXDCustomVisuals.Core;

namespace SRXDCustomVisuals.Plugin; 

public class NoteEventController {
    private bool[] hits;
    private bool[] holdsBefore;
    private bool[] holdsAfter;

    public NoteEventController(int hitsCount, int holdsCount) {
        hits = new bool[hitsCount];
        holdsBefore = new bool[holdsCount];
        holdsAfter = new bool[holdsCount];
    }

    public void Hit(byte index) => hits[index] = true;

    public void Hold(byte index) => holdsAfter[index] = true;

    public void Reset() {
        for (int i = 0; i < hits.Length; i++)
            hits[i] = false;

        for (int i = 0; i < holdsBefore.Length; i++)
            holdsBefore[i] = false;

        for (int i = 0; i < holdsAfter.Length; i++)
            holdsAfter[i] = false;
    }

    public void Listen() {
        for (int i = 0; i < hits.Length; i++)
            hits[i] = false;
        
        for (int i = 0; i < holdsAfter.Length; i++)
            holdsAfter[i] = false;
    }

    public void Send() {
        var visualsEventManager = VisualsEventManager.Instance;
        
        for (byte i = 0; i < hits.Length; i++) {
            if (!hits[i])
                continue;
            
            visualsEventManager.SendEvent(new VisualsEvent(VisualsEventType.NoteOn, 255, i));
            visualsEventManager.SendEvent(new VisualsEvent(VisualsEventType.NoteOff, 255, i));
        }

        for (byte i = 0; i < holdsBefore.Length; i++) {
            bool before = holdsBefore[i];
            bool after = holdsAfter[i];
            
            if (!before && after)
                visualsEventManager.SendEvent(new VisualsEvent(VisualsEventType.NoteOn, 255, i));
            else if (before && !after)
                visualsEventManager.SendEvent(new VisualsEvent(VisualsEventType.NoteOff, 255, i));

            holdsBefore[i] = holdsAfter[i];
        }
    }
}