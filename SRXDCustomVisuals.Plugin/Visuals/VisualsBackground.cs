using System.Collections.Generic;
using SMU.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SRXDCustomVisuals.Plugin; 

public class VisualsBackground {
    public static VisualsBackground Empty { get; } = new(new BackgroundDefinition(), string.Empty);
    
    public bool DisableBaseBackground { get; }
    
    public bool UseAudioSpectrum { get; }
    
    public bool UseAudioWaveform { get; }
    
    public bool UseDepthTexture { get; }
    
    public float FarClip { get; }

    public IReadOnlyList<string> EventLabels { get; }

    public IReadOnlyList<string> CurveLabels { get; }

    private string modName;
    private string[] assetBundleNames;
    private string[] dependencies;
    private VisualsElement[] elements;
    private Dictionary<string, AssetBundle> assetBundles;
    private List<GameObject> instances;
    private bool loaded;

    public VisualsBackground(BackgroundDefinition definition, string modName) {
        this.modName = modName;
        DisableBaseBackground = definition.DisableBaseBackground;
        UseAudioSpectrum = definition.UseAudioSpectrum;
        UseAudioWaveform = definition.UseAudioWaveform;
        UseDepthTexture = definition.UseDepthTexture;
        FarClip = definition.FarClip;
        assetBundleNames = definition.AssetBundles.Copy();
        dependencies = definition.Dependencies.Copy();
        EventLabels = definition.EventLabels.Copy();
        CurveLabels = definition.CurveLabels.Copy();

        var elementReferences = definition.Elements;
        
        elements = new VisualsElement[elementReferences.Length];

        for (int i = 0; i < elementReferences.Length; i++) {
            var reference = elementReferences[i];
            
            elements[i] = new VisualsElement(reference.Bundle, reference.Asset, reference.Root);
        }

        assetBundles = new Dictionary<string, AssetBundle>();
        instances = new List<GameObject>();
    }

    public void Load(IReadOnlyList<Transform> roots) {
        if (loaded)
            return;

        ModsUtility.TryLoadMod(modName);

        foreach (string dependency in dependencies) {
            if (!ModsUtility.TryLoadMod(dependency))
                Plugin.Logger.LogWarning($"Could not load dependency {dependency}");
        }

        foreach (string bundleName in assetBundleNames) {
            if (ModsUtility.TryGetAssetBundle(modName, bundleName, out var bundle))
                assetBundles.Add(bundleName, bundle);
            else
                Plugin.Logger.LogWarning($"Could not load asset bundle {bundleName}");
        }

        foreach (var element in elements) {
            string bundleName = element.BundleName;
            string assetName = element.AssetName;
            
            if (!assetBundles.TryGetValue(bundleName, out var bundle) || !bundle.Contains(assetName)) {
                Plugin.Logger.LogWarning($"Could not find asset {assetName} in bundle {bundleName}");
                
                continue;
            }

            var prefab = bundle.LoadAsset<GameObject>(assetName);

            if (prefab == null) {
                Plugin.Logger.LogWarning($"Asset {assetName} in bundle {bundleName} is not a GameObject");
                
                continue;
            }
            
            if (element.Root < 0 || element.Root >= roots.Count) {
                Plugin.Logger.LogWarning($"Root for asset {assetName} in bundle {bundleName} is out of range");
                
                continue;
            }
            
            var instance = Object.Instantiate(prefab, roots[element.Root]);

            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            instances.Add(instance);
        }

        loaded = true;
    }

    public void Unload() {
        if (!loaded)
            return;
        
        foreach (var instance in instances)
            Object.Destroy(instance);

        instances.Clear();

        foreach (string bundleName in assetBundleNames)
            ModsUtility.UnloadAssetBundle(modName, bundleName);
        
        assetBundles.Clear();
        loaded = false;
    }
}