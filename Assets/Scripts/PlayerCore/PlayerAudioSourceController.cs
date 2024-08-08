using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UZSG.Entities;
using UZSG.Systems;

namespace UZSG.Players
{
    public class PlayerAudioSourceController : AudioSourceController
    {
        public Player Player;
        public GroundChecker Ground;

        string _texture = "none";
        float _cooldown = 0f;
        float _timer = 0f;
        int _clipIndex;

        private void Start()
        {
            StartCoroutine(PlayFootstepSound());
        }

        private IEnumerator PlayFootstepSound()
        {
            while (true)
            {
                if (IsMoving && Player.Controls.IsGrounded)
                {    

                    if (Player.Controls.IsRunning)
                    {
                        _cooldown = 0.250f;
                    }
                    else if (Player.Controls.IsCrouching)
                    {
                        _cooldown = 0.750f;
                    }
                    else
                    {
                        _cooldown = 0.5f;
                    }

                    // half working solution to Ground getting dereferenced half of the time
                    _texture = Ground?.RetrieveTexture();

                    _timer += Time.deltaTime;
                    if (_timer >= _cooldown)
                    {
                        _clipIndex = UnityEngine.Random.Range(0, 7);

                        // add more if for more textures, idk what are the better implementation
                        if (_texture == "grass")
                        {
                            PlaySound($"grass_walk_{_clipIndex}");
                        }
                        else if(_texture == "dirt")
                        {
                            PlaySound($"dirt_walk_{_clipIndex}");
                        }
                        else
                        {
                            
                            PlaySound($"grass_walk_{_clipIndex}");
                        }
                        
                        _timer = 0f;
                    }
                }
                else
                {
                    _timer = 0f;
                }

                yield return null;
            }
            
        }

        bool IsMoving
        {
            get
            {
                if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
                    return true;
                else
                    return false;
            }
        }
    }
}