using System;
using UnityEngine;
using UZSG.Player;
using UZSG.UI;
using UZSG.Interactions;
using UZSG.Systems;
using System.Linq.Expressions;
using UZSG.Items;

namespace UZSG.FPP
{
    public enum FPPStates { None, Equip, Idle, Run, Primary, Secondary, Hold, Dequip }
    public struct EquipableArgs
    {
    }

    /// <summary>
    /// Stuff that are visible in FPP when equipped.
    /// </summary>
    public interface IFPPVisible
    {
        public GameObject FPPModel { get; }
    }

    /// <summary>
    /// Handles the Equipable items of the first-person view.
    /// </summary>
    public class FPPHandler : MonoBehaviour
    {
        [SerializeField] Camera _camera;
        [SerializeField] PlayerActions _playerActions;
        
        GameObject _current;
        FPPAnimator _animator;
        StateMachine<FPPStates> _sm;
        WeaponData _equippedData;
        float transitionDuration = 0.1f;

        void Awake()
        {
            _camera = GetComponent<Camera>();
            _sm = GetComponent<StateMachine<FPPStates>>();
        }

        void Start()
        {
            UI.Cursor.Hide();
        }

        /// <summary>
        /// Cache model and data.
        /// </summary>
        public void Load(IFPPVisible obj)
        {
            _current = Instantiate(obj.FPPModel, _camera.transform);
        }
    }
}
