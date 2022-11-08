using DefaultNamespace;
using UnityEngine;

namespace Menu
{
    public class TutorialButton : TwoStateButton
    {
        public void Awake()
        {
            base.isPressed = GameGlobalState.instance.tutorialState;
            base.SetComponents();
        }
        
        public void SwitchTutorialState()
        {
            base.SwitchState();
            GameGlobalState.instance.tutorialState = base.isPressed;
        }
    }
}