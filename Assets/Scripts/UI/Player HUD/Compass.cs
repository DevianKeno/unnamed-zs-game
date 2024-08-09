 using UnityEngine;
using UnityEngine.UI;
using UZSG.Entities;

namespace UZSG.UI.HUD
{
    public class Compass : MonoBehaviour
    {
        public Player Player;
        public RawImage compassImage;
        public bool enableShake;
        public float shakeDuration = 0.5f; // Duration of the shake
        public float shakeMagnitude = 0.01f; // Maximum shake amount
        

        Transform _posPlayer;
        Vector2 _previousUVRect;
        bool _isShaking = false;
        float _shakeTimer = 0f;

        // Start is called before the first frame update
        void Start()
        {
            _posPlayer = Player.FPP.CameraController.transform;
        }

        // Update is called once per frame
        void Update()
        {
            if(enableShake)
            {
                float _compassTravel = _posPlayer.localEulerAngles.y / 360f;
                Vector2 _newUVRect = new Rect(_compassTravel, 0, 1f, 1f).min;

                if (_newUVRect != _previousUVRect && Vector2.Distance(_newUVRect, _previousUVRect) > 0.05)
                {
                    _isShaking = false;
                    _shakeTimer = 0f;
                }

                _previousUVRect = _newUVRect;

                if (!_isShaking)
                {

                    float shakeFactor = Mathf.Clamp01(_shakeTimer / shakeDuration);
                    float randomOffset = Random.Range(-shakeMagnitude * (1 - shakeFactor), shakeMagnitude * (1 - shakeFactor));
                    compassImage.uvRect = new Rect(_compassTravel + randomOffset, 0, 1f, 1f);
                    _shakeTimer += Time.deltaTime;
                    if (_shakeTimer >= shakeDuration)
                    {
                        _isShaking = false;
                    }
                }
                else
                {
                    compassImage.uvRect = new Rect(_compassTravel, 0, 1f, 1f);
                    if (Vector2.Distance(_newUVRect, _previousUVRect) == 0)
                    {
                        _isShaking = true;
                        _shakeTimer = 0f;
                    }
                }
            }
            else
            {
                compassImage.uvRect = new Rect(_posPlayer.localEulerAngles.y / 360f, 0, 1f, 1f);
            }

            
        }
    }
}