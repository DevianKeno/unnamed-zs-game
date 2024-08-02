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
    public float restingSize;
    public float maxSize;
    public float speed;
    public Player player;
    private RectTransform _crosshair;
    private int _isFiring;
    private float _currentSize;
    private float _crouchMultiplier = 1.0f;
    private float _recoilMultiplier = 1.0f;
    private float _addedFiringFactor = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        _crosshair = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log($"{player.Controls.Rigidbody.velocity.sqrMagnitude}, {player.Controls.Rigidbody.velocity.sqrMagnitude > 0}");
        CrosshairChange();

        _crosshair.sizeDelta = new Vector2(_currentSize, _currentSize);
    }

    void CrosshairChange()
    {
        float moveSize = 0f;
        // Set moveSize to maxSize if the player is moving, restingSize if otherwise
        if (isMoving)
        {
            moveSize = maxSize;
        }
        else
        {
            moveSize = restingSize;
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

        // Set recoilMultiplier based on a fixed arbitrary number, experimental, scuffed, and subject to change
        if (player.FPP.HeldItem is GunWeaponController gunWeapon)
        {
            gunWeapon.StateMachine.OnStateChanged += OnGunWeaponStateChanged;
        }
        else
        {
            _recoilMultiplier = 1.0f;
        }

        // Set effectiveMaxSize depending if the player is moving, standing still, crouched, and firing
        float _effectiveMaxSize = moveSize * _crouchMultiplier * _recoilMultiplier;
        // Debug.Log($"moveSize: {moveSize}, crouchMult: {_crouchMultiplier}, recMult: {_recoilMultiplier}, effective: {_effectiveMaxSize}");

        if (_effectiveMaxSize > restingSize)
        {
            _currentSize = Mathf.Lerp(_currentSize, _effectiveMaxSize, Time.deltaTime * speed);

            // Reset the addedFiringFactor and recoilMultiplier to reset crosshair after firing
            _recoilMultiplier -= _addedFiringFactor;
            _addedFiringFactor = 0.0f;
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
            _addedFiringFactor += 0.01f;
            _recoilMultiplier += 0.01f;
        }
        
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
}
