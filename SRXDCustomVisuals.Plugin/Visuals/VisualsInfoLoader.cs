using System.Collections.Generic;
using SpinCore.Utility;

namespace SRXDCustomVisuals.Plugin; 

public class VisualsInfoLoader {
    private Dictionary<string, CustomVisualsInfo> cachedVisualsInfo = new();
    
    public bool TryGetCustomVisualsInfo(TrackInfoAssetReference trackInfoRef, out CustomVisualsInfo customVisualsInfo) {
        if (!trackInfoRef.IsCustomFile) {
            customVisualsInfo = null;

            return false;
        }
        
        string uniqueName = trackInfoRef.UniqueName;

        if (cachedVisualsInfo.TryGetValue(uniqueName, out customVisualsInfo))
            return true;

        if (!CustomChartUtility.TryGetCustomData(trackInfoRef.customFile, "CustomVisualsInfo", out customVisualsInfo))
            return false;

        cachedVisualsInfo.Add(uniqueName, customVisualsInfo);

        return true;
    }
}