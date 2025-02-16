using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Serialization;

using UZSG.Data;

namespace UZSG.FPP
{
    /// <summary>
    /// Reference holder for the various Components of a Viewmodel.
    /// </summary>
    public class ViewmodelComponent : MonoBehaviour
    {
        public ItemData ItemData { get; set; }
        public IViewmodelItem ViewmodelData => ItemData as IViewmodelItem;
        
        [SerializeField, FormerlySerializedAs("modelAnimator")] Animator viewmodelAnimator;
        /// <summary>
        /// Animator component for the viewmodel animations.
        /// </summary>
        public Animator ViewmodelAnimator => viewmodelAnimator;
        
        [SerializeField] Animator cameraAnimator;
        /// <summary>
        /// Animator component for first-person camera animations.
        /// </summary>
        public Animator CameraAnimator => cameraAnimator;

        [SerializeField] FPPCameraAnimationSource cameraAnimationSource;
        public FPPCameraAnimationSource CameraAnimationSource => cameraAnimationSource;

        [SerializeField] List<ViewmodelSubcomponent> subcomponents;
        public List<ViewmodelSubcomponent> Subcomponents => subcomponents;

        void Awake()
        {
            GetSubcomponents();
        }

        [ContextMenu("Get Subcomponents")]
        public void GetSubcomponents()
        {
            if (subcomponents == null || subcomponents.Count == 0)
            {
                subcomponents = GetComponentsInChildren<ViewmodelSubcomponent>().ToList();
            }
        }

        public bool TryGetSubcomponent<T>(out T type) where T : ViewmodelSubcomponent
        {
            type = (T) subcomponents.Find(s => s is T);
            return type != null;
        }
    }
}