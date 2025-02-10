using UnityEngine;

using UZSG.Objects;
using UZSG.Saves;

namespace UZSG
{
    /// <summary>
    /// Helper class for saving. lmao wtf am I doing but this kinda feels right.
    /// I really wanted to implement that covariance
    /// </summary>
    public class SavesManager : MonoBehaviour
    {
        /// Universal ? lol
        public void Read<T>(T readable, SaveData saveData)
        {
            switch (readable)
            {
                case BaseObject baseObject:
                {
                    ReadAsBaseObject(baseObject, saveData);
                    break;
                }
            }
        }
        

        #region BaseObject Read

        /// I don't know what I was thinking when making this but, this gut feeling+
        public void ReadAsBaseObject(BaseObject baseObject, SaveData saveData)
        {
            switch (saveData)
            {
                case StorageObjectSaveData storageObjectSave 
                    when baseObject is ISaveDataReadable<StorageObjectSaveData> storageObjectReader:
                {
                    storageObjectReader.ReadSaveData(storageObjectSave);
                    break;
                }
                case BaseObjectSaveData baseObjectSave
                    when baseObject is ISaveDataReadable<BaseObjectSaveData> baseObjectReader:
                {
                    baseObjectReader.ReadSaveData(baseObjectSave);
                    break;
                }
                default:
                {
                    break;
                }
            }
        }

        #endregion


        #region BaseObject Write

        public BaseObjectSaveData WriteAsBaseObject(BaseObject baseObject)
        {
            /// I don't know what I was thinking when making this but, this gut feeling
            switch (baseObject)
            {
                case ISaveDataWriteable<StorageObjectSaveData> storageObjectWriter:
                {
                    return storageObjectWriter.WriteSaveData();
                }
                case ISaveDataWriteable<BaseObjectSaveData> baseObjectWriter:
                {
                    return baseObjectWriter.WriteSaveData();
                }
                default:
                {
                    return default;
                }
            }
        }
        
        #endregion
    }
}