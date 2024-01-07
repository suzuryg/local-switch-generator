using UnityEngine;

namespace Suzuryg.LocalSwitchGenerator
{
    // [CreateAssetMenu(menuName = "LocalSwitchGenerator/LocalizationTable")]
    internal class LocalizationTable : ScriptableObject
    {
        [Header("MainWindow_UI")]
        public string TargetObject = "Target Object";
        public string InitialState = "Initial State";
        public string ToggleButtons = "Toggle ON and OFF Buttons";
        public string Button = "Button";
        public string OffToOn = "Button to Turn ON";
        public string OnToOff = "Button to Turn OFF";
        public string Generate = "Generate";
        public string Cancel = "Cancel";

        [Header("MainWindow_Help")]
        public string ComponentsWillBeAdded = "Item component and InteractItemTrigger component will be added to the button.";

        [Header("MainWindow_Message")]
        public string PleaseSelectSceneObject = "Please select an object on the scene (you cannot select an asset).";
        public string NoTarget = "Please select a target object.";
        public string NoButtons = "Please select buttons.";
        public string SameObjects = "Please select all different objects for the target object and the buttons.";
        public string ConfirmGeneration = "Generate switch?";
        public string GenerationCompleted = "Switch generation completed!";
        public string GenerationError = "The following error occurred during switch generation. Please check the console for details.";
        public string ComponentsAddedSingle = "Components have been added to \"<0>\".";
        public string ComponentsAddedDouble = "Components have been added to \"<0>\" and \"<1>\".";
        public string ObjectGenerated = "\"<0>\" object has been generated.";
    }
}
