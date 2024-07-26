using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UZSG.Systems;
using UZSG.Players;
using UZSG.Entities;
using UnityEngine.InputSystem;

namespace UZSG.Items.Weapons
{
    public class GunWeaponController : WeaponController, IReloadable
    {
        public Player Player => owner as Player;
        public WeaponData WeaponData => ItemData as WeaponData;
        int _currentRounds;
        public int CurrentRounds
        {
            get { return _currentRounds; }
            set { _currentRounds = value; }
        }
        public bool HasAmmo => _currentRounds > 0;
        float _fireDelta;
        float _fireRateThreshold => 60f / WeaponData.RangedAttributes.RoundsPerMinute;
        bool _inhibitActions;
        bool _isHoldingFire;
        bool _hasFired;
        bool _isReloading;
        bool _hasBulletChambered;
        bool _isAimingDownSights;
        public bool IsAimingDownSights => _isAimingDownSights;
        FiringMode _currentFiringMode;
        public FiringMode CurrentFiringMode => _currentFiringMode;

        List<string> fireSoundIds = new();

        public event Action OnFire;
        public event Action OnReload;

        [Space(10)]
        [SerializeField] GunWeaponStateMachine stateMachine;
        public GunWeaponStateMachine StateMachine => stateMachine;
        public GunMuzzleController MuzzleController;
        [SerializeField] AudioSourceController audioSourceController;

        public void Initialize(GunItemEntityInfo info)
        {
            Initialize();
            _currentRounds = info.Rounds;
            Player.HUD.AmmoCounter.SetCurrent(_currentRounds);
        }

        public override void Initialize()
        {
            InitializeAudioController();
            InitializeEventsFromOwnerInput();
            _currentRounds = WeaponData.RangedAttributes.ClipSize; /// To be removed
        }

        void InitializeAudioController()
        {
            audioSourceController.LoadAudioAssetIds(WeaponData.AudioData, () =>
            {
                InitializeGunSounds();
            });

            int audioPoolSize;
            if ((WeaponData.RangedAttributes.FiringModes & FiringModes.FullAuto) == FiringModes.FullAuto
                || (WeaponData.RangedAttributes.FiringModes & FiringModes.Burst) == FiringModes.Burst)
            {
                audioPoolSize = 24;
            }
            else
            {
                audioPoolSize = 8;
            }
            audioSourceController.CreateAudioPool(audioPoolSize); 
        }

        void InitializeEventsFromOwnerInput()
        {
            if (owner is not Player player) return;

            var inputs = player.Controls.Inputs;
            inputs["Primary Action"].started += OnPlayerPrimary;
            inputs["Primary Action"].canceled += OnPlayerPrimary;
            inputs["Secondary Action"].started += OnPlayerSecondary;
            inputs["Secondary Action"].canceled += OnPlayerSecondary;
            inputs["Reload"].performed += OnPlayerReload;
            inputs["Select Fire"].performed += OnPlayerSelectFire;
        }


        #region Player input callbacks

        void OnPlayerPrimary(InputAction.CallbackContext context)
        {
            if (!gameObject.activeSelf) return;
            
            if (context.started)
            {
                _isHoldingFire = true;
                StartCoroutine(FireCoroutine());
            }
            else if (context.canceled)
            {
                _isHoldingFire = false;
                StopCoroutine(FireCoroutine());
            }
        }

        void OnPlayerSecondary(InputAction.CallbackContext context)
        {
            if (!gameObject.activeSelf) return;
        }
        
        void OnPlayerReload(InputAction.CallbackContext context)
        {
            if (!gameObject.activeSelf) return;
            
            // TryReload();
        }
        
        Array firingModes = Enum.GetValues(typeof(FiringMode));

        void OnPlayerSelectFire(InputAction.CallbackContext context)
        {
            if (!gameObject.activeSelf) return;

            StartCoroutine(SelectFireCoroutine());
        }

