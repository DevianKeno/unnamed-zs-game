using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UZSG.Scenes
{
    public class TitleSceneHandler : MonoBehaviour
    {
        [SerializeField] Button exitButton;

        void Start()
        {
            exitButton?.onClick.AddListener(ExitGame);
        }

        void ExitGame()
        {
            Application.Quit();
        }
    }    
}