using UnityEngine;

namespace UZSG.FPP
{
    /// <summary>
    /// A light source object in a viewmodel.
    /// </summary>
    public class ViewmodelLightSource : ViewmodelSubcomponent
    {
        [SerializeField] Light lightComponent;
        public Light Light => lightComponent;
    }
}