using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using SMU.Utilities;
using SRXDCustomVisuals.Core;
using UnityEngine;

namespace SRXDCustomVisuals.Plugin; 

public class Patches {
    private static NoteClearType previousClearType;
    private static VisualsInfoAccessor visualsInfoAccessor = new();
    private static VisualsSceneManager visualsSceneManager = new();
    private static TrackVisualsEventPlayback eventPlayback = new();
    private static SequenceEditor sequenceEditor;
    private static NoteEventController noteEventController = new(11);
    private static SpectrumBufferController spectrumBufferController = new();

    private static BackgroundAssetReference OverrideBackgroundIfVisualsInfoHasOverride(BackgroundAssetReference defaultBackground, PlayableTrackDataHandle handle) {
        if (!Plugin.EnableCustomVisuals.Value)
            return defaultBackground;
        
        return visualsSceneManager.GetBackgroundForScene(
            defaultBackground,
            visualsInfoAccessor.GetCustomVisualsInfo(handle.Setup.TrackDataSegments[0].trackInfoRef).Background);
    }

    [HarmonyPatch(typeof(Game), nameof(Game.Update)), HarmonyPostfix]
    private static void Game_Update_Postfix() => spectrumBufferController.Update();

    [HarmonyPatch(typeof(Track), "Awake"), HarmonyPostfix]
    private static void Track_Awake_Postfix(Track __instance) {
        new GameObject("Visuals Event Manager", typeof(VisualsEventManager));
        sequenceEditor = new GameObject("Sequence Editor", typeof(SequenceEditor)).GetComponent<SequenceEditor>();
        VisualsSceneManager.CreateDirectories();
        eventPlayback.SetSequence(new TrackVisualsEventSequence());
    }

    [HarmonyPatch(typeof(Track), nameof(Track.PlayTrack)), HarmonyPostfix]
    private static void Track_PlayTrack_Postfix(Track __instance) {
        noteEventController.Reset();

        var playState = __instance.playStateFirst;
        var trackData = playState.trackData;

        var customVisualsInfo = visualsInfoAccessor.GetCustomVisualsInfo(trackData.TrackInfoRef);
        var eventSequence = new TrackVisualsEventSequence(customVisualsInfo);

        if (Plugin.EnableCustomVisuals.Value)
            visualsSceneManager.LoadScene(customVisualsInfo.Background);

        eventPlayback.SetSequence(eventSequence);
        eventPlayback.Play(playState.currentTrackTick);
        sequenceEditor.Init(eventSequence, playState);
    }

    [HarmonyPatch(typeof(Track), nameof(Track.ReturnToPickTrack)), HarmonyPostfix]
    private static void Track_ReturnToPickTrack_Postfix() {
        noteEventController.Reset();
        visualsSceneManager.UnloadScene();
        eventPlayback.SetSequence(new TrackVisualsEventSequence());
        sequenceEditor.Exit();
        sequenceEditor.Visible = false;
    }

    [HarmonyPatch(typeof(Track), nameof(Track.Update)), HarmonyPostfix]
    private static void Track_Update_Postfix(Track __instance) {
        var playState = __instance.playStateFirst;

        if (playState.playStateStatus == PlayStateStatus.Playing)
            eventPlayback.Advance(playState.currentTrackTick);
    }

    [HarmonyPatch(typeof(Track), nameof(Track.SetDebugPaused)), HarmonyPostfix]
    private static void Track_SetDebugPaused_Postfix(Track __instance, bool paused) {
        if (paused)
            eventPlayback.Pause();
        else
            eventPlayback.Play(__instance.playStateFirst.currentTrackTick);
    }

    [HarmonyPatch(typeof(PlayState.ScoreState), nameof(PlayState.ScoreState.UpdateNoteStates)), HarmonyPrefix]
    private static void ScoreState_UpdateNoteStates_Prefix() {
        if (Plugin.EnableCustomVisuals.Value)
            noteEventController.Listen();
    }

    [HarmonyPatch(typeof(PlayState.ScoreState), nameof(PlayState.ScoreState.UpdateNoteStates)), HarmonyPostfix]
    private static void ScoreState_UpdateNoteStates_Postfix() {
        if (Plugin.EnableCustomVisuals.Value)
            noteEventController.Send();
    }

