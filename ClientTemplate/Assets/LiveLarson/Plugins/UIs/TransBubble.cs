using TMPro;
using UnityEngine;

namespace LiveLarson.Plugins.UIs
{
    public class TransBubble : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI m_text = null;
        private Animator m_animator = null;
        private static TransBubble instances;

        private static readonly int OpenTrigger = Animator.StringToHash("Open");
        private static readonly int CloseTrigger = Animator.StringToHash("Close");
        private static readonly int Exit = Animator.StringToHash("Exit");
        
        public delegate void OnClickTranslate(bool isNative);
        public static event OnClickTranslate OnTranslateAction;

        private void Awake()
        {
            m_animator = gameObject.GetComponent<Animator>();
        }

        private void OnEnable()
        {
            m_animator.SetTrigger(Exit);
            instances = this;
        }

        private void OnDisable()
        {
            m_animator.SetTrigger(Exit);
            instances = null;
        }

        public static void SetNativeText(string nativeText)
        {
            instances.SetText(nativeText);
        }
        
        private void SetText(string nativeText)
        {
            if (m_text != null) m_text.text = nativeText;
        }

        public static void Translate(bool toNative)
        {
            if (instances == null) return;
            OnTranslateAction?.Invoke(toNative);
            if (toNative)
            {
                instances.Open();
            }
            else
            {
                instances.Close();
            }
        }

        private void Open()
        {
            m_animator.SetBool(CloseTrigger,false);
            m_animator.SetTrigger(OpenTrigger);
        }

        private void Close()
        {
            m_animator.SetBool(OpenTrigger,false);
            m_animator.SetTrigger(CloseTrigger);
        }




    }

}
