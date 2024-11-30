using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LiveLarson.Util
{
    public class InfiniteScroll : MonoBehaviour
    {
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform viewport;
        [SerializeField] private RectTransform content;
        [SerializeField] private HorizontalLayoutGroup layoutGroup;
        private bool isUpdated;

        private List<RectTransform> items;

        private Vector2 oldVelocity;

        private void Start()
        {
            isUpdated = false;
            oldVelocity = Vector2.zero;

            var itemsToAdd = Mathf.CeilToInt(viewport.rect.width / (items[0].rect.width + layoutGroup.spacing));

            for (var i = 0; i < itemsToAdd; i++)
            {
                var item = Instantiate(items[i % items.Count], content);
                item.SetAsLastSibling();
            }

            for (var i = 0; i < itemsToAdd; i++)
            {
                var num = items.Count - i - 1;
                while (num < 0) num += items.Count;
                var item = Instantiate(items[num], content);
                item.SetAsFirstSibling();
            }

            var localPosition = content.localPosition;
            localPosition = new Vector3((0 - (items[0].rect.width + layoutGroup.spacing)) * itemsToAdd,
                localPosition.y, localPosition.z);
            content.localPosition = localPosition;
        }

        private void Update()
        {
            if (isUpdated)
            {
                scrollRect.velocity = oldVelocity;
                isUpdated = false;
            }

            if (content.localPosition.x > 0)
            {
                Canvas.ForceUpdateCanvases();
                oldVelocity = scrollRect.velocity;
                content.localPosition -= new Vector3(items.Count * (items[0].rect.width + layoutGroup.spacing), 0, 0);
                isUpdated = true;
            }

            if (content.localPosition.x < -items.Count * (items[0].rect.width + layoutGroup.spacing))
            {
                Canvas.ForceUpdateCanvases();
                oldVelocity = scrollRect.velocity;
                content.localPosition += new Vector3(items.Count * (items[0].rect.width + layoutGroup.spacing), 0, 0);
                isUpdated = true;
            }
        }

        private void OnEnable()
        {
            items = new List<RectTransform>();
            foreach (RectTransform child in content)
                items.Add(child);
        }
    }
}