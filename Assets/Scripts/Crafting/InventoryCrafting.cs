using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.ResourceManagement.Util;
using UnityEngine.UI;
using UZSG.Data;
using UZSG.Entities;
using UZSG.Inventory;
using UZSG.Items;

namespace UZSG.Crafting
{
    //test
    public struct CraftFinishedInfo 
    {
            public DateTime StartTime;
            public DateTime EndTime;
    }
 
    public class InventoryCrafting : Crafter
    {
        
    }
}