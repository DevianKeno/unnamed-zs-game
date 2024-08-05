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
        [Header("Crosshair")]
        public Player player;
        public float restingSize;
        public float maxSize;
        public float speed;
        [Range(0.00f, 1.0f)]
        public float addedFiringFactor;

        float _currentSize;
        float _moveSize = 0.0f;
        float _jumpMultiplier = 1.0f;
        float _crouchMultiplier = 1.0f;
        float _recoilMultiplier = 1.0f;
        float _baseRecoilValue = 1.0f;
        float _addedTotalFiringFactor = 0.0f;

        [SerializeField] RectTransform _crosshair;

        void Start()
        {
            player.FPP.OnChangeHeldItem += OnChangeHeldItem;
            _crosshair = GetComponent<RectTransform>();
        }

        void OnChangeHeldItem(HeldItemController controller)
        {
            print("CHANGED!");
            // TODO: fix recoilMult not activating on first change of held weapon item
            // Set recoilMultiplier based on weapon spread data; experimental, scuffed, and subject to change
            if (player.FPP.HeldItem is GunWeaponController gunWeapon)
            {
                _baseRecoilValue = CalculateBaseRecoilMultiplier(gunWeapon.WeaponData.RangedAttributes.Spread);

                gunWeapon.StateMachine.OnStateChanged += OnGunWeaponStateChanged;
            }
            else
            {
                _recoilMultiplier = 1.0f;
            }

        }

        void Update()
        {
            CrosshairChange();

            _crosshair.sizeDelta = new Vector2(_currentSize, _currentSize);
        }

        void CrosshairChange()
        {
            // Set moveSize to maxSize if the player is moving, restingSize if otherwise
            if (IsMoving)
            {
                _moveSize = maxSize;
            }
            else
            {
                _moveSize = restingSize;
            }

            // Set jumpMultiplier to a fixed value if the player is jumping or in the air
            if (!player.Controls.IsGrounded || IsJumping)
            {
                _jumpMultiplier = 1.3f;
            }
            else
            {
                _jumpMultiplier = 1.0f;
            }

            // Set crouchMultiplier to a fixed value if the player is crouching
            if (player.Controls.IsCrouching)
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

            if (_effectiveMaxSize > restingSize)
            {
                _currentSize = Mathf.Lerp(_currentSize, _effectiveMaxSize, Time.deltaTime * speed);

                // Reset the addedFiringFactor and recoilMultiplier to reset crosshair after firing
                _recoilMultiplier = 1.0f;
            }
            else
            {
                _currentSize = Mathf.Lerp(_currentSize, restingSize, Time.deltaTime * speed);
            }
        }

        void OnGunWeaponStateChanged(object sender, StateMachine<GunWeaponStates>.StateChangedContext e)
        {
            if (e.To == GunWeaponStates.Fire)
            {
                _recoilMultiplier = _baseRecoilValue + addedFiringFactor;
            }
        }

        float CalculateBaseRecoilMultiplier(float _baseGunSpread)
        {
            return 0.5f + Mathf.Lerp(1, maxSize / restingSize, (_baseGunSpread / 15));
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
