﻿using System.Collections.Generic;
using SMU.Utilities;
using SRXDCustomVisuals.Core;
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

    public void Load(VisualsEventManager eventManager, VisualsMetadata metadata, IReadOnlyList<Transform> roots) {
        if (loaded)
            return;
        
        assetBundles = ModsUtility.LoadAssetBundles(modName);

        foreach (var element in elements) {
            string bundleName = element.BundleName;
            string assetName = element.AssetName;
            
            if (!assetBundles.TryGetValue(bundleName, out var bundle)) {
                Plugin.Logger.LogWarning($"Could not find asset bundle {bundleName}");
                
                continue;
            }
            
            if (!bundle.Contains(assetName)) {
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

            foreach (var handler in instance.GetComponentsInChildren<IVisualsEventHandler>())
                eventManager.AddHandler(handler);

            foreach (var handler in instance.GetComponentsInChildren<IVisualsMetadataHandler>())
                handler.ApplyMetadata(metadata);
        }

        loaded = true;
    }

    public void Unload() {
        if (!loaded)
            return;
        
        foreach (var instance in instances)
            Object.Destroy(instance);

        instances.Clear();

        ModsUtility.UnloadAssetBundles(modName);
        assetBundles.Clear();
        loaded = false;
    }
}