using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditorInternal;
using UnityEngine;
using UZSG.Entities;
using UZSG.FPP;
using UZSG.Items.Weapons;
using UZSG.Systems;

public class DynamicCrosshair : MonoBehaviour
{
    [Header("Crosshair")]
    public Player player;
    public float restingSize;
    public float maxSize;
    public float speed;
    [Range(0.00f, 0.10f)]
    public float addedFiringFactor;

    private RectTransform _crosshair;
    private float _currentSize;
    private float _moveSize = 0.0f;
    private float _jumpMultiplier = 1.0f;
    private float _crouchMultiplier = 1.0f;
    private float _recoilMultiplier = 1.0f;
    private float _baseRecoilValue = 1.0f;
    private float _addedTotalFiringFactor = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        player.FPP.OnChangeHeldItem += OnChangeHeldItem;
        _crosshair = GetComponent<RectTransform>();
    }

    private void OnChangeHeldItem(HeldItemController controller)
    {
        // Set recoilMultiplier based on a fixed arbitrary number; experimental, scuffed, and subject to change
        if (player.FPP.HeldItem is GunWeaponController gunWeapon)
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

    // Update is called once per frame
    void Update()
    {
        CrosshairChange();

        _crosshair.sizeDelta = new Vector2(_currentSize, _currentSize);
    }

    void CrosshairChange()
    {
        
        // Set moveSize to maxSize if the player is moving, restingSize if otherwise
        if (isMoving)
        {
            _moveSize = maxSize;
        }
        else
        {
            _moveSize = restingSize;
        }

        // Set jumpMultiplier to a fixed value if the player is jumping or in the air
        if (!player.Controls.IsGrounded || isJumping)
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
            // _baseRecoilValue = 1.0f;
            _addedTotalFiringFactor = 0; // test/tracker
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
            print($"FF: {addedFiringFactor}, addedTFF: {_addedTotalFiringFactor}, baseRecoil: {_baseRecoilValue}, recMult: {_recoilMultiplier}");
            _recoilMultiplier = _baseRecoilValue + addedFiringFactor;
            print($"AFTER FF: {addedFiringFactor}, addedTFF: {_addedTotalFiringFactor}, baseRecoil: {_baseRecoilValue}, recMult: {_recoilMultiplier}");
        }

    }

    float CalculateBaseRecoilMultiplier(float _baseGunSpread)
    {
        return 0.5f + Mathf.Lerp(1, maxSize / restingSize, (_baseGunSpread / 180));
    }

    bool isMoving
    {
        get
        {
            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
                return true;
            else
                return false;
        }
    }

    bool isJumping
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
