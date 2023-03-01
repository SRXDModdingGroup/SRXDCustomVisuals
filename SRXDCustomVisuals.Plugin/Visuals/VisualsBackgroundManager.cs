using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace SRXDCustomVisuals.Plugin; 

public class VisualsBackgroundManager {
    private static readonly int SPECTRUM_BANDS_CUSTOM = Shader.PropertyToID("_SpectrumBandsCustom");

    public static void CreateDirectories() {
        if (!Directory.Exists(Util.AssetBundlesPath))
            Directory.CreateDirectory(Util.AssetBundlesPath);
        
        if (!Directory.Exists(Util.BackgroundsPath))
            Directory.CreateDirectory(Util.BackgroundsPath);
    }
    
    private VisualsBackground currentBackground;
    private Dictionary<string, VisualsBackground> backgrounds = new();

    public void LoadBackground(string backgroundName) {
        bool backgroundExists = TryGetBackground(backgroundName, out var background);

        if (backgroundExists && background == currentBackground)
            return;

        if (currentBackground != null) {
            currentBackground.Unload();
            currentBackground = null;
        }
        
        var mainCamera = MainCamera.Instance.GetComponent<Camera>();
        
        if (!backgroundExists) {
            mainCamera.clearFlags = CameraClearFlags.Skybox;
            mainCamera.GetUniversalAdditionalCameraData().requiresDepthTexture = false;
            
            return;
        }

        if (background.DisableBaseBackground) {
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = Color.black;
        }
        else
            mainCamera.clearFlags = CameraClearFlags.Skybox;

        mainCamera.GetUniversalAdditionalCameraData().requiresDepthTexture = true;
        background.Load(new[] { null, mainCamera.transform });
        currentBackground = background;
    }

    public void UnloadBackground() {
        if (currentBackground == null)
            return;
        
        var mainCamera = MainCamera.Instance.GetComponent<Camera>();
        
        mainCamera.clearFlags = CameraClearFlags.Skybox;
        mainCamera.GetUniversalAdditionalCameraData().requiresDepthTexture = false;
        currentBackground.Unload();
        currentBackground = null;
    }

    public void UpdateSpectrumBuffer(ComputeBuffer buffer, ComputeBuffer emptyBuffer) {
        if (currentBackground != null && currentBackground.UseAudioSpectrum)
            Shader.SetGlobalBuffer(SPECTRUM_BANDS_CUSTOM, buffer);
        else
            Shader.SetGlobalBuffer(SPECTRUM_BANDS_CUSTOM, emptyBuffer);
    }

    public BackgroundAssetReference GetBaseBackground(BackgroundAssetReference defaultBaseBackground, string visualsBackgroundName) {
        if (TryGetBackground(visualsBackgroundName, out var visualsBackground) && visualsBackground.DisableBaseBackground)
            return BackgroundSystem.DefaultBackground;
        
        return defaultBaseBackground;
    }

    private bool TryGetBackground(string name, out VisualsBackground background) {
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