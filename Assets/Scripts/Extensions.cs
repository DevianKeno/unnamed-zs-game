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
        
        public static void ChangeTag(this UnityEngine.GameObject gameObject, string tag, bool tagChildren = true)
        {
            if (!IsValidTag(tag))
            {
                UnityEngine.Debug.LogWarning($"Tag '{tag}' is not defined in the Tags Manager.");
                return;
            }

            gameObject.tag = tag;

            if (tagChildren)
            {
                foreach (UnityEngine.Transform child in gameObject.transform)
                {
                    child.gameObject.ChangeTag(tag, true);
                }
            }
        }


        private static bool IsValidTag(string tag)
        {
            for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.tags.Length; i++)
            {
                if (UnityEditorInternal.InternalEditorUtility.tags[i] == tag)
                {
                    return true;
                }
            }
            return false;
        }
    }
}