using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SMU.Utilities;
using SpinCore;
using SpinCore.UI;

namespace SRXDCustomVisuals.Plugin;

[BepInDependency("com.pink.spinrhythm.moddingutils", "1.0.6")]
[BepInDependency("com.pink.spinrhythm.spincore")]
[BepInPlugin("SRXD.CustomVisuals", "CustomVisuals", "1.0.0.0")]
public class Plugin : SpinPlugin {
    public static Bindable<bool> EnableCustomVisuals { get; private set; }

    public new static ManualLogSource Logger { get; private set; }
    
    protected override void Awake() {
        base.Awake();

        Logger = base.Logger;

        var harmony = new Harmony("Storyboard");
        
        harmony.PatchAll(typeof(Patches));
        EnableCustomVisuals = AddBindableConfig("EnableCustomVisuals", true);
    }

    protected override void CreateMenus() {
        var root = CreateOptionsTab("Storyboard").UIRoot;

        SpinUI.CreateToggle("Enable Custom Visuals", root).Bind(EnableCustomVisuals);
    }
}