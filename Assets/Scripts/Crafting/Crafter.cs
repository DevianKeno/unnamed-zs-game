using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using UZSG.Inventory;
using UZSG.Items;
using UZSG.Data;
using UZSG.Objects;

namespace UZSG.Crafting
{
    /// <summary>
    /// Base abstract class for all crafting logic.
    /// </summary>
    public class Crafter : MonoBehaviour
    {
        [SerializeField] protected bool simultaneousCrafting = false;
        [SerializeField] protected int maxSimultaneousCrafts = 1;
        [SerializeField] protected bool autoCraftNext = true;
        [SerializeField] protected int availableCraftSlots;
        [SerializeField] protected List<CraftingRoutine> routines = new();
        /// <summary>
        /// The list of CraftingRoutines this crafter is currently processing.
        /// </summary>
        public List<CraftingRoutine> Routines => routines;
        /// <summary>
        /// Whether if the crafter has available slots for crafting.
        /// </summary>
        public bool IsQueueFull
        {
            get => availableCraftSlots == 0;
        }

        #region Events

        /// <summary>
        /// Raise once everytime a crafting routine notifies (raises its own event).
        /// </summary>
        public event Action<CraftingRoutine> OnRoutineNotify;
        /// <summary>
        /// Raise every frame while a a crafting routine crafts an Item.
        /// </summary>
        public event Action<CraftingRoutine> OnRoutineUpdate;

        #endregion


        void Start()
        {
            availableCraftSlots = maxSimultaneousCrafts;
        }

        /// <summary>
        /// Crafts a new item using the provided options.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="begin"></param>
        /// <returns></returns>
        public virtual CraftingRoutine CraftNewItem(ref CraftItemOptions options, bool begin = true)
        {
            var routine = new CraftingRoutine(options);
            
            routine.OnNotify += OnRoutineEventCall;
            routine.OnUpdate += OnRoutineEventUpdate;
            routine.Prepare();
            routines.Add(routine);
            if (begin && availableCraftSlots > 0)
            {
                CraftNextAvailable();
            }

            return routine;
        }

        public void CraftNextAvailable()
        {
            if (!routines.Any()) return;

            var nextRoutine = GetNextRoutine();
            if (nextRoutine != null)
            {
                nextRoutine.Start();
                availableCraftSlots--;
            }
        }

        public void ContinueRemainingCraft()
        {
            if (!routines.Any()) return;

            var pausedRoutine = GetPausedRoutine();
            if (pausedRoutine != null)
            {
                pausedRoutine.Start();
                // StartCoroutine(pausedRoutine._StartCraftRoutine());
            }
        }

        CraftingRoutine GetNextRoutine()
        {
            foreach (var r in routines)
            {
                if (r.Status == CraftingRoutineStatus.Prepared)
                {
                    return r;
                }
            }
            return null;
        }

        CraftingRoutine GetPausedRoutine()
        {
            foreach (var r in routines)
            {
                if (r.Status == CraftingRoutineStatus.Paused)
                {
                    return r;
                }
            }
            return null;
        }

        public void PauseAll()
        {
            foreach (var r in routines)
            {
                r.Pause();
            }
        }
        
        public void CancelRoutine(int index)
        {
            if (routines.IsValidIndex(index))
            {
                routines[index].Cancel();
            }
        }


        #region Crafting routine event callbacks

        protected virtual void OnRoutineEventCall(CraftingRoutine routine)
        {            
            if (routine.Status == CraftingRoutineStatus.Finished)
            {
                routines.Remove(routine);
                routine.OnNotify -= OnRoutineEventCall;
                routine.OnUpdate -= OnRoutineEventUpdate;
                availableCraftSlots++;

                if (autoCraftNext) CraftNextAvailable();
            }

            OnRoutineNotify?.Invoke(routine);
        }

        protected virtual void OnRoutineEventUpdate(CraftingRoutine routine)
        {
            OnRoutineUpdate?.Invoke(routine);
        }

        #endregion
    }
}