        IEnumerator SelectFireCoroutine()
        {
            if (_inhibitActions || _isHoldingFire)
            {
                yield break;
            }

            _inhibitActions = true;
            var currentMode = (int)_currentFiringMode;
            var availableModes = WeaponData.RangedAttributes.FiringModes;

            do
            {
                currentMode = (currentMode + 1) % firingModes.Length;
                _currentFiringMode = (FiringMode)currentMode;

                if ((availableModes & GetFiringModeFlag(_currentFiringMode)) != 0)
                {
                    Player.HUD.AmmoCounter.SetFiringMode(_currentFiringMode);
                    break;
                }

            } while (true);
            yield return new WaitForSeconds(0.33f);
            _inhibitActions = false;
        }

        FiringModes GetFiringModeFlag(FiringMode mode)
        {
            return mode switch
            {
                FiringMode.Single => FiringModes.Single,
                FiringMode.FullAuto => FiringModes.FullAuto,
                FiringMode.Burst => FiringModes.Burst,
                _ => FiringModes.All
            };
        }

        #endregion

        IEnumerator FireCoroutine()
        {
            if (_inhibitActions || _isReloading || !HasAmmo)
            {
                yield break;
            }

            _inhibitActions = true;
            
            if (_currentFiringMode == FiringMode.Single)
            {
                Shoot();
                yield return new WaitForSeconds(_fireRateThreshold);
                _inhibitActions = false;
            }
            else if (_currentFiringMode == FiringMode.Burst)
            {
                for (int i = 0; i < WeaponData.RangedAttributes.BurstFireCount; i++)
                {
                    if (_currentRounds <= 0 || _isReloading) break;

                    Shoot();
                    yield return new WaitForSeconds(_fireRateThreshold);
                }
                yield return new WaitForSeconds(WeaponData.RangedAttributes.BurstFireInterval);
                _inhibitActions = false;
            }
            else if (_currentFiringMode == FiringMode.FullAuto)
            {
                while (_isHoldingFire)
                {
                    if (_currentRounds <= 0 || _isReloading) break;

                    Shoot();
                    yield return new WaitForSeconds(_fireRateThreshold);
                }
                _inhibitActions = false;
            }
        }
        
        void Shoot()
        {
            PlayRandomFireSound();
            SpawnBulletEntity();
            _currentRounds--;
            MuzzleController?.Fire();
            stateMachine.ToState(GunWeaponStates.Fire);
            OnFire?.Invoke();
            Player.HUD.AmmoCounter.SetCurrent(_currentRounds);
        }

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

        void PlayRandomFireSound()
        {
            int randIndex = UnityEngine.Random.Range(0, fireSoundIds.Count);
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
                var trajectory = ApplyBulletSpread(player.Forward, WeaponData.RangedAttributes.Spread);
                bullet.SetTrajectory(trajectory);
                bullet.SetPlayerAndShoot(player);
            });
        }

        public bool TryReload(float durationSeconds)
        {
            if (_inhibitActions || _currentRounds == WeaponData.RangedAttributes.ClipSize || _isReloading)
            {
                return false;
            }

            StartCoroutine(ReloadCoroutine(durationSeconds));
            return true;
        }

        IEnumerator ReloadCoroutine(float durationSeconds)
        {
            Debug.Log("Reloading weapon...");
            _inhibitActions = true;
            _isReloading = true;
            stateMachine.ToState(GunWeaponStates.Reload);

            yield return new WaitForSeconds(durationSeconds);

            _currentRounds = WeaponData.RangedAttributes.ClipSize;
            _isReloading = false;
            _inhibitActions = false;
            Player.HUD.AmmoCounter.SetCurrent(_currentRounds);
            Debug.Log("Completed reload");
        }

        public override void SetStateFromAction(ActionStates state)
        {
            if (!gameObject.activeSelf) return;

            if (state == ActionStates.Primary)
            {                
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
            }
            else
            {
                _isAimingDownSights = true;
                stateMachine.ToState(GunWeaponStates.ADS_Up);
            }
        }
        
        Vector3 ApplyBulletSpread(Vector3 direction, float spreadAngle)
        {
            float spreadX = UnityEngine.Random.Range(-spreadAngle / 2, spreadAngle / 2);
            float spreadY = UnityEngine.Random.Range(-spreadAngle / 2, spreadAngle / 2);

            Quaternion spreadRotation = Quaternion.Euler(spreadY, spreadX, 0);
            Vector3 spreadDirection = spreadRotation * direction;

            return spreadDirection.normalized;
        }
    }
}
