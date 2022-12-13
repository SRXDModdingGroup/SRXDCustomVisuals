using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Newtonsoft.Json;
using SMU.Utilities;
using SpinCore.Utility;
using SRXDCustomVisuals.Core;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace SRXDCustomVisuals.Plugin; 

public class Patches {
    private static readonly int CUSTOM_SPECTRUM_BUFFER = Shader.PropertyToID("_CustomSpectrumBuffer");
    private static readonly string ASSET_BUNDLES_PATH = Path.Combine(AssetBundleSystem.CUSTOM_DATA_PATH, "AssetBundles");
    private static readonly string BACKGROUNDS_PATH = Path.Combine(AssetBundleSystem.CUSTOM_DATA_PATH, "Backgrounds");
    
    private static VisualsSceneLoader currentSceneLoader;
    private static VisualsSceneManager sceneManager = new();
    private static Dictionary<string, CustomVisualsInfo> cachedVisualsInfo = new();
    private static Dictionary<string, BackgroundDefinition> cachedBackgroundDefinitions = new();
    private static Dictionary<string, VisualsSceneLoader> sceneLoaders = new();
    private static float2[] cachedSpectrum = new float2[256];
    private static ComputeBuffer computeBuffer;
    private static NoteClearType previousClearType;
    private static bool holding;
    private static bool beatHolding;
    private static bool spinningRight;
    private static bool spinningLeft;
    private static bool scratching;
    private static bool invokeHitMatch;
    private static bool invokeHitBeat;
    private static bool invokeHitSpinRight;
    private static bool invokeHitSpinLeft;
    private static bool invokeHitTap;
    private static bool invokeHitScratch;
    private static IVisualsEvent hitMatchEvent;
    private static IVisualsEvent hitBeatEvent;
    private static IVisualsEvent hitSpinRightEvent;
    private static IVisualsEvent hitSpinLeftEvent;
    private static IVisualsEvent hitTapEvent;
    private static IVisualsEvent hitScratchEvent;
    private static IVisualsProperty holdingProperty;
    private static IVisualsProperty beatHoldingProperty;
    private static IVisualsProperty spinningRightProperty;
    private static IVisualsProperty spinningLeftProperty;
    private static IVisualsProperty scratchingProperty;

    private static bool TryGetCustomVisualsInfo(TrackInfoAssetReference trackInfoRef, out CustomVisualsInfo customVisualsInfo) {
        if (!trackInfoRef.IsCustomFile) {
            customVisualsInfo = null;

            return false;
        }
        
        string uniqueName = trackInfoRef.UniqueName;

        if (cachedVisualsInfo.TryGetValue(uniqueName, out customVisualsInfo))
            return true;

        if (!CustomChartUtility.TryGetCustomData(trackInfoRef.customFile, "CustomVisualsInfo", out customVisualsInfo))
            return false;

        cachedVisualsInfo.Add(uniqueName, customVisualsInfo);

        return true;
    }

