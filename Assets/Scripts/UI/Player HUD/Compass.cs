using UnityEngine;
using UnityEngine.UI;

using UZSG.Entities;

namespace UZSG.UI.HUD
{
    public enum CompassAnimationType {
        NoShake,
        NormalShake,
        DampenedShake,
        SpringBasedShake
    }

    public class Compass : MonoBehaviour
    {
        public Player Player;
        [Space]

        public bool Enabled;

        [Header("Animated Compass")]
        public CompassAnimationType ShakeType = new();

        [Header("Normal Shake variables")]
        public float shakeDuration = 0.5f; // Duration of the shake
        public float shakeMagnitude = 0.01f; // Maximum shake amount

        Vector2 _previousUVRect;
        bool _isShaking = false;
        float _shakeTimer = 0f;

        [Header("Dampened Shake variables")]
        public float _rotationSpeed = 10f;
        public float _dampening = 0.9f; // Adjust dampening factor

        float _targetAngle;
        float _velocity;

        [Header("Spring Based Shake variables")]
        public float _springConstant = 5f; // Adjust spring stiffness
        public float _damping = 5f; // Adjust damping to control oscillation

        float _targetAngle2;
        float _velocity2;
        float _currentAngle;
        Transform FPPCamera;

        [SerializeField] RawImage compassImage;

        internal void Initialize(Player player)
        {
            Player = player;
            FPPCamera = player.FPP.Camera.transform;
            Enabled = true;
        }

        void Update()
        {
            if (!Enabled) return;
            
            compassImage.uvRect = new Rect(FPPCamera.transform.localEulerAngles.y / 360f, 0, 1f, 1f);
            
            if (ShakeType == CompassAnimationType.NormalShake)
            {
                Shake1();
            }
            else if (ShakeType == CompassAnimationType.DampenedShake)
            {
                Shake2();
            }
            else if (ShakeType == CompassAnimationType.SpringBasedShake)
            {
                Shake3();
            }
            else
            {
                NoShake();
            }
        }

        void Shake1()
        {
            float _compassTravel = FPPCamera.localEulerAngles.y / 360f;
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
        
        void Shake2()
        {
            _targetAngle = FPPCamera.localEulerAngles.y / 360f;
            float currentAngle = compassImage.uvRect.x;
            float deltaAngle = _targetAngle - currentAngle;
            _velocity = Mathf.Lerp(_velocity, deltaAngle * _rotationSpeed, Time.deltaTime);
            _velocity *= _dampening;
            currentAngle += _velocity * Time.deltaTime;
            compassImage.uvRect = new Rect(currentAngle, 0, 1f, 1f);
        }

        void Shake3()
        {
            _targetAngle2 = FPPCamera.localEulerAngles.y / 360f;
            float displacement = _targetAngle2 - _currentAngle;
            float acceleration = _springConstant * displacement - _damping * _velocity2;
            _velocity2 += acceleration * Time.deltaTime;
            _currentAngle += _velocity2 * Time.deltaTime;
            compassImage.uvRect = new Rect(_currentAngle, 0, 1f, 1f);
        }

        void NoShake()
        {
            compassImage.uvRect = new Rect(FPPCamera.localEulerAngles.y / 360f, 0, 1f, 1f);
        }
    }
}