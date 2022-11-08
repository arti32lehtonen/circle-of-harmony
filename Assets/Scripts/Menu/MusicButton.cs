using DefaultNamespace;
using Unity.VisualScripting;
using UnityEngine;
using Utils;

namespace Menu
{
    public class MusicButton : TwoStateButton
    {
        public void Awake()
        {
            base.isPressed = !GameGlobalState.instance.mainTheme.mute;
            base.SetComponents();
        }

        public void SwitchMusicState()
        {
            base.SwitchState();
            GameGlobalState.instance.mainTheme.mute = !base.isPressed;
        }
    }
}