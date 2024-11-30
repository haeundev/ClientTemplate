using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LiveLarson.Plugins.UIs
{
    public class UIChildSelector : UIBehaviour
    {
        [Serializable]
        public class Child
        {
            public GameObject gameObject = default;
            public List<DOTweenAnimation> animations = default;
        }

        [SerializeField] private List<Child> childs = default;
        [SerializeField] private int defaultIndex = 0;

        [Space, Header("Data for run time ")]
        [SerializeField] private GameObject selected = default;
        [SerializeField] private bool keepSelected = false;

        protected override void Awake()
        {
            base.Awake();
            if (childs == null || childs.Count == 0)
                Collect();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (keepSelected == false || selected == null)
            {
                if (childs.Count > defaultIndex)
                    Select(childs[0].gameObject);
            }
        }

        public void Collect()
        {
            childs = new List<Child>();
            var count = transform.childCount;
            for (int i = 0; i < count; i++)
            {
                var child = CreateChild(transform.GetChild(i).gameObject);
                childs.Add(child);
            }
        }

        public Child CreateChild(GameObject obj)
        {
            if (obj == null)
                return null;

            var child = new Child();
            child.gameObject = obj;
            child.animations = obj.GetComponents<DOTweenAnimation>()?.ToList();
            return child;
        }

        public void Select(GameObject obj)
        {
            childs.ForEach(p =>
            {
                bool isSelected = p.gameObject == obj;
                p.gameObject.SetActive(isSelected);
                if (isSelected && p.animations != null)
                    p.animations.ForEach(q => q.DOPlay());
            });
            selected = obj;
        }

        public void SelectAnimationOnly(GameObject obj)
        {
            childs.ForEach(p =>
            {
                bool isSelected = p.gameObject == obj;
                if (isSelected && p.animations != null)
                    p.animations.ForEach(q => q.DOPlay());
            });
            selected = obj;
        }

        public void Select(int index)
        {
            selected = null;
            for (int i = 0; i < childs.Count; i++)
            {
                var child = childs[i];
                bool isSelected = i == index;
                child.gameObject.SetActive(isSelected);
                if (isSelected && child.animations != null)
                    child.animations.ForEach(q => q.DOPlay());
                if (isSelected)
                    selected = child.gameObject;
            }
        }
    }
}