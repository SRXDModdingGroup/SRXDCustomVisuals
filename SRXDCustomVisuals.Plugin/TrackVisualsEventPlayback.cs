using System.Collections.Generic;
using SRXDCustomVisuals.Core;

namespace SRXDCustomVisuals.Plugin; 

public class TrackVisualsEventPlayback {
    private TrackVisualsEventData eventData;
    private long lastTime;
    private bool[,] activeNoteOns = new bool[256, 256];
    private int[] lastOnOffEventPerChannel = new int[256];

    public void Update(long time, bool reset) {
        var visualsEventManager = VisualsEventManager.Instance;
        
        if (reset) {
            for (int i = 0; i < 256; i++) {
                for (int j = 0; j < 256; j++) {
                    if (!activeNoteOns[i, j])
                        continue;
                    
                    visualsEventManager.SendEvent(new VisualsEvent(VisualsEventType.NoteOff, (byte) i, (byte) j, 0));
                    activeNoteOns[i, j] = false;
                }
            }
        }
        else {
            
        }

        lastTime = time;
    }
}