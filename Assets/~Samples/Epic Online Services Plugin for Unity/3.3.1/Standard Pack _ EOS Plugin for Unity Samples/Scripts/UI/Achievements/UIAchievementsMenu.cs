/*
* Copyright (c) 2021 PlayEveryWare
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

﻿using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using Epic.OnlineServices;
using Epic.OnlineServices.Achievements;
using Epic.OnlineServices.Ecom;
using Epic.OnlineServices.UI;
using Epic.OnlineServices.Stats;

using PlayEveryWare.EpicOnlineServices;

namespace PlayEveryWare.EpicOnlineServices.Samples
{
    /// <summary>
    /// Unity UI sample that uses <c>AchievementManager</c> to demo features.  Can be used as a template or starting point for implementing Achievement features.
    /// </summary>

    public class UIAchievementsMenu : MonoBehaviour, ISampleSceneUI
    {
        [Header("Achievements UI")]
        public Button refreshDataButton;
        public Button loginIncreaseButton;
        public Toggle showDefinitionToggle;
        public Button unlockAchievementButton;
        public Text definitionsDescription;
        public ScrollRect scrollRect;
        public Transform achievementListContainer;
        public UIAchievementButton itemTemplate;
        public RawImage achievementUnlockedIcon;
        public RawImage achievementLockedIcon;

        [Header("Controller")]
        public GameObject UIFirstSelected;

        private List<UIAchievementButton> achievementListItems;

        private bool displayDefinition = false;
        private int displayIndex = -1;

        class AchievementData
        {
            public DefinitionV2 Definition;
            public PlayerAchievement? PlayerData;
        }
        List<AchievementData> achievementDataList;

        private void Awake()
        {
            achievementDataList = new List<AchievementData>();
            achievementListItems = new List<UIAchievementButton>();

            HideMenu();
        }

        private void OnEnable()
        {
            AchievementsService.Instance.AddUpdateCallback(OnAchievementDataUpdated);
        }

        private void OnDisable()
        {
            AchievementsService.Instance.RemoveUpdateCallback(OnAchievementDataUpdated);
        }

        private void OnDestroy()
        {
            EOSManager.Instance.RemoveManager<AchievementsService>();
        }

        private void Update()
        {
            // Controller: Detect if nothing is selected and controller input detected, and set default
            if (UIFirstSelected.activeSelf != true
                || EventSystem.current == null || EventSystem.current.currentSelectedGameObject != null
                || !InputUtility.WasGamepadUsedLastFrame())
            {
                return;
            }

            // Controller
            EventSystem.current.SetSelectedGameObject(UIFirstSelected);
            Debug.Log("Nothing currently selected, default to UIFirstSelected: EventSystem.current.currentSelectedGameObject = " + EventSystem.current.currentSelectedGameObject);
        }

        public void ShowMenu()
        {
            refreshDataButton.gameObject.SetActive(true);
            loginIncreaseButton.gameObject.SetActive(true);
            showDefinitionToggle.gameObject.SetActive(true);

            // Controller
            EventSystem.current.SetSelectedGameObject(UIFirstSelected);
        }

        public void HideMenu()
        {
            refreshDataButton.gameObject.SetActive(false);
            loginIncreaseButton.gameObject.SetActive(false);
            showDefinitionToggle.gameObject.SetActive(false);
            unlockAchievementButton.gameObject.SetActive(false);
            definitionsDescription.gameObject.SetActive(false);
            scrollRect.gameObject.SetActive(false);
            achievementUnlockedIcon.gameObject.SetActive(false);
            achievementLockedIcon.gameObject.SetActive(false);
        }

        public async void IncrementLoginStat()
        {
            await StatsService.Instance.IngestStatAsync("login_count", 1);
        }

        //manually unlock achievement being displayed
        //TODO: refresh achievement data without having to log out
        public async void UnlockAchievement()
        {
            if (displayIndex < 0 || displayIndex > AchievementsService.GetAchievementsCount())
            {
                return;
            }

            var definition = AchievementsService.Instance.GetAchievementDefinitionAtIndex(displayIndex);

            try
            {
                await AchievementsService.Instance.UnlockAchievementAsync(definition.AchievementId);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        public async void OnRefreshDataClicked()
        {
            await AchievementsService.Instance.RefreshAsync();
        }

        // Achievements
        private async void OnAchievementDataUpdated()
        {
            foreach (var item in achievementListItems)
            {
                Destroy(item.gameObject);
            }
            achievementListItems.Clear();
            achievementDataList.Clear();

            uint achievementDefCount = AchievementsService.GetAchievementsCount();

            if (achievementDefCount > 0)
            {
                foreach (var achievementDef in AchievementsService.Instance.CachedAchievements())
                {
                    achievementDataList.Add(new AchievementData()
                    {
                        Definition = achievementDef,
                        PlayerData = null
                    });
                }

                var userId = EOSManager.Instance.GetProductUserId();
                if (userId.IsValid())
                {
                    foreach (var playerAch in AchievementsService.Instance.CachedPlayerAchievements(userId))
                    {
                        var achData = achievementDataList.Find((AchievementData data) => data.Definition.AchievementId == playerAch.AchievementId);
                        if (achData != null)
                        {
                            achData.PlayerData = playerAch;
                        }
                    }
                }

                scrollRect.gameObject.SetActive(true);
                scrollRect.content.sizeDelta = new Vector2(0, achievementDefCount * 30);

                int i = 0;
                List<AchievementData> achievementDataListCopy = new(achievementDataList);
                foreach (var achievementData in achievementDataListCopy)
                {
                    var newButton = Instantiate(itemTemplate, achievementListContainer);
                    newButton.SetNameText(achievementData.Definition.AchievementId);
                    newButton.gameObject.SetActive(true);
                    newButton.index = i;
                    bool unlocked = achievementData.PlayerData.HasValue && achievementData.PlayerData.Value.Progress >= 1;
                    Texture2D iconTex = null;
                    int iconGiveupFrame = Time.frameCount + 120;
                    while (iconTex == null)
                    {
                        iconTex = unlocked ?
                            AchievementsService.Instance.GetAchievementUnlockedIconTexture(achievementData.Definition.AchievementId)
                           : AchievementsService.Instance.GetAchievementLockedIconTexture(achievementData.Definition.AchievementId);
                        await System.Threading.Tasks.Task.Yield();

                        if (Time.frameCount > iconGiveupFrame)
                        {
                            UnityEngine.Debug.LogWarning("Timeout : Failed to get icon");
                            break;
                        }
                    }
                    newButton.SetIconTexture(iconTex);
                    i += 1;
                    achievementListItems.Add(newButton);
                }
            }
            else
            {
                definitionsDescription.text = "No Achievements Found";
                definitionsDescription.gameObject.SetActive(true);
            }

            RefreshDisplayingDefinition();
        }

        public void OnShowDefinitionChanged(bool value)
        {
            displayDefinition = value;

            if (displayIndex >= 0)
            {
                OnDefinitionIdButtonClicked(displayIndex);
            }
        }
        public void RefreshDisplayingDefinition()
        {
            if (displayIndex == -1)
            {
                return;
            }
            OnDefinitionIdButtonClicked(displayIndex);
        }

        public void OnDefinitionIdButtonClicked(int i)
        {
            if (i > AchievementsService.GetAchievementsCount())
            {
                return;
            }

            displayIndex = i;

            var achievementData = achievementDataList[i];
            var definition = achievementData.Definition;
            achievementUnlockedIcon.texture = AchievementsService.Instance.GetAchievementUnlockedIconTexture(definition.AchievementId);
            achievementLockedIcon.texture = AchievementsService.Instance.GetAchievementLockedIconTexture(definition.AchievementId);

            unlockAchievementButton.gameObject.SetActive(true);

            if (displayDefinition)
            {
                DisplayAchievementDefinition(definition);
            }
            else
            {
                DisplayPlayerAchievement(definition);
            }

            unlockAchievementButton.interactable = achievementData.PlayerData.HasValue && achievementData.PlayerData?.Progress < 1;
        }

        //Show player-specific achievement data
        void DisplayPlayerAchievement(DefinitionV2 definition)
        {
            PlayerAchievement? achievementNullable = null;
            foreach (var ach in AchievementsService.Instance.CachedPlayerAchievements(EOSManager.Instance.GetProductUserId()))
            {
                if (ach.AchievementId == definition.AchievementId)
                {
                    achievementNullable = ach;
                    break;
                }
            }

            if (achievementNullable == null)
            {
                definitionsDescription.text = "Player achievement info not found.";
                definitionsDescription.gameObject.SetActive(true);
                achievementUnlockedIcon.gameObject.SetActive(true);
                achievementLockedIcon.gameObject.SetActive(true);
                return;
            }

            PlayerAchievement achievement = achievementNullable.Value;
            string selectedDescription = string.Format(
                "Id: {0}\nDisplay Name: {1}\nDescription: {2}\nProgress: {3}\nUnlock Time: {4}\n",
                achievement.AchievementId, achievement.DisplayName, achievement.Description, achievement.Progress, achievement.UnlockTime);

            if (achievement.StatInfo != null)
            {
                foreach (PlayerStatInfo si in achievement.StatInfo)
                {
                    selectedDescription += String.Format("Stat Info: '{0}': {1}/{2}\n", si.Name, si.CurrentValue, si.ThresholdValue);
                }
            }

            bool locked = achievement.Progress < 1.0;

            definitionsDescription.text = selectedDescription;
            definitionsDescription.gameObject.SetActive(true);
            achievementUnlockedIcon.gameObject.SetActive(!locked);
            achievementLockedIcon.gameObject.SetActive(locked);
        }

        //Show global achievement definition
        void DisplayAchievementDefinition(DefinitionV2 definition)
        {
            string selectedDescription = string.Format(
                "Id: {0}\nUnlocked Display Name: {1}\nUnlocked Description: {2}\nLocked Display Name: {3}\nLocked Description: {4}\nHidden: {5}\n",
                definition.AchievementId, definition.UnlockedDisplayName, definition.UnlockedDescription, definition.LockedDisplayName, definition.LockedDescription, definition.IsHidden);

            foreach (StatThresholds st in definition.StatThresholds)
            {
                selectedDescription += string.Format("Stat Thresholds: '{0}': {1}\n", st.Name, st.Threshold);
            }

            definitionsDescription.text = selectedDescription;
            definitionsDescription.gameObject.SetActive(true);
            achievementUnlockedIcon.gameObject.SetActive(true);
            achievementLockedIcon.gameObject.SetActive(true);
        }
    }
}