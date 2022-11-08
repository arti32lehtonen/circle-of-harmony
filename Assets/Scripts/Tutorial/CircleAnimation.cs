using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class CircleAnimation : MonoBehaviour
    {
        public Image fillImage;
        private float _interval = 0.01f;
        private float _fillAmount = 0f;
        
        void Update()
        {
            _fillAmount = Mathf.Min(1, _fillAmount + _interval);
            fillImage.fillAmount = _fillAmount;

            if (_fillAmount >= 1)
            {
                _fillAmount = 0;
            }
        }

    }
}