    [HarmonyPatch(typeof(TrackGameplayLogic), nameof(TrackGameplayLogic.UpdateNoteStateInternal)), HarmonyPrefix]
    private static void TrackGameplayLogic_UpdateNoteStateInternal_Prefix(PlayState playState, int noteIndex) => previousClearType = playState.noteStates[noteIndex].clearType;
    
    [HarmonyPatch(typeof(TrackGameplayLogic), nameof(TrackGameplayLogic.UpdateNoteStateInternal)), HarmonyPostfix]
    private static void TrackGameplayLogic_UpdateNoteStateInternal_Postfix(PlayState playState, int noteIndex) {
        if (!Plugin.EnableCustomVisuals.Value)
            return;
        
        var trackData = playState.trackData;
        var clearType = playState.noteStates[noteIndex].clearType;
        var note = trackData.GetNote(noteIndex);
        var noteType = note.NoteType;
        
        if (noteType == NoteType.DrumStart && clearType is NoteClearType.ClearedInitialHit or NoteClearType.MissedInitialHit) {
            var drumNote = trackData.NoteData.GetDrumForNoteIndex(noteIndex).GetValueOrDefault();
            ref var sustainNoteState = ref playState.scoreState.GetSustainState(drumNote.FirstNoteIndex);

            if (sustainNoteState.isSustained && playState.currentTrackTick < trackData.GetNote(drumNote.LastNoteIndex).tick)
                noteEventController.Hold((int) NoteIndex.HoldBeat);
        }

        if (clearType == previousClearType || clearType >= NoteClearType.ClearedEarly)
            return;
        
        switch (noteType) {
            case NoteType.Match when clearType == NoteClearType.Cleared:
                noteEventController.Hit((int) NoteIndex.HitMatch);

                break;
            case NoteType.DrumStart:
                var drumNote = trackData.NoteData.GetDrumForNoteIndex(noteIndex).GetValueOrDefault();

                if (drumNote.IsHold ? clearType == NoteClearType.ClearedInitialHit : clearType == NoteClearType.Cleared)
                    noteEventController.Hit((int) NoteIndex.HitBeat);

                break;
            case NoteType.SpinRightStart when clearType == NoteClearType.ClearedInitialHit:
                noteEventController.Hit((int) NoteIndex.HitSpinRight);

                break;
            case NoteType.SpinLeftStart when clearType == NoteClearType.ClearedInitialHit:
                noteEventController.Hit((int) NoteIndex.HitSpinLeft);

                break;
            case NoteType.Tap when clearType == NoteClearType.Cleared:
            case NoteType.HoldStart when clearType == NoteClearType.ClearedInitialHit:
                noteEventController.Hit((int) NoteIndex.HitTap);

                break;
            case NoteType.ScratchStart when clearType == NoteClearType.Cleared:
                noteEventController.Hit((int) NoteIndex.HitScratch);

                break;
        }
    }

    [HarmonyPatch(typeof(FreestyleSectionLogic), nameof(FreestyleSectionLogic.UpdateFreestyleSectionState)), HarmonyPostfix]
    private static void FreestyleSectionLogic_UpdateFreestyleSectionState_Postfix(PlayState playState, int noteIndex) {
        if (!Plugin.EnableCustomVisuals.Value)
            return;
        
        var sustainSection = playState.trackData.NoteData.FreestyleSections.GetSectionForNote(noteIndex);
        
        if (!sustainSection.HasValue)
            return;
        
        ref var sustainNoteState = ref playState.scoreState.GetSustainState(noteIndex);

        if (sustainNoteState.isSustained && playState.currentTrackTick < sustainSection.Value.EndTick)
            noteEventController.Hold((int) NoteIndex.Hold);
    }
    
