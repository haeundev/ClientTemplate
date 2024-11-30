//Generated file. Don't modify this script.
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using LiveLarson.Plugins.UIs;
using TMPro;

namespace UI
{
    public partial class UI_MainHUD : UIWindow
    {
        public enum UIComponents : int
        {
            BG,
            Icon,
            Title,
            MenuButton,
        }

        public Image BG => GetUI(0) as Image;
        public Image Icon => GetUI(1) as Image;
        public TextMeshProUGUI Title => GetUI(2) as TextMeshProUGUI;
        public Button MenuButton => GetUI(3) as Button;
    }

}

