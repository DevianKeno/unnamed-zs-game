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
        int _currentRounds; /// TEST
        public int CurrentRounds
        {
            get { return _currentRounds; }
            set { _currentRounds = value; }
        }
        float _fireDelta;
        float _fireRateThreshold;
        bool _hasFired;
        bool _isReloading;
        bool _isAimingDownSights;

        public event Action OnFire;

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

            _currentRounds = weaponData.RangedAttributes.ClipSize;
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

            if (_currentRounds > 0)
            {
                _hasFired = true;
                /// Summon bullet?
                int randIndex = UnityEngine.Random.Range(0, 4); /// magic number, subject to change
                string audioName = $"fire{randIndex}";
                audioSourceController.PlaySound(audioName);
                _currentRounds--;
                OnFire?.Invoke();
                return true;
            }
            else
            {
                /// Play no ammo sound "click"
                Debug.Log("No ammo");
                // audioSourceController.PlaySound("dryfire");
                return false;
            }
        }

        public void SetWeaponState(RangedWeaponStates state)
        {
            stateMachine.ToState(state);
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
            else if (state == ActionStates.Secondary)
            {
                /// ADS controls, assuming ADS is "toggle"
                ToggleAimDownSights();
            }
        }

        void ToggleAimDownSights()
        {
            if (_isAimingDownSights)
            {
                _isAimingDownSights = false;
                stateMachine.ToState(RangedWeaponStates.ADS_Down);
            } else
            {
                _isAimingDownSights = true;
                stateMachine.ToState(RangedWeaponStates.ADS_Up);
            }
        }

        public bool TryReload(float durationSeconds)
        {
            if (_currentRounds == weaponData.RangedAttributes.ClipSize || _isReloading)
            {
                return false;
            }

            StartCoroutine(ReloadCoroutine(durationSeconds));
            return true;
        }

        IEnumerator ReloadCoroutine(float durationSeconds)
        {
            Debug.Log("Reloading weapon...");
            _isReloading = true;
            stateMachine.ToState(RangedWeaponStates.Reload);

            yield return new WaitForSeconds(durationSeconds);

            _currentRounds = 10; 
            _isReloading = false;
            Debug.Log("Completed reload");
        }
    }
}