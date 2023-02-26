namespace SRXDCustomVisuals.Plugin; 

public class VisualsElement {
    public string BundleName { get; }
    
    public string AssetName { get; }
    
    public int Root { get; }

    public VisualsElement(string bundleName, string assetName, int root) {
        AssetName = assetName;
        BundleName = bundleName;
        Root = root;
    }
}