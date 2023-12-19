using UnityEngine;

namespace URMG.Items
{
[CreateAssetMenu(fileName = "Item", menuName = "URMG/Item")]
public class ItemData : ScriptableObject
{
    public static readonly ItemData None;
    public string Id;
    public string Name;
    [TextArea] public string Description;
    public Sprite Sprite;
    public bool IsStackable;
    public int MaxStackSize;
}
}