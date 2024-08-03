using System;

using UnityEngine;

using UZSG.Entities;
using UZSG.FPP;
using UZSG.Items;
using UZSG.Items.Weapons;
using UZSG.Systems;

namespace UZSG.UI
{
    public class DynamicCrosshair : MonoBehaviour
    {
        [Header("Crosshair")]
        public Player Player;
        public float RestingSize;
        public float MaxSize;
        public float Speed;
        [Range(0.00f, 0.10f)]
        public float AddedFiringFactor;

        float _currentSize;
        float _moveSize = 0.0f;
        float _jumpMultiplier = 1.0f;
        float _crouchMultiplier = 1.0f;
        float _recoilMultiplier = 1.0f;
        float _baseRecoilValue = 1.0f;
        float _addedTotalFiringFactor = 0.0f;
        
        [SerializeField] RectTransform crosshair;

        void Start()
        {
            Player.FPP.OnChangeHeldItem += OnChangeHeldItem;
        }

        // Update is called once per frame
        void Update()
        {
            CrosshairChange();

            crosshair.sizeDelta = new Vector2(_currentSize, _currentSize);
        }

        void OnChangeHeldItem(HeldItemController controller)
        {
            // Set recoilMultiplier based on a fixed arbitrary number; experimental, scuffed, and subject to change
            if (Player.FPP.HeldItem is GunWeaponController gunWeapon)
            {
                _baseRecoilValue = CalculateBaseRecoilMultiplier(gunWeapon.WeaponData.RangedAttributes.Spread);

                gunWeapon.StateMachine.OnStateChanged += OnGunWeaponStateChanged;
                //print($"FF: {addedFiringFactor}, addedTFF: {_addedTotalFiringFactor}, baseRecoil: {_baseRecoilValue}, recMult: {_recoilMultiplier}");
            }
            else
            {
                print("called");
                _recoilMultiplier = 1.0f;
            }

        }

        void CrosshairChange()
        {
            // Set moveSize to maxSize if the player is moving, restingSize if otherwise
            if (IsMoving)
            {
                _moveSize = MaxSize;
            }
            else
            {
                _moveSize = RestingSize;
            }

            // Set jumpMultiplier to a fixed value if the player is jumping or in the air
            if (!Player.Controls.IsGrounded || IsJumping)
            {
                _jumpMultiplier = 1.3f;
            }
            else
            {
                _jumpMultiplier = 1.0f;
            }

            // Set crouchMultiplier to a fixed value if the player is crouching
            if (Player.Controls.IsCrouching)
            {
                _crouchMultiplier = 0.7f;
            }
            else
            {
                _crouchMultiplier = 1.0f;
            }

            
            // Set effectiveMaxSize depending if the player is moving, standing still, jumping, crouched, and firing
            float _effectiveMaxSize = _moveSize * _jumpMultiplier * _crouchMultiplier * _recoilMultiplier;

            //print($"moveSize: {_moveSize}, jumpMult: {_jumpMultiplier}, crouchMult: {_crouchMultiplier}, recMult: {_recoilMultiplier}, effective: {_effectiveMaxSize}");

            if (_effectiveMaxSize > RestingSize)
            {
                _currentSize = Mathf.Lerp(_currentSize, _effectiveMaxSize, Time.deltaTime * Speed);

                // Reset the addedFiringFactor and recoilMultiplier to reset crosshair after firing
                _recoilMultiplier = 1.0f;
                // _baseRecoilValue = 1.0f;
                _addedTotalFiringFactor = 0; // test/tracker
            }
            else
            {
                _currentSize = Mathf.Lerp(_currentSize, RestingSize, Time.deltaTime * Speed);
            }
        }

        void OnGunWeaponStateChanged(object sender, StateMachine<GunWeaponStates>.StateChangedContext e)
        {
            if (e.To == GunWeaponStates.Fire)
            {
                print($"FF: {AddedFiringFactor}, addedTFF: {_addedTotalFiringFactor}, baseRecoil: {_baseRecoilValue}, recMult: {_recoilMultiplier}");
                _recoilMultiplier = _baseRecoilValue + AddedFiringFactor;
                print($"AFTER FF: {AddedFiringFactor}, addedTFF: {_addedTotalFiringFactor}, baseRecoil: {_baseRecoilValue}, recMult: {_recoilMultiplier}");
            }

        }

        float CalculateBaseRecoilMultiplier(float _baseGunSpread)
        {
            return 0.5f + Mathf.Lerp(1, MaxSize / RestingSize, (_baseGunSpread / 180));
        }

        bool IsMoving
        {
            get
            {
                if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
                    return true;
                else
                    return false;
            }
        }

        bool IsJumping
        {
            get
            {
                if (Input.GetAxis("Jump") != 0)
                    return true;
                else
                    return false;
            }
        }
    }
}