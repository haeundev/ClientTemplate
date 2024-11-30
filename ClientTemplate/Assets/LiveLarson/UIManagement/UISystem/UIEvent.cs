using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LiveLarson.UIManagement.UISystem
{
    public class UIEvent : MonoBehaviour , IPointerDownHandler, IPointerUpHandler , IPointerClickHandler
    {
        public Action<PointerEventData> PointerDownAction;
        public Action<PointerEventData> PointerUpAction;
        public Action<PointerEventData> PointerClickAction;
        
        public void OnPointerDown(PointerEventData eventData)
        {
            PointerDownAction?.Invoke(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            PointerUpAction?.Invoke(eventData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            PointerClickAction?.Invoke(eventData);
        }
    }

}
