using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using SMU;
using SMU.Utilities;
using SpinCore.Utility;
using SRXDCustomVisuals.Core;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace SRXDCustomVisuals.Plugin; 

public class Patches {
    private static VisualsScene currentScene;
    private static Dictionary<string, VisualsScene> scenes = new();
    private static NoteClearType previousClearType;
    private static string assetBundlesPath = Path.Combine(AssetBundleSystem.CUSTOM_DATA_PATH, "AssetBundles");
    private static float2[] cachedSpectrum = new float2[256];
    private static ComputeBuffer computeBuffer;
    private static readonly int CUSTOM_SPECTRUM_BUFFER = Shader.PropertyToID("_CustomSpectrumBuffer");

    private static bool TryGetScene(string uniqueName, CustomVisualsInfo info, out VisualsScene scene) {
        if (scenes.TryGetValue(uniqueName, out scene))
            return true;
        
        var modules = new List<VisualsModule>();
        bool success = true;
        var assetBundles = new Dictionary<string, AssetBundle>();

        foreach (string bundleName in info.AssetBundles) {
            if (AssetBundleUtility.TryGetAssetBundle(assetBundlesPath, bundleName, out var bundle))
                assetBundles.Add(bundleName, bundle);
            else {
                Plugin.Logger.LogWarning($"Failed to load asset bundle {bundleName}");
                success = false;
            }
        }
            
        if (!success)
            return false;

        foreach (var moduleReference in info.Modules) {
            if (assetBundles.TryGetValue(moduleReference.Bundle, out var bundle)) {
                var module = bundle.LoadAsset<VisualsModule>(moduleReference.Asset);

                if (module != null)
                    modules.Add(module);
            }
            else {
                Plugin.Logger.LogWarning($"Could not find asset bundle {moduleReference.Bundle}");
                success = false;
            }
        }
            
        if (!success)
            return false;

        scene = new VisualsScene(modules);
        scenes.Add(uniqueName, scene);

        return true;
    }

    private static float Boost(float x) => 1f - 1f / (100f * x + 1f);

    private static BackgroundAssetReference OverrideBackgroundIfStoryboardHasOverride(BackgroundAssetReference defaultBackground, PlayableTrackDataHandle handle) {
        var trackInfoRef = handle.Setup.TrackDataSegments[0].trackInfoRef;

        if (!Plugin.EnableCustomVisuals.Value || !trackInfoRef.IsCustomFile)
            return defaultBackground;

        if (CustomChartUtility.TryGetCustomData<CustomVisualsInfo>(trackInfoRef.customFile, "CustomVisualsInfo", out var customVisualsInfo)
            && customVisualsInfo.HasCustomVisuals
            && customVisualsInfo.DisableBaseBackground
            && TryGetScene(trackInfoRef.UniqueName, customVisualsInfo, out _))
            return BackgroundSystem.UtilityBackgrounds.lowMotionBackground;
        
        return defaultBackground;
    }

    [HarmonyPatch(typeof(Game), nameof(Game.Update)), HarmonyPostfix]
    private static void Game_Update_Postfix() {
        var spectrumProcessor = TrackTrackerAndUtils.Instance.SpectrumProcessor;
        var smoothedBandsLeft = spectrumProcessor.GetSmoothedBands(AudioChannels.LeftChannel);
        var smoothedBandsRight = spectrumProcessor.GetSmoothedBands(AudioChannels.RightChannel);
        
        if (!smoothedBandsLeft.IsCreated || !smoothedBandsRight.IsCreated)
            return;

        for (int i = 0; i < 256; i++)
            cachedSpectrum[i] = new float2(Boost(smoothedBandsLeft[i]), Boost(smoothedBandsRight[i]));

        computeBuffer ??= new ComputeBuffer(256, UnsafeUtility.SizeOf<float2>(), ComputeBufferType.Structured);
        computeBuffer.SetData(cachedSpectrum);
        Shader.SetGlobalBuffer(CUSTOM_SPECTRUM_BUFFER, computeBuffer);
    }

    [HarmonyPatch(typeof(Track), "Awake"), HarmonyPostfix]
    private static void Track_Awake_Postfix(Track __instance) {
        string customAssetBundlePath = Path.Combine(AssetBundleSystem.CUSTOM_DATA_PATH, "AssetBundles");

        if (!Directory.Exists(customAssetBundlePath))
            Directory.CreateDirectory(customAssetBundlePath);
    }

