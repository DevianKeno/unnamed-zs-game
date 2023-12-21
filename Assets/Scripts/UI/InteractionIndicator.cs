using UnityEngine;
using TMPro;
using URMG.Items;
using URMG.Interactions;
using System;

namespace URMG.UI
{
public class InteractionIndicator : MonoBehaviour
{
    public struct Data
    {
        public string Action;
        public string Object;
    }

    [SerializeField] GameObject indicator;
    [SerializeField] TextMeshProUGUI actionTMP;
    [SerializeField] TextMeshProUGUI objectTMP;

    public void SetActionText(string msg)
    {
        actionTMP.text = msg;
    }

    public void SetObjectText(string msg)
    {
        objectTMP.text = msg;
    }

    public void Show()
    {
        indicator.SetActive(true);
    }
    
    public void Show(Data data)
    {
        SetActionText(data.Action);
        SetObjectText(data.Object);
        indicator.SetActive(true);
    }

    public void Hide()
    {
        indicator.SetActive(false);
    }

    public void Set(IInteractable obj)
    {
        if (obj == null) return;
        SetActionText(obj.Action);
    }

    public void SetItem(Item item)
    {
        if (item == null) 
        {
            Debug.Log("Failed to set PickupIndicator item as Item is null.");
            return;
        }
        objectTMP.text = item.Name;
    }
}
}
