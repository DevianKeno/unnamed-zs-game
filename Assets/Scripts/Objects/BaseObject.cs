using UnityEngine;
using UZSG.Attributes;
using UZSG.Data;
using UZSG.Systems;

namespace UZSG.Objects
{
    public abstract class BaseObject : MonoBehaviour, IAttributable
    {
        [SerializeField] protected ObjectData objectData;
        public ObjectData ObjectData => objectData;
        [SerializeField] AttributeCollection attributes;
        public AttributeCollection Attributes => attributes;
        public bool HasAudio = false;
        [SerializeField] protected AudioSourceController audioController;
        public AudioSourceController Audio => audioController;

        protected virtual void Start()
        {
            InitializeAttributes();
            if (HasAudio) InitializeAudioController();
        }

        protected virtual void InitializeAttributes()
        {
            attributes = new();
            // attributes.InitializeFromData();
        }
        
        protected virtual void InitializeAudioController()
        {
            audioController.LoadAudioAssetsData(objectData.AudioAssetsData);
        }
    }
}