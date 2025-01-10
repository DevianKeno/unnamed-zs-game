using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

using UZSG.Systems;
using UZSG.Data;
using UZSG.Entities;
using UZSG.Players;

namespace UZSG.Items.Weapons
{
    public class GunWeaponController : HeldWeaponController, IReloadable
    {
        public Player Player => owner as Player;
        public WeaponRangedAttributes RangedAttributes => WeaponData.RangedAttributes;

        bool _inhibitActions;
        bool _isHoldingFire;
        bool _isReloading;
        bool _hasBulletChambered;
        bool _isFiring;
        bool _hasFired;
        bool _isAimingDownSights;
        bool _hasValidAmmoData;

        float SecondsPerRound => 60f / RangedAttributes.RoundsPerMinute;

        List<string> fireSoundIds = new();

        public event Action OnFire;
        public event Action OnReload;

        [Space]
        [SerializeField] GunWeaponStateMachine stateMachine;
        public GunWeaponStateMachine StateMachine => stateMachine;
        public GunMuzzleController MuzzleController;
        

        #region Properites

        /// <summary>
        /// Current rounds in the magazine.
        /// </summary>
        public int CurrentRounds { get; private set; }
        /// <summary>
        /// Reserve bullets in the Player's bag.
        /// </summary>
        public int Reserve
        {
            get
            {
                if (_hasValidAmmoData)
                {
                    return Player.Inventory.Bag.CountId(RangedAttributes.Ammo.Id);
                }
                else
                {
                    return -1;
                }
            }
        }
        public FiringMode CurrentFiringMode { get; private set; }
        public bool HasAmmo => CurrentRounds > 0;
        public bool IsAimingDownSights => _isAimingDownSights;
        /// <summary>
        /// Whether the player can pull the trigger.
        /// </summary>
        public bool CanShoot
        {
            get
            {
                return !Player.Actions.IsBusy && !Player.Controls.IsRunning;
            }
        }
        public bool MagIsFull
        {
            get => CurrentRounds >= RangedAttributes.ClipSize;
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

            _hasValidAmmoData = RangedAttributes.Ammo != null;
            Player.Actions.OnPickupItem += OnPlayerPickupItem;
        }

        void InitializeAudioController()
        {
            audioSourceController.LoadAudioAssetsData(WeaponData.AudioAssetsData, () =>
            {
                InitializeGunSounds();
            });

            int audioPoolSize;
            if ((RangedAttributes.FiringModes & FiringModes.FullAuto) == FiringModes.FullAuto
                || (RangedAttributes.FiringModes & FiringModes.Burst) == FiringModes.Burst)
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
            var ammoData = RangedAttributes.Ammo;
            if (ammoData != null)
            {
                Player.VitalsHUD.AmmoCounter.SetReserve(Reserve);
                Player.VitalsHUD.AmmoCounter.SetCartridgeText(ammoData.DisplayName);
            }
            else
            {
                Game.Console.Warn($"Ammo data for '{WeaponData.Id}' is not set");
                Player.VitalsHUD.AmmoCounter.SetCartridgeText("-");
            }
            CurrentRounds = RangedAttributes.ClipSize; /// To be removed
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
            
            if (context.started && CanShoot)
            {
                if (IsBroken)
                {
                    /// prompt broken weapon
                    return;
                }

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
            if (_inhibitActions || _isReloading || MagIsFull) return false;
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
                    stateMachine.ToState(GunWeaponStates.ADS_Shoot, SecondsPerRound);
                }
                else
                {
                    stateMachine.ToState(GunWeaponStates.Fire, SecondsPerRound);
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
                yield return new WaitForSeconds(SecondsPerRound);
                _inhibitActions = false;
            }
            else if (CurrentFiringMode == FiringMode.Burst)
            {
                for (int i = 0; i < RangedAttributes.BurstFireCount; i++)
                {
                    if (!HasAmmo || _isReloading) break;

                    Shoot();
                    yield return new WaitForSeconds(SecondsPerRound);
                }
                yield return new WaitForSeconds(RangedAttributes.BurstFireInterval);
                _inhibitActions = false;
            }
            else if (CurrentFiringMode == FiringMode.FullAuto)
            {
                while (_isHoldingFire)
                {
                    if (!HasAmmo || _isReloading) break;

                    Shoot();
                    yield return new WaitForSeconds(SecondsPerRound);
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
            var availableModes = RangedAttributes.FiringModes;
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
            var bulletInfo = RangedAttributes.BulletAttributes;
            var trajectory = ApplyBulletSpread(player.Forward, RangedAttributes.Spread);

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
            _inhibitActions = true;
            _isReloading = true;
            stateMachine.ToState(GunWeaponStates.Reload);

            int missingBullets = RangedAttributes.ClipSize - CurrentRounds;
            Item ammoToTake;
            
            yield return new WaitForSeconds(durationSeconds);
            
            /// Check for enough reserve bullets
            if (missingBullets <= Reserve)
            {
                CurrentRounds += missingBullets;
                ammoToTake = new(RangedAttributes.Ammo, missingBullets);
            }
            else /// not enough reserve bullets, fill the clip with whatever is left
            {
                CurrentRounds += Reserve;
                ammoToTake = new(RangedAttributes.Ammo, Reserve);
            }

            Player.Inventory.Bag.TakeItem(ammoToTake);
            Player.VitalsHUD.AmmoCounter.SetClip(CurrentRounds);
            Player.VitalsHUD.AmmoCounter.SetReserve(Reserve);
            _isReloading = false;
            _inhibitActions = false;
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
