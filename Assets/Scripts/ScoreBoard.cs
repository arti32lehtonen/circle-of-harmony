using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class ScoreBoard : MonoBehaviour
    {
        private int _score = 0;
        private int _currentMinValue = 0;
        private int _currentMaxValue = 0;

        public BuildingOptionManager buildingManager;
        public GameObject slider;
        public float speed = 0.1f;
        public float fillDelay = 0.05f;
        public GameOverManager gameOverManager;
        
        [SerializeField]
        private Image _fillImage;
        [SerializeField]
        private Image _intendedFillImage;
        private bool _isUpdating = false;
        [SerializeField]
        private TextMeshProUGUI _textComponent;
        private IEnumerator _addingScoreAnimationCoroutine;
        private int _scoreBeforeAnimation;
        
        public void Awake()
        {
            _fillImage.fillAmount = 0;
            _intendedFillImage.fillAmount = 0;
            UpdateDisplayedText($"Score: {_score} / {_currentMaxValue}");
        }

        private void UpdateDisplayedText(string displayedString)
        {
            _textComponent.text = displayedString;
        }

        public void AddValueToScore(int addValue)
        {
            UpdateGraphicComponentsAsync(addValue);
        }

        public int GetScore()
        {
            return _score;
        }

        private float GetNormalizedScore()
        {
            return (float)(_score - _currentMinValue) / (_currentMaxValue - _currentMinValue);
        }

        public void ShowScore(bool isBuildingNow, int addValue = 0)
        {
            string additionString;
            if (isBuildingNow)
            {
                string modifier;
                modifier = addValue >= 0 ? "+" : "-";
                additionString = $"({modifier}{addValue})";
            }
            else
            {
                additionString = "";
            }
            
            UpdateDisplayedText($"Score: {_score} {additionString} / {_currentMaxValue}");
            float intendedScore = (float)(_score + addValue - _currentMinValue) / 
                                  (_currentMaxValue - _currentMinValue);
            intendedScore = intendedScore > 1 ? 1 : intendedScore;
            _intendedFillImage.fillAmount = intendedScore;

        }

        public void UpdateMaxValue(int addToMaxValue)
        {
            _currentMinValue = _currentMaxValue;
            _currentMaxValue += addToMaxValue;
        }
        
        private void UpdateGraphicComponentsAsync(int addValue)
        {
            // wait for old coroutine to finish if it exists
            while (_addingScoreAnimationCoroutine != null)
            {
                continue;
            }
            
            _scoreBeforeAnimation = _score;
            _score += addValue;

            _addingScoreAnimationCoroutine = UpdateFillBar();
            StartCoroutine(_addingScoreAnimationCoroutine);
        }

        private void UpdateGraphicComponentsSync()
        {
            var normalizedScore = GetNormalizedScore();
            _fillImage.fillAmount = normalizedScore;
            _intendedFillImage.fillAmount = 0;
            UpdateDisplayedText($"Score: {_score} / {_currentMaxValue}");
        }

        private IEnumerator UpdateFillBar()
        {
            float newNormalizedValue = GetNormalizedScore();
            int maxIter = 0;
            bool atLeastOneUpdate = false;
            
            while (true)
            {
                float sliderUpperBound = Mathf.Min(1, newNormalizedValue);
                
                while (_fillImage.fillAmount < sliderUpperBound)
                {
                    float addValue = 
                        _fillImage.fillAmount + speed > sliderUpperBound ? 
                            sliderUpperBound - _fillImage.fillAmount : speed;
                    
                    _fillImage.fillAmount += addValue;
                    UpdateDisplayedText($"Score: {_score} / {_currentMaxValue}");
                    yield return new WaitForSeconds(fillDelay);
                }

                if (newNormalizedValue >= 1)
                {
                    atLeastOneUpdate = true;
                    // this changes _currentMaxValue and _currentMinValue
                    buildingManager.HandleBeatenScore();
                    newNormalizedValue = GetNormalizedScore();

                    _intendedFillImage.fillAmount = 0;
                    _fillImage.fillAmount = 0;
                }
                else
                {
                    break;
                }

                maxIter++;
                if (maxIter > 100000)
                {
                    Debug.Log("ALARM");
                    break;
                }
            }

            _intendedFillImage.fillAmount = 0;
            _addingScoreAnimationCoroutine = null;
            
            if (!atLeastOneUpdate && gameOverManager.CheckNoBuildingOptionsLeft())
            {
                gameOverManager.StartGameOverWindow();
            }
        }

        public void HandleUndoAction(int undoScore)
        {
            if (_addingScoreAnimationCoroutine == null)
            {
                _score -= undoScore;
            }
            else
            {
                StopCoroutine(_addingScoreAnimationCoroutine);
                _addingScoreAnimationCoroutine = null;
                _score = _scoreBeforeAnimation;
            }
            
            UpdateGraphicComponentsSync();
        }
    }
}