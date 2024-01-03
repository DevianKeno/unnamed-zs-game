using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UZSG.Player;
using UZSG.Systems;

namespace UZSG.FPP
{
    public class FPPImmersion : MonoBehaviour
    {
        [SerializeField] PlayerControls controls;
         [Header("Configuration")]
        [SerializeField] private bool _enable = false;
        [SerializeField, Range(0, 0.1f)] private float _Amplitude = 0.015f;
        [SerializeField, Range(0, 30)] private float _frequency = 10.0f;
        
        [SerializeField] private Transform _camera = null;
        [SerializeField] private Transform _cameraHolder = null;

        public float _toggleSpeed = 0.0f;
        private Vector3 _startPos;
        private CharacterController _controller;
        void Start()
        {
            Game.Tick.OnTick += Tick;
        }
        private void Awake()
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
        private Vector3 FootStepMotion()
        {
            Vector3 pos = Vector3.zero;
            pos.y += Mathf.Sin(Time.time * _frequency) * _Amplitude;
            pos.x += Mathf.Cos(Time.time * _frequency / 2) * _Amplitude * 2;
            return pos;
        }
        private void CheckMotion()
        {
            float speed = controls.Magnitude;
            if (speed < _toggleSpeed) return;
            if (!_controller.isGrounded) return;

            PlayMotion(FootStepMotion());
        }
        private void PlayMotion(Vector3 motion)
        {
            _camera.localPosition += motion;
        }

        private Vector3 FocusTarget()
        {
            Vector3 pos = new(transform.position.x, transform.position.y + _cameraHolder.localPosition.y, transform.position.z);
            pos += _cameraHolder.forward * 15.0f;
            return pos;
        }
        private void ResetPosition()
        {
            if (_camera.localPosition == _startPos) return;
            _camera.localPosition = Vector3.Lerp(_camera.localPosition, _startPos, 1 * Time.deltaTime);
        }
    }
}

