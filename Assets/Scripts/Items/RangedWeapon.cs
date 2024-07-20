using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UZSG.Players;
using UZSG.Systems;

namespace UZSG.Items.Weapons
{
    public abstract class WeaponEquipped : MonoBehaviour
    {
        [SerializeField] protected WeaponData weaponData;
        public WeaponData WeaponData => weaponData;
        public abstract void Initialize();
    }

    public class RangedWeapon : WeaponEquipped
    {
        int rounds = 9999; /// TEST
        float _fireDelta;
        float _fireRateThreshold;
        bool _hasFired;
        bool _isReloading;
        bool _isAimingDownSights;

        RangedWeaponStateMachine stateMachine;
        public RangedWeaponStateMachine StateMachine => stateMachine;
        AudioSourceController audioSourceController;
        
        void Awake()
        {
            stateMachine = GetComponent<RangedWeaponStateMachine>();
        }

        public override void Initialize()
        {
            audioSourceController ??= gameObject.AddComponent<AudioSourceController>();
            audioSourceController.LoadAudioAssetIds(weaponData.AudioData.AudioAssetIds);
        }

        void Update()
        {
            if (_hasFired)
            {
                _fireDelta += Time.deltaTime;

                if (_fireDelta > 60f / weaponData.RangedAttributes.RoundsPerMinute)
                {
                    _hasFired = false;
                    _fireDelta = 0f;
                }
            }
        }

        public bool TryFire()
        {
            if (_isReloading) return false;
            if (_hasFired) return false;

            _hasFired = true;
            if (rounds > 0)
            {
                /// Summon bullet?
                int randIndex = UnityEngine.Random.Range(0, 4); /// magic number, subject to change
                string audioName = $"fire{randIndex}";
                audioSourceController.PlaySound(audioName);
                rounds--;
                return true;
            }
            else
            {
                /// Play no ammo sound "click"
                // audioSourceController.PlaySound("dryfire");
                return false;
            }
        }

        public void SetWeaponStateFromPlayerAction(ActionStates state)
        {
            if (state == ActionStates.Primary)
            {
                if (!TryFire()) return;
                
                if (_isAimingDownSights)
                {
                    stateMachine.ToState(RangedWeaponStates.ADS_Shoot, _fireRateThreshold);
                }
                else
                {
                    stateMachine.ToState(RangedWeaponStates.Fire, _fireRateThreshold);
                }
            }
            /// ADS controls, assumes ADS is "hold"
            else if (state == ActionStates.SecondaryHold)
            {
                _isAimingDownSights = true;
                stateMachine.ToState(RangedWeaponStates.ADS_Up);
            }
            else if (state == ActionStates.SecondaryRelease)
            {
                _isAimingDownSights = false;
                stateMachine.ToState(RangedWeaponStates.ADS_Down);
            }
        }

        public void StartReload()
        {
            StartCoroutine(ReloadCoroutine());
        }

        IEnumerator ReloadCoroutine()
        {
            if (_isReloading) yield return null;

            _isReloading = true;
            stateMachine.ToState(RangedWeaponStates.Reload);

            yield return new WaitForSeconds(1f);
            rounds = 10; 
            _isReloading = false;
        }
    }
}