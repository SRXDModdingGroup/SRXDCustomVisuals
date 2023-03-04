using HarmonyLib;
using SRXDCustomVisuals.Core;
using UnityEngine;

namespace SRXDCustomVisuals.Plugin; 

public class Patches {
    private static VisualsInfoAccessor visualsInfoAccessor = new();
    private static VisualsBackgroundManager visualsBackgroundManager = new();
    private static TrackVisualsEventPlayback eventPlayback = new();
    private static SequenceEditor sequenceEditor;
    private static NoteEventController noteEventController = new(11);

    [HarmonyPatch(typeof(Track), "Awake"), HarmonyPostfix]
    private static void Track_Awake_Postfix(Track __instance) {
        sequenceEditor = new GameObject("Sequence Editor", typeof(SequenceEditor)).GetComponent<SequenceEditor>();
        VisualsBackgroundManager.CreateDirectories();
        eventPlayback.SetSequence(new TrackVisualsEventSequence());
    }

    [HarmonyPatch(typeof(Track), nameof(Track.PlayTrack)), HarmonyPostfix]
    private static void Track_PlayTrack_Postfix() {
        noteEventController.Reset();

        var playState = PlayState.Active;
        var customVisualsInfo = visualsInfoAccessor.GetCustomVisualsInfo(playState.TrackInfoRef);
        var sequence = new TrackVisualsEventSequence(customVisualsInfo);

        if (Plugin.EnableCustomVisuals.Value)
            visualsBackgroundManager.LoadBackground(customVisualsInfo.Background);

        VisualsEventManager.ResetAll();
        eventPlayback.SetSequence(sequence);
        sequenceEditor.Init(sequence, playState);
    }

    [HarmonyPatch(typeof(Track), nameof(Track.ReturnToPickTrack)), HarmonyPostfix]
    private static void Track_ReturnToPickTrack_Postfix() {
        noteEventController.Reset();
        VisualsEventManager.ResetAll();
        visualsBackgroundManager.UnloadBackground();
        eventPlayback.SetSequence(new TrackVisualsEventSequence());
        sequenceEditor.Exit();
        sequenceEditor.Visible = false;
    }

    [HarmonyPatch(typeof(Track), nameof(Track.Update)), HarmonyPostfix]
    private static void Track_Update_Postfix(Track __instance) {
        var playState = PlayState.Active;
        
        if (!Track.IsPaused && playState.currentTrackTick > playState.previousTrackTick)
            eventPlayback.Advance(playState.currentTrackTick);
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
    private static void TrackGameplayLogic_UpdateNoteStateInternal_Prefix(PlayState playState, int noteIndex, out NoteClearType __state)
        => __state = playState.noteStates[noteIndex].clearType;
    
    [HarmonyPatch(typeof(TrackGameplayLogic), nameof(TrackGameplayLogic.UpdateNoteStateInternal)), HarmonyPostfix]
    private static void TrackGameplayLogic_UpdateNoteStateInternal_Postfix(PlayState playState, int noteIndex, NoteClearType __state) {
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

        if (clearType == __state || clearType >= NoteClearType.ClearedEarly)
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

        sequenceEditor.UpdateEditor(out bool anyInput, out bool anyEdit);
        
        if (anyInput)
            eventPlayback.Jump(__instance.frameInfo.currentTick);
        
        if (anyEdit) {
            visualsInfoAccessor.SaveCustomVisualsInfo(
                __instance.frameInfo.playState.TrackInfoRef,
                sequenceEditor.GetCustomVisualsInfo());
        }

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
    
    [HarmonyPatch(typeof(TrackEditorGUI), nameof(TrackEditorGUI.SetCurrentTrackTime)), HarmonyPostfix]
    private static void TrackEditorGUI_SetCurrentTrackTime_Postfix() => eventPlayback.Jump(PlayState.Active.currentTrackTick);

    [HarmonyPatch(typeof(SpectrumProcessor), "UpdateComputeBuffer"), HarmonyPostfix]
    private static void SpectrumProcessor_UpdateComputeBuffer_Postfix(SpectrumProcessor __instance, SpectrumProcessor.Bands ___bandsLeft, ComputeBuffer ____computeBuffer) {
        if (___bandsLeft.smoothedBands.Length > 0)
            visualsBackgroundManager.UpdateSpectrumBuffer(____computeBuffer, __instance.EmptySpectrumBuffer);
        else
            visualsBackgroundManager.UpdateSpectrumBuffer(__instance.EmptySpectrumBuffer, __instance.EmptySpectrumBuffer);
    }

    [HarmonyPatch(typeof(PlayableTrackDataHandle), nameof(PlayableTrackDataHandle.ReplaceSetup)), HarmonyPrefix]
    private static void PlayableTrackDataHandle_ReplaceSetup_Prefix(PlayableTrackDataHandle __instance, ref PlayableTrackDataSetup newSetup) {
        if (!Plugin.EnableCustomVisuals.Value)
            return;
        
        var trackInfoRef = newSetup.TrackDataSegments[0].metadata.TrackInfoRef;
        
        if (trackInfoRef == null
            || !visualsBackgroundManager.TryGetBackground(visualsInfoAccessor.GetCustomVisualsInfo(trackInfoRef).Background, out var visualsBackground)
            || !visualsBackground.DisableBaseBackground)
            return;

        newSetup = new PlayableTrackDataSetup(newSetup);
        newSetup.BackgroundOverride = BackgroundSystem.DefaultBackground;
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
}