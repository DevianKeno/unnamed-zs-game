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

    public static class MECExt
    {
        public static float WaitUntilDone(System.Collections.Generic.IEnumerator<float> coroutine)
        {
            return MEC.Timing.WaitUntilDone(MEC.Timing.RunCoroutine(coroutine));
        }
    }

    public static class Extensions
    {
        /// <summary>
        /// Converts an enum name to a more readable format (e.g., "VeryHigh" -> "Very High").
        /// </summary>
        /// <param name="enum">Enum value</param>
        /// <returns>Formatted string</returns>
        public static string ToReadable(this System.Enum @enum)
        {
            string formatted =  @enum.ToString().Replace("_", " ");
            return System.Text.RegularExpressions.Regex.Replace(formatted, @"\b[a-z]", match => match.Value.ToUpper());
        }

        public static bool IsValidIndex<T>(this System.Collections.Generic.List<T> list, int index)
        {
            return !(index < 0 || index >= list.Count);
        }

        public static bool IsValidIndex(this System.Array array, int index)
        {
            return !(index < 0 || index >= array.Length);
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

        public static System.Threading.Tasks.Task ToTask(this Unity.Jobs.JobHandle jobHandle)
        {
            return System.Threading.Tasks.Task.Run(() => jobHandle.Complete());
        }

        public static UnityEngine.Color SetAlpha(this UnityEngine.Color color, float a)
        {
            return new UnityEngine.Color(color.r, color.g, color.b, a);
        }
    }
}