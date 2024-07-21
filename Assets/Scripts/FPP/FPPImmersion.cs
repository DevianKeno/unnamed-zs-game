using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UZSG.Players;
using UZSG.Systems;

namespace UZSG.FPP
{
    public class FPPImmersion : MonoBehaviour
    {
        [SerializeField] PlayerControls controls;

        [Header("Configuration")]
        public bool _enable = false;
        public float _toggleSpeed = 0.0f;
        [SerializeField, Range(0, 0.1f)] float _Amplitude = 0.015f;
        [SerializeField, Range(0, 30)] float _frequency = 10.0f;
        
        Vector3 _startPos;
        CharacterController _controller;

        [SerializeField] Transform _camera;
        [SerializeField] Transform _cameraHolder;
        
        void Start()
        {
            Game.Tick.OnTick += Tick;
        }

        void Awake()
        {
            controls = GetComponent<PlayerControls>();
            _controller = GetComponent<CharacterController>();
            _startPos = _camera.localPosition;
        }

        void Tick(object sender, TickEventArgs e)
        {
            if (!_enable) return;
            CheckMotion();
            ResetPosition();
            _camera.LookAt(FocusTarget());
        }

        Vector3 FootStepMotion()
        {
            Vector3 pos = Vector3.zero;
            pos.y += Mathf.Sin(Time.time * _frequency) * _Amplitude;
            pos.x += Mathf.Cos(Time.time * _frequency / 2) * _Amplitude * 2;
            return pos;
        }

        void CheckMotion()
        {
            float speed = controls.Magnitude;
            if (speed < _toggleSpeed) return;
            if (!_controller.isGrounded) return;

            PlayMotion(FootStepMotion());
        }

        void PlayMotion(Vector3 motion)
        {
            _camera.localPosition += motion;
        }

        Vector3 FocusTarget()
        {
            Vector3 pos = new(transform.position.x, transform.position.y + _cameraHolder.localPosition.y, transform.position.z);
            pos += _cameraHolder.forward * 15.0f;
            return pos;
        }

        void ResetPosition()
        {
            if (_camera.localPosition == _startPos) return;
            _camera.localPosition = Vector3.Lerp(_camera.localPosition, _startPos, 1 * Time.deltaTime);
        }
    }
}

