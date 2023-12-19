using System;
using System.Collections.Generic;
using UnityEngine;

namespace URMG
{
    public struct SalvageData
    {
        // List<Slot> Contents;
    }

    public interface ISalvageable
    {
        public SalvageData Salvage();
    }

    public interface IDurable
    {
        public Toughness Toughness { get; }
        public Durability Durability { get; }
    }

    public class Toughness
    {

    }

    public class Durability
    {
        int _current;
        public int Current { get => _current; }

        int _max;
        public int Max { get => _max; }

        public void Add(int amount)
        {

        }

        public void Remove(int amount)
        {

        }

        public void Set(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException();

        }

        /// <summary>
        /// Breaks the weapon.
        /// </summary>
        public void Break()
        {

        }
    }

    public class Weapon : MonoBehaviour, IDurable
    {
        Toughness _toughness;
        public Toughness Toughness { get => _toughness; }
        Durability _durability;
        public Durability Durability { get => _durability; }
        protected float _baseDamage;
        public float BaseDamage { get => _baseDamage; }
    }

    public class Gun
    {
        float baseDamage;
    }
}
