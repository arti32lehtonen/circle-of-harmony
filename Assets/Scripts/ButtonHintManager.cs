using TMPro;
using UnityEngine;

namespace DefaultNamespace
{
    public class ButtonHintManager : MonoBehaviour
    {
        public GameObject attachedObject;
        public TextMeshProUGUI hintTitle;
        public TextMeshProUGUI hintText;

        public void SetActive(bool isActive, SpeciesConfig config)
        {
            attachedObject.SetActive(isActive);
            if (isActive)
            {
                hintTitle.text = config.displayedName;
                hintText.text = config.hint;
            }
        }
    }
}