    [HarmonyPatch(typeof(Track), nameof(Track.PlayTrack)), HarmonyPostfix]
    private static void Track_PlayTrack_Postfix(Track __instance) {
        var data = __instance.playStateFirst.trackData;
        var trackInfoRef = data.TrackInfoRef;
        var mainCamera = MainCamera.Instance.GetComponent<Camera>();
        
        mainCamera.clearFlags = CameraClearFlags.Skybox;

        if (currentScene != null) {
            currentScene.Unload();
            currentScene = null;
        }
        
        if (!Plugin.EnableCustomVisuals.Value || !trackInfoRef.IsCustomFile)
            return;
        
        if (!CustomChartUtility.TryGetCustomData<CustomVisualsInfo>(trackInfoRef.customFile, "CustomVisualsInfo", out var customVisualsInfo)
            || !customVisualsInfo.HasCustomVisuals
            || !TryGetScene(trackInfoRef.UniqueName, customVisualsInfo, out currentScene))
            return;

        if (customVisualsInfo.DisableBaseBackground) {
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = Color.black;
        }

        var resources = new VisualsResources();
        
        resources.AddResource("spectrumTexture", SpectrumProcessor.Instance.SpectrumTexture);
        
        currentScene.Load(new[] { null, mainCamera.transform }, new VisualsParams(), new VisualsResources());
    }

    [HarmonyPatch(typeof(Track), nameof(Track.ReturnToPickTrack)), HarmonyPostfix]
    private static void Track_ReturnToPickTrack_Postfix() {
        MainCamera.Instance.GetComponent<Camera>().clearFlags = CameraClearFlags.Skybox;
        
        if (currentScene == null)
            return;
        
        currentScene.Unload();
        currentScene = null;
    }

    [HarmonyPatch(typeof(TrackGameplayLogic), nameof(TrackGameplayLogic.UpdateNoteStateInternal)), HarmonyPrefix]
    private static void TrackGameplayLogic_UpdateNoteStateInternal_Prefix(PlayState playState, int noteIndex) => previousClearType = playState.noteStates[noteIndex].clearType;
    
    [HarmonyPatch(typeof(TrackGameplayLogic), nameof(TrackGameplayLogic.UpdateNoteStateInternal)), HarmonyPostfix]
    private static void TrackGameplayLogic_UpdateNoteStateInternal_Postfix(PlayState playState, int noteIndex) {
        if (currentScene == null)
            return;
        
        var clearType = playState.noteStates[noteIndex].clearType;
        
        if (clearType == previousClearType || clearType >= NoteClearType.ClearedEarly)
            return;

        var trackData = playState.trackData;
        var note = trackData.GetNote(noteIndex);

        switch (note.NoteType) {
            case NoteType.Match when clearType == NoteClearType.Cleared:
                currentScene.InvokeEvent("HitMatch");
                
                break;
            case NoteType.DrumStart:
                var drumNote = trackData.NoteData.GetDrumForNoteIndex(noteIndex);
                
                if (drumNote.HasValue && (drumNote.Value.IsHold ? clearType == NoteClearType.ClearedInitialHit : clearType == NoteClearType.Cleared))
                    currentScene.InvokeEvent("HitBeat");

                break;
            case NoteType.SpinRightStart when clearType == NoteClearType.ClearedInitialHit:
                currentScene.InvokeEvent("HitSpinRight");

                break;
            case NoteType.SpinLeftStart when clearType == NoteClearType.ClearedInitialHit:
                currentScene.InvokeEvent("HitSpinLeft");

                break;
            case NoteType.Tap when clearType == NoteClearType.Cleared:
            case NoteType.HoldStart when clearType == NoteClearType.ClearedInitialHit:
                currentScene.InvokeEvent("HitTap");

                break;
            case NoteType.ScratchStart when clearType == NoteClearType.Cleared:
                currentScene.InvokeEvent("HitScratch");
                
                break;
        }
    }

    [HarmonyPatch(typeof(PlayableTrackDataHandle), "Loading"), HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> PlayableTrackDataHandle_Loading_Transpiler(IEnumerable<CodeInstruction> instructions) {
        var instructionsList = instructions.ToList();
        var operations = new EnumerableOperation<CodeInstruction>();
        var Patches_OverrideBackgroundIfStoryboardHasOverride = typeof(Patches).GetMethod(nameof(OverrideBackgroundIfStoryboardHasOverride), BindingFlags.NonPublic | BindingFlags.Static);

        var match = PatternMatching.Match(instructionsList, new Func<CodeInstruction, bool>[] {
            instr => instr.opcode == OpCodes.Ldloc_1 // backgroundAssetReference
        }).ElementAt(1)[0];
        
        operations.Insert(match.End, new CodeInstruction[] {
            new (OpCodes.Ldarg_0), // this
            new (OpCodes.Call, Patches_OverrideBackgroundIfStoryboardHasOverride),
            new (OpCodes.Stloc_1), // backgroundAssetReference
            new (OpCodes.Ldloc_1) // backgroundAssetReference
        });

        return operations.Enumerate(instructionsList);
    }
}