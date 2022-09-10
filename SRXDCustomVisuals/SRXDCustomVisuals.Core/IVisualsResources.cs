namespace SRXDCustomVisuals.Core; 

public interface IVisualsResources {
    T GetResource<T>(string key);
    
    T GetResource<T>(string key, T defaultValue);
}