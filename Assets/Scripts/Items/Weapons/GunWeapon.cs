using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using UZSG.Systems;
using UZSG.Players;
using UZSG.Entities;

namespace UZSG.Items.Weapons
{
    public class GunWeapon : EquippedWeapon
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

        GunWeaponStateMachine stateMachine;
        public GunWeaponStateMachine StateMachine => stateMachine;
        AudioSourceController audioSourceController;
        
        void Awake()
        {
            stateMachine = GetComponent<GunWeaponStateMachine>();
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
            if (_hasFired) return false;
            if (_isReloading) return false;

            if (_currentRounds > 0)
            {
                _hasFired = true;
                PlayRandomFireSound();
                SpawnBullet();
                _currentRounds--;
                OnFire?.Invoke();

                if (weaponData.RangedAttributes.FireType == FireType.SemiAuto)
                {
                    return true;
                }
                else if (weaponData.RangedAttributes.FireType == FireType.Automatic)
                {
                    throw new NotImplementedException("Unhandled automatic fire type");
                }
                else if (weaponData.RangedAttributes.FireType == FireType.Burst)
                {
                    throw new NotImplementedException("Unhandled burst fire type");
                }
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

        void PlayRandomFireSound()
        {
            int randIndex = UnityEngine.Random.Range(0, 4); /// magic number, subject to change
            string audioName = $"fire{randIndex}";
            audioSourceController.PlaySound(audioName);
        }

        void SpawnBullet()
        {
            var player = Owner as Player;
            var attr = weaponData.RangedAttributes;

            Game.Entity.Spawn("bullet", (info) =>
            {
                var bullet = info.Entity as Bullet;
                bullet.SetTrajectoryFromPlayer(player);
                bullet.SetBulletEntityOptions(new()
                {
                    Damage = attr.Damage,
                    Velocity = player.Forward,
                    Speed = attr.BulletVelocity <= 0f ? 100f : attr.BulletVelocity,
                });
                bullet.Shoot();
            });
        }

        public void SetWeaponState(GunWeaponStates state)
        {
            stateMachine.ToState(state);
        }

        public override void SetWeaponStateFromPlayerAction(ActionStates state)
        {
            if (state == ActionStates.Primary)
            {
                if (!TryFire()) return;
                
                if (_isAimingDownSights)
                {
                    stateMachine.ToState(GunWeaponStates.ADS_Shoot, _fireRateThreshold);
                }
                else
                {
                    stateMachine.ToState(GunWeaponStates.Fire, _fireRateThreshold);
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
                stateMachine.ToState(GunWeaponStates.ADS_Down);
            } else
            {
                _isAimingDownSights = true;
                stateMachine.ToState(GunWeaponStates.ADS_Up);
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
            stateMachine.ToState(GunWeaponStates.Reload);

            yield return new WaitForSeconds(durationSeconds);

            _currentRounds = 10; 
            _isReloading = false;
            Debug.Log("Completed reload");
        }
    }
}