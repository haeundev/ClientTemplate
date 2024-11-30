using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LiveLarson.Plugins.UIs
{
    public partial class TransText : UIBehaviour
    {
        [SerializeField] private List<DOTweenAnimation> animationEng = default;
        [SerializeField] private List<DOTweenAnimation> animationNative = default;
        [SerializeField] private TMP_Text textEng = default;
        [SerializeField] private TMP_Text textNative = default;
        public string TextEng => textEng.text;
        public string TextNative => textNative.text;

        public TMP_Text TextEngGUI => textEng;
        public TMP_Text TextNativeGUI => textNative;

        private static List<TransText> instances = new List<TransText>();
        private static bool translateState = false;
        private bool localTranslateState = false;
        private bool translatable = false;

        public delegate void OnClickTranslate(bool isNative);
        
        public static event OnClickTranslate OnTranslateAction;
        private bool CheckedTranslateState 
        { 
            get  
            {
                if (localTranslateState == false && string.IsNullOrEmpty(textEng.text) == false)
                    return false;
                if (string.IsNullOrEmpty(textNative.text) == false)
                    return true;
                return false;
            }
        }

        protected override void Awake()
        {
            if (textEng == null)
                Collect();
            translatable = !(textEng == null || textNative == null);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            instances.Add(this);
            localTranslateState = translateState;
            if (translatable)
            {
                SelectActiveText();
            }
        }

        private void SelectActiveText()
        {
            if (!translatable)
                return;

            if (CheckedTranslateState)
            {
                textEng.gameObject.SetActive(false);
                textNative.gameObject.SetActive(true);
            }
            else
            {
                textEng.gameObject.SetActive(true);
                textNative.gameObject.SetActive(false);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            instances.Remove(this);
        }

        public void Collect()
        {
            var texts = GetComponentsInChildren<TMP_Text>(true).ToList();
            if (texts.Count < 2)
            {
                if (texts.Count == 1)
                    SetEng(texts[0]);
                return;
            }

            textNative = FindText(texts, "Native", "native");
            if (textNative != null)
            {
                animationNative = textNative.GetComponents<DOTweenAnimation>()?.ToList();
                texts.Remove(textNative);
            }

            if (texts.Count == 1)
                SetEng(texts[0]);
            else
                SetEng(FindText(texts, "Eng", "eng"));
        }

        private TMP_Text FindText(List<TMP_Text> texts, params string[] str)
        {
            TMP_Text result = null;
            foreach (var item in str)
            {
                result = texts.FirstOrDefault(p => p.name.Contains(item));
                if (result != null)
                    break;
            }
            return result;
        }

        private void SetEng(TMP_Text text)
        {
            textEng = text;
            if (textEng != null)
            {
                animationEng = textEng.GetComponents<DOTweenAnimation>()?.ToList();
            }
        }

        public void SetText(string eng, string native)
        {
            if (textEng != null)
                textEng.text = eng;
            if (textNative != null)
                textNative.text = native;
            SelectActiveText();
        }

        public void ChangeFont(TMP_FontAsset fontAsset)
        {
            if (textEng != null)
                textEng.font = fontAsset;
            if (textNative != null)
                textNative.font = fontAsset;
        }

        public void RemoveAnimation(DOTweenAnimation animation)
        {
            if (animationNative != null)
                animationNative.Remove(animation);
            if (animationEng != null)
                animationEng.Remove(animation);
        }

        public void OnTranslate()
        {
            if (!translatable)
                return;
            SelectActiveText();
            Play();
        }

        public static void Translate(bool toNative)
        {
            OnTranslateAction?.Invoke(toNative);
            translateState = toNative;
            instances.ForEach(action: p =>
            {
                p.localTranslateState = translateState;
                p.OnTranslate();
            });
        }

        public static void ToggleTranslate()
        {
            Translate(!translateState);
        }

        public void ToggleLocalTranslate()
        {
            localTranslateState = !localTranslateState;
            OnTranslate();
        }

        public void Play()
        {
            if (CheckedTranslateState)
                OnPlayNative();
            else
                OnPlayEng();

        }
        private void OnPlayEng()
        {
            ShowEng();
            if (animationEng != null)
                animationEng.ForEach(p => p.DOPlay());
        }

        private void OnPlayNative()
        {
            ShowNative();
            if (animationNative != null)
                animationNative.ForEach(p => p.DOPlay());
        }

        public void SetColor(Color color)
        {
            if (textEng != null)
                textEng.color = color;
            if (textNative != null)
                textNative.color = color;
        }

        public Color GetColor()
        {
            return textEng.color;
        }

        public void ShowEng()
        {
            textEng.gameObject.SetActive(true);
            textNative.gameObject.SetActive(false);
        }

        public void ShowNative()
        {
            textEng.gameObject.SetActive(false);
            textNative.gameObject.SetActive(true);
        }
        
    }
}