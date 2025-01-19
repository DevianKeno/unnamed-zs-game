namespace UZSG
{
    public static class Vector3Ext
    {
        public static UnityEngine.Vector3 FromValue(float value)
        {
            return new UnityEngine.Vector3(value, value, value);
        }
        
        public static UnityEngine.Vector3 FromNumerics(System.Numerics.Vector3 value)
        {
            return new UnityEngine.Vector3(value.X, value.Y, value.Z);
        }
    }

    public static class Extensions
    {
        public static bool IsValidIndex<T>(this System.Collections.Generic.List<T> list, int index)
        {
            return !(index < 0 || index >= list.Count);
        }

        public static bool IsSet(this UnityEngine.AddressableAssets.AssetReference assetReference)
        {
            return assetReference != null
                && assetReference.RuntimeKeyIsValid()
                && !string.IsNullOrEmpty(assetReference.AssetGUID);
        }
        
        public static bool Includes(this UnityEngine.LayerMask layerMask, int layer)
        {
            return (layerMask.value & (1 << layer)) != 0;
        }
    }
}