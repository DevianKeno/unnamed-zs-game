namespace UZSG.Crafting 
{
    public enum CraftingRoutineStatus {
        /// <summary>
        /// Routine is not yet started but is ready to go.
        /// Raised only at OnNotify event.
        /// </summary>
        Prepared,
        /// <summary>
        /// Routine has started crafting, time is ticking!
        /// Raised only at OnNotify event.
        /// </summary>
        Started,
        /// <summary>
        /// Routine is currently crafting.
        /// Raised only at OnTick event.
        /// </summary>
        Ongoing,
        /// <summary>
        /// Routine has finished crafting one item.
        /// Raised only at OnNotify event.
        /// </summary>
        CraftedSingle,
        /// <summary>
        /// Routine has completed crafting all requested items.
        /// Raised only at OnNotify event.
        /// </summary>
        Completed,
        /// <summary>
        /// Routine is finished.
        /// Raised only at OnNotify event.
        /// </summary>
        Finished,
        /// <summary>
        /// Routine was canceled.
        /// Raised only at OnNotify event.
        /// </summary>
        Canceled,
        /// <summary>
        /// Routine was paused.
        /// Raised only at OnNotify event.
        /// </summary>
        Paused
    }
}