    private static bool TryGetBackgroundDefinition(string name, out BackgroundDefinition backgroundDefinition) {
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

    private static bool TryGetSceneLoader(string name, out VisualsSceneLoader sceneLoader) {
        if (string.IsNullOrWhiteSpace(name)) {
            sceneLoader = null;

            return false;
        }
        
        if (sceneLoaders.TryGetValue(name, out sceneLoader))
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

        sceneLoader = new VisualsSceneLoader(elements);
        sceneLoaders.Add(name, sceneLoader);

        return true;
    }

    private static float Boost(float x) => 1f - 1f / (100f * x + 1f);

    private static BackgroundAssetReference OverrideBackgroundIfVisualsInfoHasOverride(BackgroundAssetReference defaultBackground, PlayableTrackDataHandle handle) {
        var trackInfoRef = handle.Setup.TrackDataSegments[0].trackInfoRef;

        if (Plugin.EnableCustomVisuals.Value
            && TryGetCustomVisualsInfo(trackInfoRef, out var customVisualsInfo)
            && TryGetBackgroundDefinition(customVisualsInfo.Background, out var background)
            && background.DisableBaseBackground)
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
        if (!Directory.Exists(ASSET_BUNDLES_PATH))
            Directory.CreateDirectory(ASSET_BUNDLES_PATH);
        
        if (!Directory.Exists(BACKGROUNDS_PATH))
            Directory.CreateDirectory(BACKGROUNDS_PATH);
    }

    [HarmonyPatch(typeof(Track), nameof(Track.PlayTrack)), HarmonyPostfix]
    private static void Track_PlayTrack_Postfix(Track __instance) {
        var data = __instance.playStateFirst.trackData;
        var trackInfoRef = data.TrackInfoRef;
        var mainCamera = MainCamera.Instance.GetComponent<Camera>();
        
        mainCamera.clearFlags = CameraClearFlags.Skybox;

        if (currentSceneLoader != null) {
            currentSceneLoader.Unload();
            currentSceneLoader = null;
            sceneManager.Clear();
        }
        
        if (!Plugin.EnableCustomVisuals.Value
            || !TryGetCustomVisualsInfo(trackInfoRef, out var customVisualsInfo)
            || !TryGetBackgroundDefinition(customVisualsInfo.Background, out var backgroundDefinition)
            || !TryGetSceneLoader(customVisualsInfo.Background, out currentSceneLoader))
            return;

        if (backgroundDefinition.DisableBaseBackground) {
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = Color.black;
        }

        var scene = currentSceneLoader.Load(new[] { null, mainCamera.transform });
        
        Dispatcher.QueueForNextFrame(() => {
            sceneManager.SetScene(scene);
            sceneManager.InitControllers(new VisualsParams(), new VisualsResources());
            hitMatchEvent = sceneManager.GetEvent("HitMatch");
            hitBeatEvent = sceneManager.GetEvent("HitBeat");
            hitSpinRightEvent = sceneManager.GetEvent("HitSpinRight");
            hitSpinLeftEvent = sceneManager.GetEvent("HitSpinLeft");
            hitTapEvent = sceneManager.GetEvent("HitTap");
            hitScratchEvent = sceneManager.GetEvent("HitScratch");
            holdingProperty = sceneManager.GetProperty("Holding");
            beatHoldingProperty = sceneManager.GetProperty("BeatHolding");
            spinningRightProperty = sceneManager.GetProperty("SpinningRight");
            spinningLeftProperty = sceneManager.GetProperty("SpinningLeft");
            scratchingProperty = sceneManager.GetProperty("Scratching");
        });
    }

    [HarmonyPatch(typeof(Track), nameof(Track.ReturnToPickTrack)), HarmonyPostfix]
    private static void Track_ReturnToPickTrack_Postfix() {
        MainCamera.Instance.GetComponent<Camera>().clearFlags = CameraClearFlags.Skybox;
        
        if (currentSceneLoader == null)
            return;
        
        currentSceneLoader.Unload();
        currentSceneLoader = null;
        sceneManager.Clear();
    }
    
    [HarmonyPatch(typeof(PlayState.ScoreState), nameof(PlayState.ScoreState.UpdateNoteStates)), HarmonyPrefix]
    private static void ScoreState_UpdateNoteStates_Prefix() {
        invokeHitMatch = false;
        invokeHitBeat = false;
        invokeHitSpinRight = false;
        invokeHitSpinLeft = false;
        invokeHitTap = false;
        invokeHitScratch = false;
        holding = false;
        beatHolding = false;
        spinningRight = false;
        spinningLeft = false;
        scratching = false;
    }
    
    [HarmonyPatch(typeof(PlayState.ScoreState), nameof(PlayState.ScoreState.UpdateNoteStates)), HarmonyPostfix]
    private static void ScoreState_UpdateNoteStates_Postfix() {
        if (!sceneManager.HasScene)
            return;
        
        if (invokeHitMatch)
            hitMatchEvent.Invoke();
        
        if (invokeHitBeat)
            hitBeatEvent.Invoke();
        
        if (invokeHitSpinRight)
            hitSpinRightEvent.Invoke();
        
        if (invokeHitSpinLeft)
            hitSpinLeftEvent.Invoke();
        
        if (invokeHitTap)
            hitTapEvent.Invoke();
        
        if (invokeHitScratch)
            hitScratchEvent.Invoke();
        
        holdingProperty.SetBool(holding);
        beatHoldingProperty.SetBool(beatHolding);
        spinningRightProperty.SetBool(spinningRight);
        spinningLeftProperty.SetBool(spinningLeft);
        scratchingProperty.SetBool(scratching);
    }

    [HarmonyPatch(typeof(TrackGameplayLogic), nameof(TrackGameplayLogic.UpdateNoteStateInternal)), HarmonyPrefix]
    private static void TrackGameplayLogic_UpdateNoteStateInternal_Prefix(PlayState playState, int noteIndex) => previousClearType = playState.noteStates[noteIndex].clearType;
    
    [HarmonyPatch(typeof(TrackGameplayLogic), nameof(TrackGameplayLogic.UpdateNoteStateInternal)), HarmonyPostfix]
    private static void TrackGameplayLogic_UpdateNoteStateInternal_Postfix(PlayState playState, int noteIndex) {
        if (!sceneManager.HasScene)
            return;
        
        var trackData = playState.trackData;
        var clearType = playState.noteStates[noteIndex].clearType;
        var note = trackData.GetNote(noteIndex);
        var noteType = note.NoteType;
        
        if (noteType == NoteType.DrumStart && clearType is NoteClearType.ClearedInitialHit or NoteClearType.MissedInitialHit) {
            var drumNote = trackData.NoteData.GetDrumForNoteIndex(noteIndex).GetValueOrDefault();
            ref var sustainNoteState = ref playState.scoreState.GetSustainState(drumNote.FirstNoteIndex);

            if (sustainNoteState.isSustained && playState.currentTrackTick < trackData.GetNote(drumNote.LastNoteIndex).tick)
                beatHolding = true;
        }

        if (clearType == previousClearType || clearType >= NoteClearType.ClearedEarly)
            return;
        
        switch (noteType) {
            case NoteType.Match when clearType == NoteClearType.Cleared:
                invokeHitMatch = true;

                break;
            case NoteType.DrumStart:
                var drumNote = trackData.NoteData.GetDrumForNoteIndex(noteIndex).GetValueOrDefault();

                if (drumNote.IsHold ? clearType == NoteClearType.ClearedInitialHit : clearType == NoteClearType.Cleared)
                    invokeHitBeat = true;

                break;
            case NoteType.SpinRightStart when clearType == NoteClearType.ClearedInitialHit:
                invokeHitSpinRight = true;

                break;
            case NoteType.SpinLeftStart when clearType == NoteClearType.ClearedInitialHit:
                invokeHitSpinLeft = true;

                break;
            case NoteType.Tap when clearType == NoteClearType.Cleared:
            case NoteType.HoldStart when clearType == NoteClearType.ClearedInitialHit:
                invokeHitTap = true;

                break;
            case NoteType.ScratchStart when clearType == NoteClearType.Cleared:
                invokeHitScratch = true;

                break;
        }
    }

    [HarmonyPatch(typeof(FreestyleSectionLogic), nameof(FreestyleSectionLogic.UpdateFreestyleSectionState)), HarmonyPostfix]
    private static void FreestyleSectionLogic_UpdateFreestyleSectionState_Postfix(PlayState playState, int noteIndex) {
        var sustainSection = playState.trackData.NoteData.FreestyleSections.GetSectionForNote(noteIndex);
        
        if (!sustainSection.HasValue)
            return;
        
        ref var sustainNoteState = ref playState.scoreState.GetSustainState(noteIndex);

        if (sustainNoteState.isSustained && playState.currentTrackTick < sustainSection.Value.EndTick)
            holding = true;
    }
    
    [HarmonyPatch(typeof(SpinSectionLogic), nameof(SpinSectionLogic.UpdateSpinSectionState)), HarmonyPostfix]
    private static void SpinSectionLogic_UpdateSpinSectionState_Postfix(PlayState playState, int noteIndex) {
        var spinSection = playState.trackData.NoteData.SpinnerSections.GetSectionForNote(noteIndex);
        
        if (!spinSection.HasValue)
            return;
        
        ref var sustainNoteState = ref playState.scoreState.GetSustainState(noteIndex);

        if (!sustainNoteState.isSustained || playState.currentTrackTick >= playState.trackData.GetNote(spinSection.Value.LastNoteIndex).tick)
            return;
        
        if (spinSection.Value.direction == 1)
            spinningRight = true;
        else
            spinningLeft = true;
    }
    
    [HarmonyPatch(typeof(ScratchSectionLogic), nameof(ScratchSectionLogic.UpdateScratchSectionState)), HarmonyPostfix]
    private static void ScratchSectionLogic_UpdateScratchSectionState_Postfix(PlayState playState, int noteIndex) {
        var scratchSection = playState.trackData.NoteData.ScratchSections.GetSectionForNote(noteIndex);
        
        if (!scratchSection.HasValue)
            return;
        
        ref var sustainNoteState = ref playState.scoreState.GetSustainState(noteIndex);

        if (sustainNoteState.isSustained && playState.currentTrackTick < playState.trackData.GetNote(scratchSection.Value.LastNoteIndex).tick)
            scratching = true;
    }

    [HarmonyPatch(typeof(PlayableTrackDataHandle), "Loading"), HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> PlayableTrackDataHandle_Loading_Transpiler(IEnumerable<CodeInstruction> instructions) {
        var instructionsList = instructions.ToList();
        var operations = new EnumerableOperation<CodeInstruction>();
        var Patches_OverrideBackgroundIfStoryboardHasOverride = typeof(Patches).GetMethod(nameof(OverrideBackgroundIfVisualsInfoHasOverride), BindingFlags.NonPublic | BindingFlags.Static);

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