using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using SMU.Utilities;
using SRXDCustomVisuals.Core;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace SRXDCustomVisuals.Plugin; 

public class VisualsSceneManager {
    private static readonly string ASSET_BUNDLES_PATH = Path.Combine(Util.AssemblyPath, "AssetBundles");
    private static readonly string BACKGROUNDS_PATH = Path.Combine(Util.AssemblyPath, "Backgrounds");

    public static void CreateDirectories() {
        if (!Directory.Exists(ASSET_BUNDLES_PATH))
            Directory.CreateDirectory(ASSET_BUNDLES_PATH);
        
        if (!Directory.Exists(BACKGROUNDS_PATH))
            Directory.CreateDirectory(BACKGROUNDS_PATH);
    }
    
    private VisualsScene currentScene;
    private Dictionary<string, BackgroundDefinition> cachedBackgroundDefinitions = new();
    private Dictionary<string, VisualsScene> scenes = new();

    public void LoadScene(string backgroundName) {
        var mainCamera = MainCamera.Instance.GetComponent<Camera>();
        
        mainCamera.clearFlags = CameraClearFlags.Skybox;
        mainCamera.GetUniversalAdditionalCameraData().requiresDepthTexture = false;

        if (currentScene != null) {
            currentScene.Unload();
            currentScene = null;
        }
        
        if (!TryGetBackgroundDefinition(backgroundName, out var backgroundDefinition)
            || !TryGetSceneLoader(backgroundName, out currentScene))
            return;

        if (backgroundDefinition.DisableBaseBackground) {
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = Color.black;
            mainCamera.GetUniversalAdditionalCameraData().requiresDepthTexture = true;
        }

        currentScene.Load(new[] { null, mainCamera.transform });
    }

    public void UnloadScene() {
        MainCamera.Instance.GetComponent<Camera>().clearFlags = CameraClearFlags.Skybox;
        
        if (currentScene == null)
            return;
        
        currentScene.Unload();
        currentScene = null;
    }

    public BackgroundAssetReference GetBackgroundForScene(BackgroundAssetReference defaultBackground, string backgroundName) {
        if (TryGetBackgroundDefinition(backgroundName, out var backgroundDefinition)
            && backgroundDefinition.DisableBaseBackground)
            return BackgroundSystem.UtilityBackgrounds.lowMotionBackground;
        
        return defaultBackground;
    }

    private bool TryGetBackgroundDefinition(string name, out BackgroundDefinition backgroundDefinition) {
        if (string.IsNullOrWhiteSpace(name)) {
            backgroundDefinition = null;
            
            return false;
        }
        
        if (cachedBackgroundDefinitions.TryGetValue(name, out backgroundDefinition))
            return true;
        
        string backgroundPath = Path.ChangeExtension(Path.Combine(BACKGROUNDS_PATH, name), ".json");

        if (!File.Exists(backgroundPath))
            return false;

        backgroundDefinition = JsonConvert.DeserializeObject<BackgroundDefinition>(File.ReadAllText(backgroundPath));

        if (backgroundDefinition == null)
            return false;

        cachedBackgroundDefinitions.Add(name, backgroundDefinition);

        return true;
    }

    private bool TryGetSceneLoader(string name, out VisualsScene scene) {
        if (string.IsNullOrWhiteSpace(name)) {
            scene = null;

            return false;
        }
        
        if (scenes.TryGetValue(name, out scene))
            return true;

        if (!TryGetBackgroundDefinition(name, out var backgroundDefinition))
            return false;
        
        bool success = true;
        var assetBundles = new Dictionary<string, AssetBundle>();

        foreach (string bundleName in backgroundDefinition.AssetBundles) {
            if (AssetBundleUtility.TryGetAssetBundle(ASSET_BUNDLES_PATH, bundleName, out var bundle))
                assetBundles.Add(bundleName, bundle);
            else {
                Plugin.Logger.LogWarning($"Could not load asset bundle {bundleName}");
                success = false;
            }
        }

        foreach (string assembly in backgroundDefinition.Assemblies) {
            if (Util.TryLoadAssembly(assembly))
                continue;
            
            Plugin.Logger.LogWarning($"Could not load assembly {assembly}");
            success = false;
        }

        if (!success)
            return false;
        
        var elements = new List<VisualsElementReference>();

        foreach (var elementReference in backgroundDefinition.Elements) {
            if (!assetBundles.TryGetValue(elementReference.Bundle, out var bundle)) {
                Plugin.Logger.LogWarning($"Could not find asset bundle {elementReference.Bundle}");
                success = false;
                
                continue;
            }

            if (!bundle.Contains(elementReference.Asset)) {
                Plugin.Logger.LogWarning($"Could not find asset {elementReference.Asset} in bundle {elementReference.Bundle}");
                success = false;

                continue;
            }

            var element = bundle.LoadAsset<GameObject>(elementReference.Asset);

            if (element == null) {
                Plugin.Logger.LogWarning($"Asset {elementReference.Asset} in bundle {elementReference.Bundle} is not a GameObject");
                success = false;
                
                continue;
            }
            
            elements.Add(new VisualsElementReference(element, elementReference.Root));
        }
            
        if (!success)
            return false;

        scene = new VisualsScene(elements);
        scenes.Add(name, scene);

        return true;
    }
}