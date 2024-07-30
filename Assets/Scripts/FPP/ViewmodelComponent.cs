using UnityEngine;

namespace UZSG.FPP
{
    public class ViewmodelComponent : MonoBehaviour
    {
        [SerializeField] Animator modelAnimator;
        public Animator ModelAnimator => modelAnimator;
        [SerializeField] Animator cameraAnimator;
        public Animator CameraAnimator => cameraAnimator;
        [SerializeField] FPPCameraAnimationSource cameraAnimationSource;
        public FPPCameraAnimationSource CameraAnimationSource => cameraAnimationSource;
    }
}