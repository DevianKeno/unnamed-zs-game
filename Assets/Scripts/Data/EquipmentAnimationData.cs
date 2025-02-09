using System;
using System.Collections;
using System.Collections.Generic;

using UZSG.Players;

namespace UZSG.Data
{
    /// <summary>
    /// List of possible animations an FPP model have.
    /// Values are set in Inspector; <b>Do not write</b>.
    /// </summary>
    [Serializable]
    public struct EquipmentAnimationData : IEnumerable
    {
        public string Idle;
        public string Move;
        public string Primary;
        public string PrimaryHold;
        public string PrimaryRelease;
        public string[] PrimaryVariant;
        public string Secondary;
        public string SecondaryHold;
        public string SecondaryRelease;
        public string Equip;
        public string Dequip;

        public readonly string GetAnimHashFromState(ActionStates state)
        {
            return state switch
            {
                ActionStates.Idle => Idle,
                ActionStates.Primary => Primary,
                ActionStates.PrimaryHold => PrimaryHold,
                ActionStates.PrimaryRelease => PrimaryRelease,
                ActionStates.Secondary => Secondary,
                ActionStates.SecondaryHold => SecondaryHold,
                ActionStates.SecondaryRelease => SecondaryRelease,
                ActionStates.Equip => Equip,
                ActionStates.Dequip => Dequip,
                
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public readonly IEnumerator GetEnumerator()
        {
            List<string> list = new()
            {
                Equip,
                Idle,
                Move,
                Primary,
                Secondary,
                SecondaryHold
            };
            list.AddRange(PrimaryVariant);
            return list.GetEnumerator();
        }

        /// <summary>
        /// Get a random primary animation.
        /// </summary>
        public readonly string GetRandomPrimaryVariant()
        {
            if (PrimaryVariant.Length == 0) return null;
            /// There's no actual randomness happening, fix
            return PrimaryVariant[1];
        }
    }
}
