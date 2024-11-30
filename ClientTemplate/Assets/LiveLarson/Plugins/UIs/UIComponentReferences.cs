using System.Collections.Generic;
using UnityEngine;

namespace LiveLarson.Plugins.UIs
{
    public class UIComponentReferences : MonoBehaviour
    {
        [SerializeField] protected List<UIWindow.UIComponent> uiComponents;
        protected List<UIWindow.UIComponent> UIComponents => uiComponents;

        protected Component GetUI(int index)
        {
            if (UIComponents == null)
                return null;

            if (UIComponents.Count <= index)
                return null;

            return UIComponents[index].component;
        }
        
        public void SetUIComponents(List<UIWindow.UIComponent> components) { uiComponents = components; }
    }
}