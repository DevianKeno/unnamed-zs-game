using System;
using System.Collections.Generic;

using UnityEngine;

using MEC;

using UZSG.Data;

namespace UZSG.Crafting 
{
    [Serializable]
    public class CraftingRoutine
    {
        protected RecipeData recipeData;
        public RecipeData RecipeData => recipeData;

        protected CraftItemOptions options;
        public CraftItemOptions Options => options;

        public CraftingRoutineStatus Status { get; protected set; }
        public float StartTime { get; protected set;}
        public float EndTime { get; protected set;}
        /// <summary>
        /// Seconds elapsed for the entire Crafting Routine.
        /// </summary>
        public float SecondsElapsed { get; protected set; }
        /// <summary>
        /// Seconds left for the entire Crafting Routine.
        /// </summary>
        public float SecondsLeft => (int) RecipeData.CraftingTimeSeconds * TotalYield - SecondsElapsed;
        /// <summary>
        /// Progress of the entire Crafting Routine. [0-1]
        /// </summary>
        public float Progress => (float) SecondsElapsed / (recipeData.CraftingTimeSeconds * TotalYield);
        /// <summary>
        /// Seconds elapsed for one single craft in this Crafting Routine.
        /// </summary>
        public float SecondsElapsedSingle { get; protected set; }
        /// <summary>
        /// Seconds left for one single craft in this Crafting Routine.
        /// </summary>
        public float SecondsLeftSingle => (int) RecipeData.CraftingTimeSeconds - SecondsElapsedSingle;
        /// <summary>
        /// Progress of a single crafted Item. [0-1]
        /// </summary>
        public float ProgressSingle => (float) SecondsElapsedSingle / recipeData.CraftingTimeSeconds;
        /// <summary>
        /// Count of currently crafted items.
        /// </summary>
        public int CurrentYield { get; protected set; } 
        /// <summary>
        /// Count of remaining items to craft.
        /// </summary>
        public int RemainingYield { get; protected set; }
        /// <summary>
        /// Total amount of items this crafting routine will produce.
        /// </summary>
        public int TotalYield { get; protected set; }
        /// <summary>
        /// Whether this crafting routine consumes fuel.
        /// </summary>
        public bool IsFueled { get; protected set; }
        public bool IsCompleted { get; protected set; } = false;

        bool _hasStartedOnce = false;
        CoroutineHandle _routineHandle;

        #region Events
        
        public event Action<CraftingRoutine> OnNotify;
        public event Action<CraftingRoutine> OnUpdate;

        #endregion

        public CraftingRoutine(CraftItemOptions options)
        {
            this.options = options;
            recipeData = options.Recipe;
            IsFueled = options.Recipe.RequiresFuel;
            TotalYield = options.Count;
        }


        #region Public

        public void Prepare()
        {
            SecondsElapsed = 0;
            SecondsElapsedSingle = 0;
            CurrentYield = 0;
            RemainingYield = TotalYield;
            IsCompleted = false;

            Status = CraftingRoutineStatus.Prepared;
            OnNotify?.Invoke(this);
        }

        public void Start()
        {
            /// can only start the first time, or when paused
            if (_hasStartedOnce &&
                Status != CraftingRoutineStatus.Paused)
            {
                return;
            }

            _hasStartedOnce = true;
            StartTime = Time.time;
            Status = CraftingRoutineStatus.Started;
            OnNotify?.Invoke(this);
            _routineHandle = Timing.RunCoroutine(_StartCraftRoutine());
        }

        public void Pause()
        {
            if (!_hasStartedOnce || /// can't pause what's not started
                Status == CraftingRoutineStatus.Paused)
            {
                return; 
            }

            /// NOTE: kill first before setting to paused
            Status = CraftingRoutineStatus.Paused;
            Timing.KillCoroutines(_routineHandle);
            OnNotify?.Invoke(this);
            // Timing.PauseCoroutines(_routineHandle); /// NOTE: PauseCoroutines() seems nice but does not seem to work
        }

        public void Finish()
        {
            if (Status == CraftingRoutineStatus.Finished) return;
            
            Timing.KillCoroutines(_routineHandle); /// what's finished's finished
            Status = CraftingRoutineStatus.Finished;
            EndTime = Time.time;
            OnNotify?.Invoke(this);
        }
        
        public void Cancel()
        {
            if (Status == CraftingRoutineStatus.Canceled) return;

            Timing.KillCoroutines(_routineHandle);
            Status = CraftingRoutineStatus.Canceled;
            OnNotify?.Invoke(this);
        }
        
        #endregion


        IEnumerator<float> _StartCraftRoutine()
        {
            Status = CraftingRoutineStatus.Ongoing;
            
            for (int i = RemainingYield; i > 0; i--)
            {
                while (SecondsElapsedSingle < RecipeData.CraftingTimeSeconds)
                {
                    SecondsElapsedSingle += Time.deltaTime;
                    SecondsElapsed += Time.deltaTime;
                    OnUpdate?.Invoke(this);
                    yield return Timing.WaitForOneFrame;
                }

                SecondsElapsedSingle -= RecipeData.CraftingTimeSeconds;
                CurrentYield++;
                RemainingYield = TotalYield - CurrentYield;
                
                Status = CraftingRoutineStatus.CraftedSingle;
                OnNotify?.Invoke(this);
            }

            Status = CraftingRoutineStatus.Completed;
            OnNotify?.Invoke(this);
            IsCompleted = true;
        }
    }
}
