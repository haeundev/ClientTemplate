using System.Collections.Generic;
using UnityEngine;

namespace LiveLarson.UIManagement.UISystem
{
    [CreateAssetMenu(fileName = "UIContainer", menuName = "Scriptable Objects/UIContainer")]
    public class UIContainer : ScriptableObject
    {
        public List<UIKeyValue> uiList;
    }

}
