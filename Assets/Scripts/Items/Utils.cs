namespace UZSG
{
    public static class Utils
    {
        public static bool IsAssetReferenceSet(UnityEngine.AddressableAssets.AssetReference assetReference)
        {
            return assetReference != null
                && assetReference.RuntimeKeyIsValid()
                && !string.IsNullOrEmpty(assetReference.AssetGUID);
        }
    }
}