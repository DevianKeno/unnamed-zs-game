using UnityEngine;
using UnityEngine.UI;



namespace UZSG.Scenes
{
    public class TitleSceneHandler : MonoBehaviour
    {
        [SerializeField] Button settingsButton;
        [SerializeField] Button exitButton;

        void Start()
        {
            settingsButton.onClick.AddListener(OpenSettings);
            exitButton.onClick.AddListener(ExitGame);
        }

        void OpenSettings()
        {
            Game.Settings.ShowGlobalInterface();
        }

        void ExitGame()
        {
            Application.Quit();
        }
    }    
}