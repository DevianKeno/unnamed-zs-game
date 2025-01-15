using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UZSG.Saves;

namespace UZSG.UI.TitleScreen
{
    public class WorldEntryUI : UIElement, IPointerDownHandler
    {
        public WorldSaveData SaveData { get; set; }
        public event Action<WorldEntryUI> OnClick;
        [SerializeField] TextMeshProUGUI nameTmp;
        [SerializeField] TextMeshProUGUI levelTmp;
        [SerializeField] TextMeshProUGUI dateTmp;
        [SerializeField] Image image;

        public void OnPointerDown(PointerEventData eventData)
        {
            OnClick?.Invoke(this);
        }

        public void SetData(WorldSaveData saveData)
        {
            if (saveData == null) return;
            
            SaveData = saveData;
            nameTmp.text = saveData.WorldName;
            levelTmp.text = saveData.LevelId;
            dateTmp.text = saveData.LastPlayedDate.ToShortDateString();
        }
    }
}