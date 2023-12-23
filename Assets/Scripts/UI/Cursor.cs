using UnityEngine;

namespace UZSG.UI
{
    public class Cursor : MonoBehaviour
    {
        public static void Show()
        {
            UnityEngine.Cursor.visible = true;
            UnityEngine.Cursor.lockState = CursorLockMode.None;
        }

        public static void Hide()
        {
            UnityEngine.Cursor.visible = false;
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        }
    }
}