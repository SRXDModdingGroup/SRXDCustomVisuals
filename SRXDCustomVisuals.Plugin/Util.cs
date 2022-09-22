using System;
using System.IO;
using System.Reflection;

namespace SRXDCustomVisuals.Plugin; 

internal static class Util {
    private static readonly string ASSEMBLY_PATH = Path.GetDirectoryName(Assembly.GetAssembly(typeof(Plugin)).Location);

    public static bool TryLoadAssembly(string name) {
        string fileName = Path.ChangeExtension(name, ".dll");
        string path = Path.Combine(ASSEMBLY_PATH, fileName);
        
        if (!File.Exists(path)) {
            Plugin.Logger.LogWarning($"{fileName} was not found. Some visual elements may be missing controllers as a result.");

            return false;
        }

        try {
            Assembly.LoadFrom(path);

            return true;
        }
        catch {
            Plugin.Logger.LogWarning($"Failed to load {fileName}. Some visual elements may be missing controllers as a result.");
            
            return false;
        }
    }
}