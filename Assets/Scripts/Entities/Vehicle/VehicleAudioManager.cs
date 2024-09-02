using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System.ComponentModel.Composition;

namespace UZSG.Entities.Vehicles
{
    public class VehicleAudioManager : MonoBehaviour
    {
        VehicleEntity _vehicle;

        public AudioClip engineIdle;
        public AudioClip engineActive;          
        public AnimationCurve pitchCurve;       // x and y should min max of 1 to desired max pitch, could closely resemble power curve idf u want to
        public bool twoSoundSystem = false;
        [Range(0.2f, 1.0f)]public float normedSpeedThreshold;      // normalized speed value (value of 0 to 1) where the idle and active will start to transition for two-sound system
        public float minPitch = 1f;             // Minimum pitch for engine to default to

        [SerializeField] AudioSource _sourceIdle;
        [SerializeField] AudioSource _sourceActive;

        Rigidbody _carRB;
        float _carRBSpeed;
        float _carSpeed;

        // Start is called before the first frame update
        void Start()
        {
            _vehicle = GetComponent<VehicleEntity>();
        }

        void Update()
        {
            _carRB = _vehicle.Controller.GetRigidbody();
            _carSpeed = _vehicle.Controller.carSpeed;
            _carRBSpeed = _carRB.velocity.magnitude;

            if (twoSoundSystem)
            {
                TwoEngineSound();
            }
            else
            {
                OneEngineSound();
            }
            
        }

        public void PlayerInVehicle()
        {
            CancelInvoke("NoPlayerInVehicle");  // should cancel invoking the function if the player jumped out of the vehicle while moving then moved in again
            StartEngineSound();
        }

        public void NoPlayerInVehicle()
        {
            if (_carSpeed < 1 && _carSpeed > -1)
            {
                _sourceIdle.Stop();
                _sourceActive.Stop();
                CancelInvoke("NoPlayerInVehicle");
            }
            else
            {
                Invoke("NoPlayerInVehicle", Time.fixedDeltaTime);
            }
        }

        void StartEngineSound()
        {
            _sourceIdle.Play();
            _sourceActive.Play();

            if (_carSpeed < 1)  // if the car was only starting
            {
                _sourceActive.volume = 0;
            }
        }

        void OneEngineSound()
        {
            if (_sourceIdle.isPlaying)
            {
                // Normalize car speed to 0-1
                float normalizedSpeed = Mathf.Clamp01(_carSpeed / _vehicle.Controller.maxSpeed);

                // Apply pitch factor to the audio source
                // Apply pitch factor only if the car is moving
                if (_carSpeed > 1)
                {
                    //_sourceIdle.pitch = pitchCurve.Evaluate(normalizedSpeed);
                    _sourceIdle.pitch = minPitch + (_carRBSpeed / 25f);
                    print($"pitch: {_sourceIdle.pitch}");
                }
                else if (_carSpeed < 1)
                {
                    _sourceIdle.pitch = minPitch;
                }
            }
        }

        void TwoEngineSound()
        {
            if (_sourceIdle.isPlaying)
            {
                // Normalize car speed to 0-1
                float normalizedSpeed = Mathf.Clamp01(_carSpeed / _vehicle.Controller.maxSpeed);

                // Apply pitch factor to the audio source
                // Apply pitch factor only if the car is moving
                if (_carSpeed > 1)
                {
                    _sourceIdle.pitch = pitchCurve.Evaluate(normalizedSpeed);

                    if (normalizedSpeed > 0.7f)
                    {
                        _sourceIdle.volume = Mathf.Clamp01(_sourceIdle.volume - (0.1f * Time.deltaTime));
                        _sourceActive.volume = Mathf.Clamp01(_sourceActive.volume + (0.1f * Time.deltaTime));
                    }
                    else
                    {
                        if (_sourceIdle.volume < 1)
                        {
                            _sourceIdle.volume = Mathf.Clamp01(_sourceIdle.volume + (0.1f * Time.deltaTime));
                        }
                        if (_sourceActive.volume > 0)
                        {
                            _sourceActive.volume = Mathf.Clamp01(_sourceActive.volume - (0.1f * Time.deltaTime));
                        }
                    }

                }
                else if (_carSpeed < 1)
                {
                    _sourceIdle.pitch = minPitch;

                    if (_sourceIdle.volume < 1)
                    {
                        _sourceIdle.volume = Mathf.Clamp01(_sourceIdle.volume + (0.1f * Time.deltaTime));
                    }
                    if (_sourceActive.volume > 0)
                    {
                        _sourceActive.volume = Mathf.Clamp01(_sourceActive.volume - (0.1f * Time.deltaTime));
                    }
                }
            }
        }
    }
}

