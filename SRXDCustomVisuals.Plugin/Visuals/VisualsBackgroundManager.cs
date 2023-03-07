using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace SRXDCustomVisuals.Plugin; 

public class VisualsBackgroundManager {
    public static void CreateDirectories() {
        if (!Directory.Exists(Util.AssetBundlesPath))
            Directory.CreateDirectory(Util.AssetBundlesPath);
        
        if (!Directory.Exists(Util.BackgroundsPath))
            Directory.CreateDirectory(Util.BackgroundsPath);
    }
    
    public VisualsBackground CurrentBackground { get; private set; }
    
    private Dictionary<string, VisualsBackground> backgrounds = new();

    public void LoadBackground(string backgroundName) {
        bool backgroundExists = TryGetBackground(backgroundName, out var background);

        if (backgroundExists && background == CurrentBackground)
            return;

        if (CurrentBackground != null) {
            CurrentBackground.Unload();
            CurrentBackground = null;
        }
        
        var mainCamera = MainCamera.Instance.GetComponent<Camera>();
        
        if (!backgroundExists) {
            ResetCameraSettings();

            return;
        }

        if (background.DisableBaseBackground) {
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = Color.black;
        }
        else {
            mainCamera.clearFlags = CameraClearFlags.Skybox;
            mainCamera.backgroundColor = Color.white;
        }

        mainCamera.GetUniversalAdditionalCameraData().requiresDepthTexture = background.UseDepthTexture;
        mainCamera.farClipPlane = background.FarClip;
        background.Load(new[] { null, mainCamera.transform });
        CurrentBackground = background;
    }

    private static void ResetCameraSettings() {
        var mainCamera = MainCamera.Instance.GetComponent<Camera>();;
        
        mainCamera.clearFlags = CameraClearFlags.Skybox;
        mainCamera.backgroundColor = Color.white;
        mainCamera.GetUniversalAdditionalCameraData().requiresDepthTexture = false;
        mainCamera.farClipPlane = 100f;
    }

    public void UnloadBackground() {
        if (CurrentBackground == null)
            return;
        
        ResetCameraSettings();
        CurrentBackground.Unload();
        CurrentBackground = null;
    }

    public bool TryGetBackground(string name, out VisualsBackground background) {
        if (string.IsNullOrWhiteSpace(name)) {
            background = null;

            return false;
        }
        
        if (backgrounds.TryGetValue(name, out background))
            return true;

        string backgroundPath = Path.ChangeExtension(Path.Combine(Util.BackgroundsPath, name), ".json");

        if (!File.Exists(backgroundPath)) {
            Plugin.Logger.LogWarning($"Could not find background definition {name}");
            
            return false;
        }

        var backgroundDefinition = JsonConvert.DeserializeObject<BackgroundDefinition>(File.ReadAllText(backgroundPath));

        if (backgroundDefinition == null) {
            Plugin.Logger.LogWarning($"Failed to parse background definition {name}");

            return false;
        }

        background = new VisualsBackground(backgroundDefinition);
        backgrounds.Add(name, background);

        return true;
    }
}