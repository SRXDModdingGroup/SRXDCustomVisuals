using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SMU.Utilities;

namespace SRXDCustomVisuals.Plugin;

[BepInDependency("com.pink.spinrhythm.moddingutils", "1.0.7")]
[BepInPlugin("SRXD.CustomVisuals", "CustomVisuals", "1.0.0.0")]
public class Plugin : BaseUnityPlugin {
    public static Bindable<bool> EnableCustomVisuals { get; private set; }

    public new static ManualLogSource Logger { get; private set; }
    
    private void Awake() {
        Logger = base.Logger;

        var harmony = new Harmony("CustomVisuals");

        Assembly.LoadFrom(Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(Plugin)).Location), "SRXDCustomVisuals.Behaviors.dll"));

        harmony.PatchAll(typeof(Patches));
        EnableCustomVisuals = new Bindable<bool>(true);
    }
}