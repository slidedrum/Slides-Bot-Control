using UnityEngine;

namespace BotControl.SmartSelect.PressActions.TapActions
{
    public class pActionSelectAll : IInputAction
    {
        public string FriendlyName => "Select All";
        public string _FriendlyNameShort => "Select-A";
        public string FriendlyIdentifier => "Select";
        public string FriendlyNameShort => $"<color=#{ColorHex}>{_FriendlyNameShort}</color>";
        private Color Color = new Color(1f, 1f, 1f, 0.25f);
        private string ColorHex => ColorUtility.ToHtmlStringRGB(Color);
        public Il2CppSystem.Type Type => null;
        public string pressTypeIdentifier => "Tap and Hold";
        public bool Invoke(Component BestComponent)
        {

            return true;
        }
        public bool IsActionValid(Component candidate)
        {
            return false;
        }
    }
}
