using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using SMU.Utilities;

namespace SRXDCustomVisuals.Plugin; 

public class Patches {
    private static NoteClearType previousClearType;
    private static VisualsSceneManager visualsSceneManager = new();
    private static NoteEventController noteEventController = new(255, 11);
    private static SpectrumBufferController spectrumBufferController = new();

    private static BackgroundAssetReference OverrideBackgroundIfVisualsInfoHasOverride(BackgroundAssetReference defaultBackground, PlayableTrackDataHandle handle) {
        if (Plugin.EnableCustomVisuals.Value)
            return visualsSceneManager.GetBackgroundForScene(defaultBackground, handle.Setup.TrackDataSegments[0].trackInfoRef);
        
        return defaultBackground;
    }

    [HarmonyPatch(typeof(Game), nameof(Game.Update)), HarmonyPostfix]
    private static void Game_Update_Postfix() => spectrumBufferController.Update();

    [HarmonyPatch(typeof(Track), "Awake"), HarmonyPostfix]
    private static void Track_Awake_Postfix(Track __instance) => VisualsSceneManager.CreateDirectories();

    [HarmonyPatch(typeof(Track), nameof(Track.PlayTrack)), HarmonyPostfix]
    private static void Track_PlayTrack_Postfix(Track __instance) {
        if (!Plugin.EnableCustomVisuals.Value)
            return;
        
        visualsSceneManager.LoadScene(__instance.playStateFirst.trackData.TrackInfoRef);
        noteEventController.Reset();
    }

    [HarmonyPatch(typeof(Track), nameof(Track.ReturnToPickTrack)), HarmonyPostfix]
    private static void Track_ReturnToPickTrack_Postfix() => visualsSceneManager.UnloadScene();

    [HarmonyPatch(typeof(PlayState.ScoreState), nameof(PlayState.ScoreState.UpdateNoteStates)), HarmonyPrefix]
    private static void ScoreState_UpdateNoteStates_Prefix() => noteEventController.Listen();

    [HarmonyPatch(typeof(PlayState.ScoreState), nameof(PlayState.ScoreState.UpdateNoteStates)), HarmonyPostfix]
    private static void ScoreState_UpdateNoteStates_Postfix() => noteEventController.Send();

    [HarmonyPatch(typeof(TrackGameplayLogic), nameof(TrackGameplayLogic.UpdateNoteStateInternal)), HarmonyPrefix]
    private static void TrackGameplayLogic_UpdateNoteStateInternal_Prefix(PlayState playState, int noteIndex) => previousClearType = playState.noteStates[noteIndex].clearType;
    
    [HarmonyPatch(typeof(TrackGameplayLogic), nameof(TrackGameplayLogic.UpdateNoteStateInternal)), HarmonyPostfix]
    private static void TrackGameplayLogic_UpdateNoteStateInternal_Postfix(PlayState playState, int noteIndex) {
        var trackData = playState.trackData;
        var clearType = playState.noteStates[noteIndex].clearType;
        var note = trackData.GetNote(noteIndex);
        var noteType = note.NoteType;
        
        if (noteType == NoteType.DrumStart && clearType is NoteClearType.ClearedInitialHit or NoteClearType.MissedInitialHit) {
            var drumNote = trackData.NoteData.GetDrumForNoteIndex(noteIndex).GetValueOrDefault();
            ref var sustainNoteState = ref playState.scoreState.GetSustainState(drumNote.FirstNoteIndex);

            if (sustainNoteState.isSustained && playState.currentTrackTick < trackData.GetNote(drumNote.LastNoteIndex).tick)
                noteEventController.Hold((byte) NoteIndex.HoldBeat);
        }

        if (clearType == previousClearType || clearType >= NoteClearType.ClearedEarly)
            return;
        
        switch (noteType) {
            case NoteType.Match when clearType == NoteClearType.Cleared:
                noteEventController.Hit((byte) NoteIndex.HitMatch);

                break;
            case NoteType.DrumStart:
                var drumNote = trackData.NoteData.GetDrumForNoteIndex(noteIndex).GetValueOrDefault();

                if (drumNote.IsHold ? clearType == NoteClearType.ClearedInitialHit : clearType == NoteClearType.Cleared)
                    noteEventController.Hit((byte) NoteIndex.HitBeat);

                break;
            case NoteType.SpinRightStart when clearType == NoteClearType.ClearedInitialHit:
                noteEventController.Hit((byte) NoteIndex.HitSpinRight);

                break;
            case NoteType.SpinLeftStart when clearType == NoteClearType.ClearedInitialHit:
                noteEventController.Hit((byte) NoteIndex.HitSpinLeft);

                break;
            case NoteType.Tap when clearType == NoteClearType.Cleared:
            case NoteType.HoldStart when clearType == NoteClearType.ClearedInitialHit:
                noteEventController.Hit((byte) NoteIndex.HitTap);

                break;
            case NoteType.ScratchStart when clearType == NoteClearType.Cleared:
                noteEventController.Hit((byte) NoteIndex.HitScratch);

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
            noteEventController.Hold((byte) NoteIndex.Hold);
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
            noteEventController.Hold((byte) NoteIndex.HoldSpinRight);
        else
            noteEventController.Hold((byte) NoteIndex.HoldSpinLeft);
    }
    
    [HarmonyPatch(typeof(ScratchSectionLogic), nameof(ScratchSectionLogic.UpdateScratchSectionState)), HarmonyPostfix]
    private static void ScratchSectionLogic_UpdateScratchSectionState_Postfix(PlayState playState, int noteIndex) {
        var scratchSection = playState.trackData.NoteData.ScratchSections.GetSectionForNote(noteIndex);
        
        if (!scratchSection.HasValue)
            return;
        
        ref var sustainNoteState = ref playState.scoreState.GetSustainState(noteIndex);

        if (sustainNoteState.isSustained && playState.currentTrackTick < playState.trackData.GetNote(scratchSection.Value.LastNoteIndex).tick)
            noteEventController.Hold((byte) NoteIndex.HoldScratch);
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