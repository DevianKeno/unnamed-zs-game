using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UZSG.Systems;

namespace UZSG.Player
{
    public class PlayerBobbing : MonoBehaviour
    {
        bool IsEnabled = true;
        [SerializeField][Range(0, 0.1f)]
        float _amplitude = 0.0015f;
        [SerializeField][Range(0, 30f)]
        float _frequency = 10f;
        [SerializeField] float _minMoveSpeed = 3f;
        [SerializeField] Transform _cam;
        [SerializeField] Transform _camHolder;
        Vector3 _startPos;
        CharacterController _controller;

        void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _startPos = _cam.localPosition;
        }

        void Start()
        {
            Game.Tick.OnTick += Tick;
        }

        void Tick(object sender, TickEventArgs e)
        {
            if (!IsEnabled) return;

            CheckMotion();
            ResetPosition();
        }

        Vector3 FootstepMotion()
        {
            Vector3 pos = Vector3.zero;
            pos.y += Mathf.Sin(Time.time * _frequency) * _amplitude;
            pos.x += Mathf.Cos(Time.time * _frequency / 2) * _amplitude * 2;
            return pos;
        }

        void CheckMotion()
        {
            float speed = new Vector3(_controller.velocity.x, 0, _controller.velocity.z).magnitude;
            if (speed < _minMoveSpeed) return;
            if (!_controller.isGrounded) return;

            _cam.localPosition += FootstepMotion();
        }

        void ResetPosition()
        {
            if (_cam.localPosition == _startPos) return;
            _cam.localPosition = Vector3.Lerp(_cam.localPosition, _startPos, 1 * Time.deltaTime);
        }
    }
}   
