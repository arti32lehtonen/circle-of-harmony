using System;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class HeartTutorialAnimation : MonoBehaviour
    {
        public Image fillImage;
        private bool _isFilled = true;
        private float _nextUpdateTime;
        private float _interval = 0.4f;

        private void Awake()
        {
            _nextUpdateTime = _interval;
        }

        void Update()
        {
            // If the next update is reached
            if (Time.time >= _nextUpdateTime) {
                if (_isFilled)
                {
                    fillImage.fillAmount = 0.666666f;
                    _isFilled = false;
                }
                else
                {
                    fillImage.fillAmount = 1f;
                    _isFilled = true;
                }
                _nextUpdateTime = Time.time + _interval;
            }
        }
    }
}