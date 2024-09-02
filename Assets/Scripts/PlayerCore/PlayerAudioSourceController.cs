using UnityEngine;

using UZSG.Entities;
using UZSG.Systems;

namespace UZSG.Players
{
    public class PlayerAudioSourceController : AudioSourceController
    {
        public Player Player;

        string _texture = "none";
        float _cooldown = 0f;
        float _timer = 0f;
        int _clipIndex;

        private void Update()
        {
            PlayFootsteps();
        }

        void PlayFootsteps()
        {
            if(IsMoving && Player.Controls.IsGrounded && Player.Controls.Velocity.magnitude != 0f)
            {

                // 2f is arbitrary, a number kinda feels right based on player magnitude
                _cooldown = 2f / Player.Controls.Velocity.magnitude;

                // LMFAO IT'S FIXED NOW, INSANE
                _texture = Player.Controls.groundChecker.texture;

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
                    else if (_texture == "Sound_007_basecolor")
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