using System.Collections.Generic;
using UnityEngine;
using UZSG.Items;

namespace UZSG.UI
{
    public class PickupsIndicator : Window
    {
        Dictionary<string, PickupItemEntryUI> entryUIs = new();

        public GameObject pickupEntryPrefab;

        public void AddEntry(Item item)
        {
            if (entryUIs.TryGetValue(item.Id, out PickupItemEntryUI ui))
            {
                ui.IncrementCount(item);
            }
            else
            {
                ui = Instantiate(pickupEntryPrefab, transform).GetComponent<PickupItemEntryUI>();
                ui.transform.SetAsLastSibling();
                ui.SetDisplayedItem(item);
                ui.OnExpire += RemoveEntry;
                ui.Show();
                entryUIs[item.Id] = ui;
            }
        }

        public void RemoveEntry(PickupItemEntryUI entry)
        {
            entry.OnExpire -= RemoveEntry;

            if (entryUIs.ContainsValue(entry))
            {
                entryUIs.Remove(entry.Item.Id);
            }
        }
    }
}