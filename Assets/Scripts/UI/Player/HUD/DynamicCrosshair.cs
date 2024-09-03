using System;

using UnityEngine;

using UZSG.Entities;
using UZSG.Items;
using UZSG.Items.Weapons;
using UZSG.Systems;

namespace UZSG.UI.HUD
{
    public class DynamicCrosshair : MonoBehaviour
    {
        public Player Player;
        [Space]
        public bool Enabled;
        public bool IsVisible { get; private set; }
        
        [Header("Crosshair Settings")]
        public float RestingSize;
        public float MaxSize;
        [Range(0.00f, 1.0f)]
        public float AddedFiringFactor;
        public float Damping;

        float _targetSize;
        float _movingSize = 0.0f;
        float _inAirMultiplier = 1.0f;
        float _crouchingMultiplier = 1.0f;
        float _recoilMultiplier = 1.0f;
        float _baseRecoilValue = 1.0f;
        float _addedTotalFiringFactor = 0.0f;

        [Space, SerializeField] RectTransform rect;

        internal void Initialize(Player player)
        {
            Player = player;
            Player.FPP.OnChangeHeldItem += OnChangeHeldItem;
            Enabled = true;
        }

        internal void OnChangeHeldItem(HeldItemController controller)
        {
            // TODO: fix recoilMult not activating on first change of held weapon item
            // Set recoilMultiplier based on weapon spread data; experimental, scuffed, and subject to change
            if (Player.FPP.HeldItem is GunWeaponController gunWeapon)
            {
                _baseRecoilValue = CalculateBaseRecoilMultiplier(gunWeapon.WeaponData.RangedAttributes.Spread);

                gunWeapon.StateMachine.OnTransition += OnGunWeaponStateChanged;
            }
            else
            {
                _recoilMultiplier = 1.0f;
            }

        }

        void Update()
        {
            if (!Enabled) return;
            
            ResizeCrosshair();
        }
        
        public void Show()
        {
            Enabled = true;
            rect.gameObject.SetActive(true);
            IsVisible = true;
        }
        
        public void Hide()
        {
            Enabled = false;
            rect.gameObject.SetActive(false);
            IsVisible = false;
        }

        void ResizeCrosshair()
        {
            /// Set moveSize to maxSize if the player is moving, restingSize if otherwise
            if (Player.Controls.IsMoving)
            {
                _movingSize = MaxSize;
            }
            else
            {
                _movingSize = RestingSize;
            }

            /// Set jumpMultiplier to a fixed value if the player is jumping or in the air
            if (!Player.Controls.IsGrounded)
            {
                _inAirMultiplier = 1.3f;
            }
            else
            {
                _inAirMultiplier = 1.0f;
            }

            /// Set crouchMultiplier to a fixed value if the player is crouching
            if (Player.Controls.IsCrouching)
            {
                _crouchingMultiplier = 0.85f;
            }
            else
            {
                _crouchingMultiplier = 1.0f;
            }
            
            /// Set effectiveMaxSize depending if the player is moving, standing still, jumping, crouched, and firing
            float effectiveMaxSize = _movingSize * _inAirMultiplier * _crouchingMultiplier * _recoilMultiplier;
            _targetSize = Mathf.Lerp(_targetSize, effectiveMaxSize, Time.deltaTime * Damping);
            /// Reset the addedFiringFactor and recoilMultiplier to reset crosshair after firing
            _recoilMultiplier = 1.0f;
            rect.sizeDelta = new Vector2(_targetSize, _targetSize);
        }

        void OnGunWeaponStateChanged(StateMachine<GunWeaponStates>.TransitionContext transition)
        {
            if (transition.To == GunWeaponStates.Fire)
            {
                _recoilMultiplier = _baseRecoilValue + AddedFiringFactor;
            }
        }

        float CalculateBaseRecoilMultiplier(float bulletSpread)
        {
            return 0.5f + Mathf.Lerp(1, MaxSize / RestingSize, bulletSpread / 15); /// what's 0.5f and 15?
        }
    }
}
