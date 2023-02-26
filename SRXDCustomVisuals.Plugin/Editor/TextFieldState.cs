namespace SRXDCustomVisuals.Plugin; 

public class TextFieldState {
    public string DisplayValue {
        get => displayValue;
        set {
            if (value == displayValue)
                return;
            
            displayValue = value;
            displayValueChanged = true;
        }
    }

    public string ActualValue { get; set; } = string.Empty;

    private bool displayValueChanged;
    private string displayValue = string.Empty;

    public void Init(string value) {
        displayValue = value;
        ActualValue = value;
    }

    public void RevertDisplayValue() => displayValue = ActualValue;

    public bool CheckValueChanged() {
        bool wasChanged = displayValueChanged;

        displayValueChanged = false;

        return wasChanged;
    }
}