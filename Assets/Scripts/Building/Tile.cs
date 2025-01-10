using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UZSG.Attributes;
using UZSG.Data;

namespace UZSG.Building
{
    public class Tile : MonoBehaviour
    {
        [SerializeField] int instanceId;
        public int InstanceId => instanceId;
        [Space]
        
        [SerializeField] protected ObjectData tileData;
        public ObjectData TileData => tileData;
        /// <summary>
        /// Shorthand to get EntityData Id.
        /// </summary>
        public string Id => tileData.Id;

        [SerializeField] protected AttributeCollection attributes;
        public AttributeCollection Attributes => attributes;

        /// <summary>
        /// The transform position of this Structure. 
        /// </summary>
        public Vector3 Position
        {
            get { return transform.position; }
            set { transform.position = value; }
        }
        /// <summary>
        /// The transform rotation of this Structure. 
        /// </summary>
        public Quaternion Rotation
        {
            get { return transform.rotation; }
            set { transform.rotation = value; }
        }

        [SerializeField] GameObject model;

        public void Place()
        {
            gameObject.isStatic = true;
        }
    }
}