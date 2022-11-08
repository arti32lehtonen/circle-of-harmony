using DefaultNamespace;
using Unity.VisualScripting;
using UnityEngine;

namespace Menu
{
    public class StartMainGameButton : StartSceneButton
    {
        public TutorialButton tutorialState;

        public void LoadMainGame()
        {
            base.LoadLevel();
            // if (!tutorialState.isPressed)
            // {
            //     var tutorialObject = FindObjectOfType(typeof(TutorialManager));
            //     tutorialObject.GetComponent<TutorialManager>().SkipTutorial();
            // }
        }

    }
}