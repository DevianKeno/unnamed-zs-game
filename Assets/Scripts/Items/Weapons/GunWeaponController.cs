using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

using UZSG.Systems;
using UZSG.Data;
using UZSG.Entities;
using UZSG.Players;
using Unity.Mathematics;

namespace UZSG.Items.Weapons
{
    public class GunWeaponController : HeldWeaponController, IReloadable
    {
        public Player Player => owner as Player;

        /// <summary>
        /// Current rounds in the magazine.
        /// </summary>
        public int CurrentRounds { get; private set; }
        /// <summary>
        /// Reserve bullets in the Player's bag.
        /// </summary>
        public int Reserve { get; private set; } 
        public FiringMode CurrentFiringMode { get; private set; }

        bool _inhibitActions;
        bool _isHoldingFire;
        bool _isReloading;
        bool _hasBulletChambered;
        bool _isFiring;
        bool _hasFired;
        bool _isAimingDownSights;

        float _fireRateThreshold => 60f / WeaponData.RangedAttributes.RoundsPerMinute;

        List<string> fireSoundIds = new();

        public event Action OnFire;
        public event Action OnReload;

        [Space]
        [SerializeField] GunWeaponStateMachine stateMachine;
        public GunWeaponStateMachine StateMachine => stateMachine;
        public GunMuzzleController MuzzleController;
        

        #region Properites

        public bool HasAmmo => CurrentRounds > 0;
        public bool IsAimingDownSights => _isAimingDownSights;
        public bool CanFire
        {
            get
            {
                return !Player.Actions.IsBusy && !Player.Controls.IsRunning;
            }
        }

        #endregion


        #region Initializing methods

        public void InitializeFromGunItemEntity(GunItemEntityInfo info)
        {
            Initialize();

            CurrentRounds = info.Rounds;
            Player.VitalsHUD.AmmoCounter.SetClip(CurrentRounds);
        }

        public override void Initialize()
        {
            InitializeAudioController();
            InitializeBullets();
            InitializeEventsFromOwnerInput();

            Player.Actions.OnPickupItem += OnPlayerPickupItem;
        }

