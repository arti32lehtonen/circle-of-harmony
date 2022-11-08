using DefaultNamespace;
using TMPro;
using UnityEngine;

namespace Menu
{
    public class MainSceneExit : MonoBehaviour
    {
        public GameObject pauseCanvas;
        public GameObject exitMenu;
        public GameObject gameOverMenu;
        
        public void SwitchPauseCanvasState(bool state)
        {
            GetComponent<AudioSource>().Play();
            pauseCanvas.SetActive(state);
            exitMenu.SetActive(true);
            gameOverMenu.SetActive(false);
        }
        
        public void TurnOnGameOver(int score)
        {
            var textComponent = gameOverMenu.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
            textComponent.text = $"Game  Over!\nYour  score  is  {score}";
            
            pauseCanvas.SetActive(true);
            exitMenu.SetActive(false);
            gameOverMenu.SetActive(true);
        }

        
    }
}