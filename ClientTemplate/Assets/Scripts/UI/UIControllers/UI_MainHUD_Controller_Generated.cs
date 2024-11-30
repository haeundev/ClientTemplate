//This is Auto Generated Code, Don't modify this script.
using System;
using LiveLarson.Plugins.UIs;
using LiveLarson.UIManagement.UISystem;

namespace UI
{
    public partial class UI_MainHUD_Controller : UIController
    {
        public override System.Type UIWindowType => typeof(UI_MainHUD);
        public UI_MainHUD Window => _window as UI_MainHUD;
        public override void SetWindowOption(int id, Action<UIController> completeWindowSetting, WindowOption option)
        {
            base.SetWindowOption(id, completeWindowSetting, option);
           CreateWindow<UI_MainHUD>();
        }
    }
}