        void InitializeAudioController()
        {
            audioSourceController.LoadAudioAssetsData(WeaponData.AudioAssetsData, () =>
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

        void InitializeBullets()
        {
            Reserve = 0;
            var ammoData = WeaponData.RangedAttributes.Ammo;
            if (ammoData != null)
            {
                Reserve += Player.Inventory.Bag.CountId(ammoData.Id);
                Player.VitalsHUD.AmmoCounter.SetReserve(Reserve);
                Player.VitalsHUD.AmmoCounter.SetCartridgeText(ammoData.Name);
            }
            else
            {
                Game.Console.LogWarning($"Ammo data for '{WeaponData.Id}' is not set");
                Player.VitalsHUD.AmmoCounter.SetCartridgeText("-");
            }
            CurrentRounds = WeaponData.RangedAttributes.ClipSize; /// To be removed
        }

        /// <summary>
        /// Just initializes the gun's different fire sounds.
        /// </summary>
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

        void InitializeEventsFromOwnerInput()
        {
            if (owner is not Player player) return;
            var inputs = player.Actions.Inputs;

            inputs["Primary Action"].started += OnPlayerPrimary;
            inputs["Primary Action"].canceled += OnPlayerPrimary;

            inputs["Secondary Action"].started += OnPlayerSecondary;
            inputs["Secondary Action"].canceled += OnPlayerSecondary;

            inputs["Select Fire"].performed += OnPlayerSelectFire;
        }

        void OnEnable()
        {
            ResetStates();
        }

        #endregion


        #region Player input callbacks

        void OnPlayerPrimary(InputAction.CallbackContext context)
        {
            if (!gameObject.activeSelf) return;
            
            if (context.started && CanFire)
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
                
        Array firingModes = Enum.GetValues(typeof(FiringMode));

        void OnPlayerSelectFire(InputAction.CallbackContext context)
        {
            if (!gameObject.activeSelf) return;

            StartCoroutine(SelectFireCoroutine());
        }

        #endregion


        #region Player event callbacks
        void OnPlayerPickupItem(Item item)
        {
            if (item.Data is AmmoData ammoData)
            {
                Reserve = Player.Inventory.Bag.CountId(ammoData.Id);
                Player.VitalsHUD.AmmoCounter.SetReserve(Reserve);
            }
        }

        #endregion

        
        #region Public methods

        public void InitializeFromGunItemEntity(GunItemEntity gun)
        {
            
        }

        public bool TryReload(float durationSeconds)
        {
            if (_inhibitActions || _isReloading) return false;
            if (Reserve <= 0) return false;

            StartCoroutine(ReloadCoroutine(durationSeconds));
            return true;
        }

        public void ResetStates()
        {
            _isFiring = false;
            _inhibitActions = false;
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

        #endregion


        IEnumerator FireCoroutine()
        {
            if (_inhibitActions || _isReloading || !HasAmmo)
            {
                yield break;
            }

            _inhibitActions = true;
            
            if (CurrentFiringMode == FiringMode.Single)
            {
                Shoot();
                yield return new WaitForSeconds(_fireRateThreshold);
                _inhibitActions = false;
            }
            else if (CurrentFiringMode == FiringMode.Burst)
            {
                for (int i = 0; i < WeaponData.RangedAttributes.BurstFireCount; i++)
                {
                    if (!HasAmmo || _isReloading) break;

                    Shoot();
                    yield return new WaitForSeconds(_fireRateThreshold);
                }
                yield return new WaitForSeconds(WeaponData.RangedAttributes.BurstFireInterval);
                _inhibitActions = false;
            }
            else if (CurrentFiringMode == FiringMode.FullAuto)
            {
                while (_isHoldingFire)
                {
                    if (!HasAmmo || _isReloading) break;

                    Shoot();
                    yield return new WaitForSeconds(_fireRateThreshold);
                }
                _inhibitActions = false;
            }
        }

        IEnumerator SelectFireCoroutine()
        {
            if (_inhibitActions || _isHoldingFire)
            {
                yield break;
            }

            _inhibitActions = true;
            var availableModes = WeaponData.RangedAttributes.FiringModes;
            if (availableModes == FiringModes.None)
            {
                Debug.LogWarning($"The weapon '{ItemData.Id}'s firing mode is set to none! It will not fire!");
                _inhibitActions = false;
                yield break;
            }
    
            do
            {
                CurrentFiringMode = SwitchFiringMode(CurrentFiringMode);
            } while ((GetFiringModeFlag(CurrentFiringMode) & availableModes) == 0);

            Player.VitalsHUD.AmmoCounter.SetFiringMode(CurrentFiringMode);
            yield return new WaitForSeconds(0.33f);
            _inhibitActions = false;
        }

        FiringMode SwitchFiringMode(FiringMode mode)
        {
            return (FiringMode) (((int) mode + 1) % firingModes.Length);
        }

        FiringModes GetFiringModeFlag(FiringMode mode)
        {
            return mode switch
            {
                FiringMode.Single => FiringModes.Single,
                FiringMode.FullAuto => FiringModes.FullAuto,
                FiringMode.Burst => FiringModes.Burst,
            };
        }
        
        void Shoot()
        {
            PlayRandomFireSound();
            SpawnBulletEntity();
            CurrentRounds--;
            MuzzleController?.Fire();
            stateMachine.ToState(GunWeaponStates.Fire);
            OnFire?.Invoke();
            Player.VitalsHUD.AmmoCounter.SetClip(CurrentRounds);
        }

        void PlayRandomFireSound()
        {
            int randIndex = UnityEngine.Random.Range(0, fireSoundIds.Count);
            audioSourceController.PlaySound($"fire_{randIndex}");
        }

        void SpawnBulletEntity()
        {
            var player = Owner as Player;
            var bulletInfo = WeaponData.RangedAttributes.BulletAttributes;
            var trajectory = ApplyBulletSpread(player.Forward, WeaponData.RangedAttributes.Spread);

            Game.Entity.Spawn<Bullet>("bullet", callback: (info) =>
            {
                var bullet = info.Entity;
                bullet.SetBulletAttributes(bulletInfo);
                bullet.SetTrajectory(trajectory);
                bullet.SetPlayerAndShoot(player);
            });
        }

        IEnumerator ReloadCoroutine(float durationSeconds)
        {
            Debug.Log("Reloading weapon...");
            _inhibitActions = true;
            _isReloading = true;
            stateMachine.ToState(GunWeaponStates.Reload);

            int clipSize = WeaponData.RangedAttributes.ClipSize;
            int missingBullets = clipSize - CurrentRounds;

            /// Check for enough reserve bullets
            if (missingBullets <= Reserve)
            {
                CurrentRounds += missingBullets;
                Reserve -= missingBullets;
            }
            else /// not enough reserve bullets, fill the clip with whatever is left
            {
                CurrentRounds += Reserve;
                Reserve = 0;
            }

            Player.VitalsHUD.AmmoCounter.SetClip(CurrentRounds);
            Player.VitalsHUD.AmmoCounter.SetReserve(Reserve);

            yield return new WaitForSeconds(durationSeconds);
            _isReloading = false;
            _inhibitActions = false;

            Debug.Log("Completed reload");
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
