using System;
using LiveLarson.UIManagement.UISystem;

namespace UI
{
    public partial class UI_MainHUD_Controller : UIController
    {
        protected override void Awake()
        {
            Show();
            CompleteWindowSetting?.Invoke(this);
        }
    }
}
