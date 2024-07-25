using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UZSG.Systems;
using UZSG.Players;
using UZSG.Entities;

namespace UZSG.Items.Weapons
{
    public interface IReloadable
    {
        public bool TryReload(float durationSeconds);
    }

    public class GunWeaponController : WeaponController, IReloadable
    {
        public WeaponData WeaponData => ItemData as WeaponData;
        int _currentRounds;
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
        public event Action OnReload;

        GunWeaponStateMachine stateMachine;
        public GunWeaponStateMachine StateMachine => stateMachine;
        AudioSourceController audioSourceController;

        public GunMuzzleController MuzzleController;
        [SerializeField] int burstFireCount = 3; /// Number of rounds per burst

        void Awake()
        {
            stateMachine = GetComponent<GunWeaponStateMachine>();
            audioSourceController ??= gameObject.AddComponent<AudioSourceController>();
        }

        public void Initialize(GunItemEntityInfo info)
        {
            Initialize();
            _currentRounds = info.Rounds;
        }

        public override void Initialize()
        {
            audioSourceController.LoadAudioAssetIds(WeaponData.AudioData, () =>
            {
                InitializeGunSounds();
            });

            _currentRounds = WeaponData.RangedAttributes.ClipSize;
        }

        List<string> fireSoundIds = new();

        void InitializeGunSounds()
        {
            foreach (AudioClip clip in audioSourceController.AudioClips)
            {
                var soundString = clip.name.Split('_');
                if (soundString[0] == "fire")
                {
                    fireSoundIds.Add(clip.name);
                }
            }
        }

        void Update()
        {
            if (_hasFired)
            {
                _fireDelta += Time.deltaTime;

                if (_fireDelta > 60f / WeaponData.RangedAttributes.RoundsPerMinute)
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
                SpawnBulletEntity();
                _currentRounds--;
                MuzzleController?.Fire();
                OnFire?.Invoke();

                if ((WeaponData.RangedAttributes.FiringModes & FiringModes.Single) == FiringModes.Single)
                {
                    return true;
                }
                else if ((WeaponData.RangedAttributes.FiringModes & FiringModes.Automatic) == FiringModes.Automatic)
                {
                    StartCoroutine(AutomaticFire());
                    return true;
                }
                else if ((WeaponData.RangedAttributes.FiringModes & FiringModes.Burst) == FiringModes.Burst)
                {
                    StartCoroutine(BurstFire());
                    return true;
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

        IEnumerator AutomaticFire()
        {
            while ((WeaponData.RangedAttributes.FiringModes & FiringModes.Automatic) == FiringModes.Automatic)
            {
                if (_currentRounds <= 0 || _isReloading) break;
                PlayRandomFireSound();
                SpawnBulletEntity();
                _currentRounds--;
                MuzzleController.Fire();
                OnFire?.Invoke();
                yield return new WaitForSeconds(60f / WeaponData.RangedAttributes.RoundsPerMinute);
            }
        }

        IEnumerator BurstFire()
        {
            for (int i = 0; i < burstFireCount; i++)
            {
                if (_currentRounds <= 0 || _isReloading) break;
                PlayRandomFireSound();
                SpawnBulletEntity();
                _currentRounds--;
                MuzzleController.Fire();
                OnFire?.Invoke();
                yield return new WaitForSeconds(60f / WeaponData.RangedAttributes.RoundsPerMinute);
            }
        }

        void PlayRandomFireSound()
        {
            int randIndex = UnityEngine.Random.Range(0, fireSoundIds.Count - 1);
            audioSourceController.PlaySound(fireSoundIds[randIndex]);
        }

        void SpawnBulletEntity()
        {
            var player = Owner as Player;
            var bulletInfo = WeaponData.RangedAttributes.BulletAttributes;
            Game.Entity.Spawn("bullet", (info) =>
            {
                var bullet = info.Entity as Bullet;
                bullet.SetBulletAttributes(bulletInfo);
                bullet.SetTrajectoryFromPlayerAndShoot(player);
            });
        }

        public void SetWeaponState(GunWeaponStates state)
        {
            stateMachine.ToState(state);
        }

        public override void SetStateFromAction(ActionStates state)
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
            if (_currentRounds == WeaponData.RangedAttributes.ClipSize || _isReloading)
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

            _currentRounds = WeaponData.RangedAttributes.ClipSize; 
            _isReloading = false;
            Debug.Log("Completed reload");
        }
    }
}
