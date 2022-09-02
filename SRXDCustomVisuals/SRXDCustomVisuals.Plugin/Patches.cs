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
using UnityEngine;

namespace SRXDCustomVisuals.Plugin; 

public class Patches {
    private static VisualsScene currentScene;
    private static Dictionary<string, VisualsScene> scenes = new();
    private static NoteClearType previousClearType;

    private static BackgroundAssetReference OverrideBackgroundIfStoryboardHasOverride(BackgroundAssetReference defaultBackground, PlayableTrackDataHandle handle) {
        var info = handle.Setup.TrackDataSegments[0].trackInfoRef;

        if (!Plugin.EnableCustomVisuals.Value || !info.IsCustomFile)
            return defaultBackground;

        var customVisualsInfo = CustomChartUtility.GetCustomData<CustomVisualsInfo>(info.customFile, "CustomVisualsInfo");

        if (!customVisualsInfo.HasCustomVisuals || !customVisualsInfo.DisableBaseBackground)
            return defaultBackground;

        return BackgroundSystem.UtilityBackgrounds.lowMotionBackground;
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
        var info = data.TrackInfoRef;
        var mainCamera = MainCamera.Instance.GetComponent<Camera>();
        
        mainCamera.clearFlags = CameraClearFlags.Skybox;

        if (currentScene != null) {
            currentScene.Unload();
            currentScene = null;
        }
        
        if (!Plugin.EnableCustomVisuals.Value || !info.IsCustomFile)
            return;

        var customVisualsInfo = CustomChartUtility.GetCustomData<CustomVisualsInfo>(info.customFile, "CustomVisualsInfo");
        
        if (!customVisualsInfo.HasCustomVisuals)
            return;

        if (customVisualsInfo.DisableBaseBackground) {
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = Color.black;
        }

        if (!scenes.TryGetValue(info.Guid, out currentScene)) {
            var modules = new List<VisualsModule>();

            foreach (var moduleReference in customVisualsInfo.Modules) {
                if (!AssetBundleUtility.TryGetAssetBundle(Path.Combine(AssetBundleSystem.CUSTOM_DATA_PATH, "AssetBundles"), moduleReference.AssetBundleName, out var bundle))
                    continue;

                var module = bundle.LoadAsset<VisualsModule>(moduleReference.AssetName);
                
                if (module != null)
                    modules.Add(module);
            }

            currentScene = new VisualsScene(modules);
            scenes.Add(info.Guid, currentScene);
        }
        
        currentScene.Load(new[] { null, mainCamera.transform });
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