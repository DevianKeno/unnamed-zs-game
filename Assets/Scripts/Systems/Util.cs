using Epic.OnlineServices;
using UZSG.EOS;
using UZSG.EOS.Lobbies;


namespace UZSG
{
    public static class Utils
    {
        public static bool IsSameVersion(string version)
        {
            return Game.Main.GetVersionString() == version;
        }
        
        /// <summary>
        /// Convert UnityEngine Quaternion to System.Numerics Quaternion.
        /// </summary>
        public static System.Numerics.Quaternion FromUnityQuat(UnityEngine.Quaternion quaternion)
        {
            return new System.Numerics.Quaternion(
                x: quaternion.x,
                y: quaternion.y,
                z: quaternion.z,
                w: quaternion.w
            );
        }

        /// <summary>
        /// Convert System.Numerics Quaternion to UnityEngine Quaternion.
        /// </summary>
        public static UnityEngine.Quaternion FromNumericQuat(System.Numerics.Quaternion quaternion)
        {
            return new UnityEngine.Quaternion(
                x: quaternion.X,
                y: quaternion.Y,
                z: quaternion.Z,
                w: quaternion.W
            );
        }

        /// <summary>
        /// Convert UnityEngine Vector3 to System.Numerics Vector3.
        /// </summary>
        public static System.Numerics.Vector3 FromUnityVec3(UnityEngine.Vector3 vector)
        {
            return new System.Numerics.Vector3(
                x: vector.x,
                y: vector.y,
                z: vector.z
                );
        }
        
        /// <summary>
        /// Convert UnityEngine Vector3 to float array size 3.
        /// </summary>
        public static float[] ToFloatArray(UnityEngine.Vector3 vector)
        {
            return new float[] { vector.x, vector.y, vector.z };
        }

        /// <summary>
        /// Convert float array size 3 to UnityEngine Vector3. 
        /// </summary>
        public static UnityEngine.Vector3 FromFloatArray(float[] values)
        {
            return new UnityEngine.Vector3(
                x: values[0],
                y: values[1],
                z: values[2]
            );
        }

        /// <summary>
        /// Convert System.Numerics Vector3 to UnityEngine Vector3.
        /// </summary>
        public static UnityEngine.Vector3 FromNumericVec3(System.Numerics.Vector3 vector)
        {
            return new UnityEngine.Vector3(
                x: vector.X,
                y: vector.Y,
                z: vector.Z
            );
        }
    }
}