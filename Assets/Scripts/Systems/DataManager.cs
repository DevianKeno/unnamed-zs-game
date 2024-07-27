// using System;
// using System.Collections.Generic;

// using UnityEngine;

// using UZSG.Items;
// using UZSG.Crafting;

/// THIS IS TOO MUCH CUMBERSOME
// namespace UZSG.Systems
// {
//     public class DataManager : MonoBehaviour, IInitializeable
//     {
//         public static DataManager Current { get; private set; }

//         bool _isInitialized;
//         public bool IsInitialized => _isInitialized;

//         Dictionary<Type, SubDataManagerBase> _dataManagers = new()
//         {
//             {typeof(ItemData), itemManager},
//         };
        
//         static AttributesManager attrManager;
//         public static AttributesManager Attributes { get => attrManager; }
//         static ItemManager itemManager;
//         public static ItemManager Items { get => itemManager; }
//         static EntityManager entityManager;
//         public static EntityManager Entity { get => entityManager; }
//         static RecipeManager recipeManager;
//         public static RecipeManager Recipes { get => recipeManager; }

//         internal void Initialize()
//         {
//             if (_isInitialized) return;
//             _isInitialized = true;
            
//             Current = this;

//             var startTime = Time.time;
//             Game.Console.LogDebug("Initializing database...");
//             ReadAllData();
//         }

//         void ReadAllData()
//         {
//             foreach (var data in Resources.LoadAll<BaseData>("Data"))
//             {
//                 _dataManagers[typeof(ItemData)].Register(data);
//             }
//         }

//         public T GetData<T>(string id) where T : BaseData
//         {
//             if (_dataManagers.ContainsKey(typeof(T)))
//             {
//                 return _dataManagers[typeof(T)].GetData
//             };

//             return default;
//         }
//     }
// }
