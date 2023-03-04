using System.Collections.Generic;
using SpinCore.Utility;

namespace SRXDCustomVisuals.Plugin; 

public class VisualsInfoAccessor {
    private Dictionary<AssetReferenceKey, CustomVisualsInfo> cachedVisualsInfo = new();

    public void SaveCustomVisualsInfo(TrackInfoAssetReference trackInfoRef, CustomVisualsInfo customVisualsInfo) {
        if (!trackInfoRef.IsCustomFile || trackInfoRef.customFile is not CustomTrackBundleSaveFile customFile)
            return;

        var key = trackInfoRef.GetReferenceKey();

        if (customVisualsInfo.IsEmpty()) {
            CustomChartUtility.RemoveCustomData(customFile, "CustomVisualsInfo");
            cachedVisualsInfo.Remove(key);
            
            return;
        }
        
        CustomChartUtility.SetCustomData(customFile, "CustomVisualsInfo", customVisualsInfo);
        cachedVisualsInfo[key] = customVisualsInfo;
    }
    
    public CustomVisualsInfo GetCustomVisualsInfo(TrackInfoAssetReference trackInfoRef) {
        if (trackInfoRef == null || !trackInfoRef.IsCustomFile)
            return new CustomVisualsInfo();

        var key = trackInfoRef.GetReferenceKey();

        if (cachedVisualsInfo.TryGetValue(key, out var customVisualsInfo))
            return customVisualsInfo;

        if (trackInfoRef.customFile is not CustomTrackBundleSaveFile customFile
            || !CustomChartUtility.TryGetCustomData(customFile, "CustomVisualsInfo", out customVisualsInfo))
            return new CustomVisualsInfo();

        cachedVisualsInfo.Add(key, customVisualsInfo);

        return customVisualsInfo;
    }
}