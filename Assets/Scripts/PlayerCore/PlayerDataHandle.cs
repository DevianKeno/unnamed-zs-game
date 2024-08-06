using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UZSG.Attributes;
using UZSG.Data;
using UZSG.Entities;


namespace UZSG.Players
{
    public class PlayerDataHandle
    {
        public Player Player; // Ingame player data
        string _jsonPlayerFile = "Assets/Scripts/Data/PlayerData.json";   // File directory of player data
        string _jsonData; // the data being loaded and save into a json file
        PlayerData _playerDataToJson; // Player data to be save and loaded from JSON file
       
        public void LoadPlayerJson()
        {
            // Read the JSON file
            _jsonData = File.ReadAllText(Application.dataPath + _jsonPlayerFile);

            // Convert into PlayerData
            _playerDataToJson = JsonUtility.FromJson<PlayerData>(_jsonData);

            // Load the data to the player
            Player.PlayerData.Inventory = _playerDataToJson.Inventory;
            Player.PlayerData.Vitals = _playerDataToJson.Vitals;
            Player.PlayerData.Generic = _playerDataToJson.Generic;
        }

        public void SavePlayerJson()
        {
            // store player data
            _playerDataToJson.Inventory = Player.PlayerData.Inventory;
            _playerDataToJson.Vitals = Player.PlayerData.Vitals;
            _playerDataToJson.Generic =  Player.PlayerData.Generic;

            // Convert to json
            _jsonData = JsonUtility.ToJson(_playerDataToJson);

            // Write into the filepath the player data
            File.WriteAllText(Application.dataPath + _jsonPlayerFile, _jsonData);
        }
    }


}