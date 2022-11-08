using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace DefaultNamespace
{
    public class HitPointsDisplayer : MonoBehaviour
    {
        private int _maxHitPoints;
        private bool _animationIsRunning = false;
        
        private Specimen _attachedSpecimen;
        private GameObject _attachedObject;
        private bool _isSilentMode = false;
        private IEnumerator _heartAnimationCoroutine;
        
        private Color _standardColor;
        private Color _animationColor;
        private float _delay;
        
        private Image GetHeartImage()
        {
            return _attachedObject.transform.Find("Heart").gameObject.GetComponent<Image>();
        }

        public void Initialize(GameObject attachedObject, Specimen specimen, GlobalSettingsConfig config)
        {
            _maxHitPoints = specimen.hitPoints;

            _attachedObject = attachedObject;
            _attachedSpecimen = specimen;

            _animationColor = config.hitPointsUIAnimationColor;
            _delay = config.hitPointsUIAnimationDelay;
            
            SetNewValue(_maxHitPoints);
        }

        public void SetSilentMode(bool isSilent)
        {
            _isSilentMode = isSilent;
            if (_isSilentMode)
            {
                DeactivateUI();
            }
        }

        public void DeactivateUI()
        {
            if ( _heartAnimationCoroutine != null)
            {
                StopCoroutine(_heartAnimationCoroutine);
                _heartAnimationCoroutine = null;
                
                SpriteRenderer specimenImage = _attachedSpecimen.GetComponent<SpriteRenderer>();
                specimenImage.color = _standardColor;
            }
            
            _attachedObject.gameObject.SetActive(false);
        }
        
        private void SetNewValue(int newHitPointsValue)
        {
            var healthBar = _attachedObject.transform.Find("HitPoints").gameObject;
            var textComponent = healthBar.GetComponent<TextMeshProUGUI>();
            textComponent.text = newHitPointsValue.ToString();
            
            healthBar.SetActive(true);
        }

        public void ShowCurrentHitPoints()
        {
            if (!_isSilentMode)
            {
                var heartImage = GetHeartImage();
                heartImage.fillAmount = (float)_attachedSpecimen.hitPoints / _maxHitPoints;
            
                _attachedObject.gameObject.SetActive(true);
                var healthBar = _attachedObject.transform.Find("HitPoints").gameObject;
                healthBar.SetActive(false);
            }
        }

        public void ShowPossibleDamage(int possibleDamage, int addedScore, bool isDead)
        {
            if (!_isSilentMode)
            {
                SetNewValue(addedScore);
            
                _attachedObject.gameObject.SetActive(true);

                if (_heartAnimationCoroutine == null)
                {
                    var heartImage = GetHeartImage();
                    _heartAnimationCoroutine = StartHeartAnimation(heartImage, possibleDamage, isDead);
                    StartCoroutine(_heartAnimationCoroutine);
                }
            }
        }

        private IEnumerator StartHeartAnimation(Image heartImage, int possibleDamage, bool isDead)
        {
            SpriteRenderer specimenImage = _attachedSpecimen.GetComponent<SpriteRenderer>();
            _standardColor = specimenImage.color;

            while (heartImage.gameObject.activeInHierarchy)
            {
                heartImage.fillAmount = (float)_attachedSpecimen.hitPoints / _maxHitPoints;
                if (isDead)
                {
                    specimenImage.color = _standardColor;
                }
                
                yield return new WaitForSeconds(_delay);
                heartImage.fillAmount = 
                    (float)(_attachedSpecimen.hitPoints - possibleDamage) / _maxHitPoints;

                if (isDead)
                {
                    specimenImage.color = _animationColor;
                }
                
                
                yield return new WaitForSeconds(_delay);
            }
        }
    }
}