    [HarmonyPatch(typeof(SpinSectionLogic), nameof(SpinSectionLogic.UpdateSpinSectionState)), HarmonyPostfix]
    private static void SpinSectionLogic_UpdateSpinSectionState_Postfix(PlayState playState, int noteIndex) {
        if (!Plugin.EnableCustomVisuals.Value)
            return;
        
        var spinSection = playState.trackData.NoteData.SpinnerSections.GetSectionForNote(noteIndex);
        
        if (!spinSection.HasValue)
            return;
        
        ref var sustainNoteState = ref playState.scoreState.GetSustainState(noteIndex);

        if (!sustainNoteState.isSustained || playState.currentTrackTick >= playState.trackData.GetNote(spinSection.Value.LastNoteIndex).tick)
            return;
        
        if (spinSection.Value.direction == 1)
            noteEventController.Hold((int) NoteIndex.HoldSpinRight);
        else
            noteEventController.Hold((int) NoteIndex.HoldSpinLeft);
    }
    
    [HarmonyPatch(typeof(ScratchSectionLogic), nameof(ScratchSectionLogic.UpdateScratchSectionState)), HarmonyPostfix]
    private static void ScratchSectionLogic_UpdateScratchSectionState_Postfix(PlayState playState, int noteIndex) {
        if (!Plugin.EnableCustomVisuals.Value)
            return;
        
        var scratchSection = playState.trackData.NoteData.ScratchSections.GetSectionForNote(noteIndex);
        
        if (!scratchSection.HasValue)
            return;
        
        ref var sustainNoteState = ref playState.scoreState.GetSustainState(noteIndex);

        if (sustainNoteState.isSustained && playState.currentTrackTick < playState.trackData.GetNote(scratchSection.Value.LastNoteIndex).tick)
            noteEventController.Hold((int) NoteIndex.HoldScratch);
    }

    [HarmonyPatch(typeof(TrackEditorGUI), nameof(TrackEditorGUI.UpdateEditor)), HarmonyPrefix]
    private static void TrackEditorGUI_UpdateEditor_Prefix(TrackEditorGUI __instance) {
        bool wasVisible = sequenceEditor.Visible;

        if (sequenceEditor.UpdateEditor())
            eventPlayback.Jump(__instance.frameInfo.currentTick);

        if (wasVisible || !sequenceEditor.Visible)
            return;
        
        __instance.UpdateFrameInfo();
        __instance.frameInfo.trackData.ClearNoteSelection();
    }
    
    [HarmonyPatch(typeof(TrackEditorGUI), nameof(TrackEditorGUI.SetCurrentTrackTime)), HarmonyPrefix]
    private static void TrackEditorGUI_SetCurrentTrackTime_Prefix(ref bool canChangeSelection) {
        if (sequenceEditor.Visible)
            canChangeSelection = false;
    }

    [HarmonyPatch(typeof(TrackEditorGUI), nameof(TrackEditorGUI.SaveChanges)), HarmonyPrefix]
    private static void TrackEditorGUI_SaveChanges_Prefix(TrackEditorGUI __instance) {
        if (!sequenceEditor.Dirty)
            return;

        visualsInfoAccessor.SaveCustomVisualsInfo(
            __instance.frameInfo.trackData.TrackInfoRef,
            sequenceEditor.GetCustomVisualsInfo());
        sequenceEditor.ClearDirty();
    }

    [HarmonyPatch(typeof(TrackEditorGUI), "HandleNoteEditorInput"), HarmonyPrefix]
    private static bool TrackEditorGUI_HandleNoteEditorInput_Prefix() => !sequenceEditor.Visible;
    
    [HarmonyPatch(typeof(TrackEditorGUI), "HandleMoveCursorInput"), HarmonyPrefix]
    private static bool TrackEditorGUI_HandleMoveCursorInput_Prefix() => !sequenceEditor.Visible;

    [HarmonyPatch(typeof(TrackEditorGUI), nameof(TrackEditorGUI.CheckCommand)), HarmonyPrefix]
    private static bool TrackEditorGUI_CheckCommand_Prefix(InputMapping.SpinCommands command, ref bool __result) {
        if (!sequenceEditor.Visible || command is not (
                InputMapping.SpinCommands.EditorRedo or
                InputMapping.SpinCommands.EditorUndo or
                InputMapping.SpinCommands.AddCuePoint or 
                InputMapping.SpinCommands.RemoveCuePoint))
            return true;
        
        __result = false;

        return false;
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