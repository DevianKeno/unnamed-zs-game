using System;
using System.IO;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;


using UZSG.Data;
using UZSG.Worlds;

namespace UZSG.UI.TitleScreen
{
    public class WorldEntryUI : UIElement, IPointerDownHandler
    {
        public WorldManifest WorldManifest;
        public string LevelDataPath => Path.Join(WorldManifest.WorldRootDirectory, "level.dat");
        public event Action<WorldEntryUI> OnClick;

        [SerializeField] TextMeshProUGUI nameTmp;
        [SerializeField] TextMeshProUGUI levelTmp;
        [SerializeField] TextMeshProUGUI dateTmp;
        [SerializeField] Image image;

        public void OnPointerDown(PointerEventData eventData)
        {
            OnClick?.Invoke(this);
        }

        public void SetManifest(WorldManifest manifest)
        {
            if (manifest == null) return;
            
            this.WorldManifest = manifest;
            nameTmp.text = manifest.WorldName;
            var levelData = Resources.Load<LevelData>($"Data/Levels/{manifest.LevelId}");
            levelTmp.text = levelData != null ? levelData.DisplayName : "<color=\"red\">INVALID LEVEL ID!";
            dateTmp.text = manifest.LastPlayedDate; 

            Sprite worldImage = LoadWorldImage(manifest.WorldName);
            if (worldImage != null)
            {
                image.color = Color.white;
                image.sprite = worldImage;
            }
        }

        Sprite LoadWorldImage(string worldName)
        {
            var imagePath = Path.Combine(Application.persistentDataPath, WorldManager.WORLDS_FOLDER, worldName, $"world{WorldManager.WORLD_SCREENSHOT_EXTENSION}");
            if (!File.Exists(imagePath))
            {
                return null;
            }
            
            var imageBytes = File.ReadAllBytes(imagePath);
            var texture = new Texture2D(2, 2);
            if (texture.LoadImage(imageBytes))
            {
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
            }
            else
            {
                return null;
            }
        }
    }
}