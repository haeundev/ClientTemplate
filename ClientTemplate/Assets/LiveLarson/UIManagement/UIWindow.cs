using UnityEngine;

namespace LiveLarson.UIManagement
{
    public abstract class UIWindow : MonoBehaviour
    {
        public virtual void Show()
        {
            gameObject.SetActive(true);
            OnShow();
        }

        public virtual void Hide()
        {
            OnHide();
            gameObject.SetActive(false);
        }

        protected virtual void OnShow()
        {
            // Custom show logic (e.g., animations)
        }

        protected virtual void OnHide()
        {
            // Custom hide logic (e.g., animations)
        }
    }
}