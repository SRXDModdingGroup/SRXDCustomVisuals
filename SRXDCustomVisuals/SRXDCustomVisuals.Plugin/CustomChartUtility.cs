using Newtonsoft.Json;

namespace SRXDCustomVisuals.Plugin; 

public static class CustomChartUtility {
    public static bool TryGetCustomData<T>(IMultiAssetSaveFile customFile, string key, out T data) {
        if (!customFile.HasJsonValueForKey(key)) {
            data = default;

            return false;
        }
        
        data = JsonConvert.DeserializeObject<T>(customFile.GetLargeStringOrJson(key).Value);

        return data is not null;
    }
}