using System;
using System.Collections.Generic;

using UnityEngine;
using TMPro;

using UZSG.Objects;
using UZSG.Crafting;

namespace UZSG.UI
{
    public class WorkstationGUI : Window
    {
        protected Crafter crafter;
        public Crafter Crafter => crafter;
    }
}