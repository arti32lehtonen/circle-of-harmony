using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Menu
{
    public class TwoStateButton : MonoBehaviour
    {
        public Sprite pressedOnImage;
        public Sprite pressedOffImage;
        public string pressedOnText;
        public string pressedOffText;
        public bool isPressed = true;

        public void Awake()
        {
            SetComponents();
        }

        public void SwitchState()
        {
            GetComponent<AudioSource>().Play();
            isPressed = !isPressed;
            SetComponents();
        }

        public void SetComponents()
        {
            var imageComponent = GetComponent<Image>();
            imageComponent.sprite = isPressed ? pressedOnImage : pressedOffImage;

            var textComponent = transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
            textComponent.text = isPressed ? pressedOnText : pressedOffText;
        }
    }
}