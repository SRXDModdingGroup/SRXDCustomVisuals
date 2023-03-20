using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using SMU.Utilities;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace SRXDCustomVisuals.Plugin; 

public class VisualsBackgroundManager {
    public VisualsBackground CurrentBackground { get; private set; } = VisualsBackground.Empty;
    
    private Dictionary<string, VisualsBackground> backgrounds = new();

    public void LoadBackground(string backgroundName) {
        bool backgroundExists = TryGetBackground(backgroundName, out var background);

        if (backgroundExists && background == CurrentBackground)
            return;

        if (CurrentBackground != VisualsBackground.Empty) {
            CurrentBackground.Unload();
            CurrentBackground = VisualsBackground.Empty;
        }

        if (!backgroundExists) {
            ResetCameraSettings();

            return;
        }

        var mainCamera = MainCamera.Instance.GetComponent<Camera>();
        
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
        if (CurrentBackground == VisualsBackground.Empty)
            return;
        
        ResetCameraSettings();
        CurrentBackground.Unload();
        CurrentBackground = VisualsBackground.Empty;
    }

    public bool TryGetBackground(string name, out VisualsBackground background) {
        if (string.IsNullOrWhiteSpace(name)) {
            background = null;

            return false;
        }
        
        if (backgrounds.TryGetValue(name, out background))
            return true;

        string[] split = name.Split('/');

        if (split.Length != 2 || string.IsNullOrWhiteSpace(split[0]) || string.IsNullOrWhiteSpace(split[1])) {
            Plugin.Logger.LogWarning($"{name} is not a valid background reference");
            
            return false;
        }

        string backgroundPath = Path.ChangeExtension(Path.Combine(ModsUtility.GetModDirectory(split[0]), "backgrounds", split[1]), ".json");

        if (!File.Exists(backgroundPath)) {
            Plugin.Logger.LogWarning($"Could not find background definition {name}");
            
            return false;
        }

        var backgroundDefinition = JsonConvert.DeserializeObject<BackgroundDefinition>(File.ReadAllText(backgroundPath));

        if (backgroundDefinition == null) {
            Plugin.Logger.LogWarning($"Failed to parse background definition {name}");

            return false;
        }

        background = new VisualsBackground(backgroundDefinition, split[0]);
        backgrounds.Add(name, background);

        return true;
    }
}