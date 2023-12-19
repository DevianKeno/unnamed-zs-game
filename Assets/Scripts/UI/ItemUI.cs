using UnityEngine;
using UnityEngine.UI;
using URMG.Items;

namespace URMG.UI
{
public class ItemUI : MonoBehaviour
{
    Sprite sprite;

    void Awake()
    {
        sprite = GetComponent<Sprite>();
    }

    public void SetItem(Item item)
    {
        sprite = item.Data.Sprite;
    }
}
}