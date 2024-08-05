using UnityEngine;
using UnityEngine.UI;

using UZSG.Entities;

namespace UZSG.UI.HUD
{
    public class Compass : MonoBehaviour
    {
        public Player Player;
        public RawImage compassImage;
        private Transform _posPlayer;

        // Start is called before the first frame update
        void Start()
        {
            _posPlayer = Player.FPP.CameraController.transform;
        }

        // Update is called once per frame
        void Update()
        {
            compassImage.uvRect = new Rect(_posPlayer.localEulerAngles.y / 360f, 0, 1f, 1f);
        }
    }
}