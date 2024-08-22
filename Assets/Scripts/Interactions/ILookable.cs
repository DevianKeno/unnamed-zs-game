using System;
using UZSG.Entities;

namespace UZSG.Interactions
{
    public enum LookableType {
        Interactable, Resource
    }

    /// <summary>
    /// Represents objects that the Player can look at. lmao
    /// </summary>
    public interface ILookable
    {
        public LookableType LookableType { get; }
    }
}
