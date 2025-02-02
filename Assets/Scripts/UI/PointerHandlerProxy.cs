using System;

using UnityEngine;
using UnityEngine.EventSystems;

namespace UZSG.UI
{
    /// <summary>
    /// Serves as a midman for sending event IPointerHandler calls to other scripts.
    /// </summary>
    public class PointerHandlerProxy : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public event EventHandler<PointerEventData> OnPointerEntered;
        public event EventHandler<PointerEventData> OnPointerExited;
        public event EventHandler<PointerEventData> OnPointerDowned;
        public event EventHandler<PointerEventData> OnPointerUpped;

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnPointerEntered?.Invoke(this, eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnPointerExited?.Invoke(this, eventData);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnPointerDowned?.Invoke(this, eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnPointerUpped?.Invoke(this, eventData);
        }
    }
}