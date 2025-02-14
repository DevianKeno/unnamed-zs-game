using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UZSG.Saves;

namespace UZSG.Crafting
{
    /// <summary>
    /// Base class for crafting logic.
    /// </summary>
    public class Crafter : MonoBehaviour
    {
        /// <summary>
        /// The maximum number of crafting routines that can be queued.
        /// </summary>
        public int QueueSize;
        /// <summary>
        /// The maximum number of crafting routines that can be queued.
        /// </summary>
        public int AvailableQueueSlots
        {
            get => QueueSize - routines.Count;
        }
        /// <summary>
        /// Whether to immediately start the next crafting routine in queue after the current is finished.
        /// </summary>
        public bool AutoCraftNext = true;
        /// <summary>
        /// Whether to allow multiple crafting routines to craft at one time.
        /// </summary>
        public bool SimultaneousCrafting = false;
        [SerializeField] protected int maxSimultaneousCrafts = 1;
        /// <summary>
        /// The number of crafting routines that can be simultaneously crafted at one time.
        /// Only taken into account if <c>SimultaneousCrafting</c> is enabled.
        /// </summary>
        public int MaxSimultaneousCrafts;
        [SerializeField] protected List<CraftingRoutine> routines = new();
        /// <summary>
        /// The list of CraftingRoutines this crafter is currently processing.
        /// </summary>
        public List<CraftingRoutine> Routines => routines;
        /// <summary>
        /// Whether if the crafter has available slots for crafting.
        /// </summary>
        public bool IsQueueFull => routines.Count >= QueueSize;
        
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
            
            if (begin &&
                SimultaneousCrafting &&
                AvailableQueueSlots > 0)
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

        public void ReadSaveData(List<CraftingRoutineSaveData> routineSaves)
        {
            foreach (var save in routineSaves)
            {
                if (false == Game.Recipes.TryGetData(save.RecipeId, out var recipeData)) continue;

                var routine = new CraftingRoutine(new()
                {
                    Recipe = recipeData,
                    Count = save.Count,
                });
                routine.ReadSaveData(save);

                routine.OnNotify += OnRoutineEventCall;
                routine.OnUpdate += OnRoutineEventUpdate;
                routine.Prepare();
            }
        }

        public List<CraftingRoutineSaveData> WriteSaveData()
        {
            var list = new List<CraftingRoutineSaveData>();
            foreach (var r in routines)
            {
                r.Pause();
                list.Add(r.WriteSaveData());

            }
            return list;
        }


        #region Crafting routine event callbacks

        protected virtual void OnRoutineEventCall(CraftingRoutine routine)
        {            
            if (routine.Status == CraftingRoutineStatus.Finished)
            {
                routines.Remove(routine);
                routine.OnNotify -= OnRoutineEventCall;
                routine.OnUpdate -= OnRoutineEventUpdate;

                if (AutoCraftNext) CraftNextAvailable();
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