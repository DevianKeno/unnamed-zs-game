using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UZSG
{
    /// <summary>
    /// These should be read from a file.
    /// </summary>
    public class Defaults
    {
        public static float BaseMaxHealth = 100f;
        public static float BaseHealthRegen = 0.1f;
        public static float BaseMaxStamina = 100f;
        public static float BaseStaminaRegen = 5f;
        public static float BaseMaxMana = 100f;
        public static float BaseManaRegen = 1f;
        public static float BaseMaxHunger = 100f;
        public static float BaseMaxThirst = 100f;
        
        public static Vector2 InventoryHandsDims = new(4, 3);
    }
}