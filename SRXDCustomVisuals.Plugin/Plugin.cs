using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SMU.Utilities;
using SpinCore;
using SpinCore.UI;
using SRXDCustomVisuals.Core;
using UnityEngine;

namespace SRXDCustomVisuals.Plugin;

[BepInDependency("com.pink.spinrhythm.moddingutils", "1.0.7")]
[BepInDependency("com.pink.spinrhythm.spincore", "1.0.0")]
[BepInPlugin(PluginInfo.Guid, PluginInfo.Name, PluginInfo.Version)]
public class Plugin : SpinPlugin {
    public static Bindable<bool> EnableCustomVisuals { get; private set; }

    public new static ManualLogSource Logger { get; private set; }

    protected override void Awake() {
        base.Awake();

        Logger = base.Logger;

        var harmony = new Harmony("CustomVisuals");

        harmony.PatchAll(typeof(Patches));
        EnableCustomVisuals = Config.CreateBindable("EnableCustomVisuals", true);
    }

    protected override void Init() {
        var root = MenuManager.CreateOptionsTab("Custom Visuals").UIRoot;

        SpinUI.CreateToggle("Enable Custom Visuals", root).Bind(EnableCustomVisuals);
    }
}