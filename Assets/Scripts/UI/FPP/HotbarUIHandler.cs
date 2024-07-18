using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UZSG.UI;

public class HotbarUIHandler : MonoBehaviour
{
    public Dictionary<int, ItemSlotUI> Slots = new();

    void Start()
    {
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent<ItemSlotUI>(out var slot))
            {
                Slots[slot.Index] = slot;
            }
        }
    }
}
