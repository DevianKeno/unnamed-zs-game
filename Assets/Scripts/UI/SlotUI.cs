using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using URMG.Items;
using System;

namespace URMG.UI
{
public class SlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public static Color Opaque = new(1f, 1f, 1f, 1f);
    public static Color Transparent = new(1f, 1f, 1f, 0f);
    public static Color Normal = new(0f, 0f, 0f, 0.5f);
    public static Color Hovered = new(0.2f, 0.2f, 0.2f, 0.5f);

    Item _item;    
    [SerializeField] Image slotImage;
    [SerializeField] Image itemImage;
    [SerializeField] TextMeshProUGUI countText;
    [SerializeField] TextMeshProUGUI altText;
    
    public void OnPointerDown(PointerEventData eventData)
    {
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        slotImage.color = Hovered;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        slotImage.color = Normal;
    }

    public void Refresh()
    {
        if (_item.Count == 1)
        {
            countText.text = "";
        } else
        {
            countText.text = _item.Count.ToString();
        }
    }

    public void SetDisplayItem(Item item)
    {
        _item = item;

        if (item.Data.Sprite != null)
        {
            altText.enabled = false;
            itemImage.sprite = item.Data.Sprite;
            itemImage.color = Opaque;
        } else
        {
            altText.enabled = true;
            altText.text = item.Data.Name;
        }

        if (item.Count == 1)
        {
            countText.text = "";
        } else
        {
            countText.text = item.Count.ToString();
        }
    }

    void Clear()
    {        
        itemImage = null;
        itemImage.color = Transparent;
    }
}
}