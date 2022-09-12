namespace SRXDCustomVisuals.Core; 

public interface IVisualsResources {
    T GetResource<T>(string key, T defaultValue);
}

public static class VisualsResourcesExtensions {
    public static T GetResource<T>(this IVisualsResources resources, string key) => resources.GetResource(key, default(T));
}