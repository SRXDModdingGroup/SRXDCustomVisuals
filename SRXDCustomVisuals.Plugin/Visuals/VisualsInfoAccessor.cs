using System.Collections.Generic;
using SpinCore.Utility;

namespace SRXDCustomVisuals.Plugin; 

public class VisualsInfoAccessor {
    private Dictionary<string, CustomVisualsInfo> cachedVisualsInfo = new();

    public void SaveCustomVisualsInfo(TrackInfoAssetReference trackInfoRef, CustomVisualsInfo customVisualsInfo) {
        if (!trackInfoRef.IsCustomFile)
            return;

        if (customVisualsInfo.IsEmpty()) {
            CustomChartUtility.RemoveCustomData(trackInfoRef.customFile, "CustomVisualsInfo");
            cachedVisualsInfo.Remove(trackInfoRef.UniqueName);
            
            return;
        }
        
        CustomChartUtility.SetCustomData(trackInfoRef.customFile, "CustomVisualsInfo", customVisualsInfo);
        cachedVisualsInfo[trackInfoRef.UniqueName] = customVisualsInfo;
    }
    
    public CustomVisualsInfo GetCustomVisualsInfo(TrackInfoAssetReference trackInfoRef) {
        if (trackInfoRef == null || !trackInfoRef.IsCustomFile)
            return new CustomVisualsInfo();

        string uniqueName = trackInfoRef.UniqueName;

        if (cachedVisualsInfo.TryGetValue(uniqueName, out var customVisualsInfo))
            return customVisualsInfo;

        if (!CustomChartUtility.TryGetCustomData(trackInfoRef.customFile, "CustomVisualsInfo", out customVisualsInfo))
            return new CustomVisualsInfo();

        cachedVisualsInfo.Add(uniqueName, customVisualsInfo);

        return customVisualsInfo;
    }
}