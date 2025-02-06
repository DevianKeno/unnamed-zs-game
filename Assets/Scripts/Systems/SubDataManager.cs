// using System;
// using System.Collections.Generic;
// using UnityEngine;

// namespace UZSG
// {
//     public interface ISubDataManager<T>
//     {
//         internal virtual void Register(T data) { }
//         public abstract T GetData(string id);
//         public abstract bool TryGetData(string id, out T data);
//     }

//     public class SubDataManagerBase : MonoBehaviour
//     {
//     }

//     public class SubDataManager<T> : SubDataManagerBase, IInitializeable, ISubDataManager<T> where T : BaseData
//     {
//         protected bool _isInitialized;
//         public bool IsInitialized => _isInitialized;

//         protected Dictionary<string, T> dataDict = new();

//         internal virtual void Register(T data)
//         {
//             if (dataDict.ContainsKey(data.Id))
//             {
//                 string newId = GenerateUniqueId(data.Id);
//                 var msg = $"[BaseManager]: Duplicate Id found for '{data.Id}', assigning new Id '{newId}'";
//                 Game.Console.Log(msg);
//                 Debug.LogWarning(msg);
//                 data.Id = newId;
//             }

//             dataDict[data.Id] = data;
//         }

//         public virtual T GetData(string id)
//         {
//             if (dataDict.ContainsKey(id))
//             {
//                 return dataDict[id];
//             }

//             Game.Console.Log("Invalid item id");
//             return null;
//         }

//         public virtual bool TryGetData(string id, out T data)
//         {
//             if (dataDict.ContainsKey(id))
//             {
//                 data = dataDict[id];
//                 return true;
//             }

//             Game.Console.Log("Invalid item id");
//             data = null;
//             return false;
//         }

//         string GenerateUniqueId(string baseId)
//         {
//             string[] parts = baseId.Split('_');
//             string basePart = parts[0];
//             int idx = 1;

//             if (parts.Length > 1 && int.TryParse(parts[^1], out int parsedIdx))
//             {
//                 basePart = string.Join("_", parts[..^1]);
//                 idx = parsedIdx + 1;
//             }

//             string newId;
//             do
//             {
//                 newId = $"{basePart}_{idx}";
//                 idx++;
//             } while (dataDict.ContainsKey(newId));

//             return newId;
//         }
//     }
// }