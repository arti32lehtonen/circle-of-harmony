using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class BuildingButtonManager : MonoBehaviour
    {
        private bool _buttonState = true; 
        private int _amount;
        public SpeciesConfig config;
        public BuildingOptionManager manager;

        private AudioSource _audioComponent;
        private Vector3 _originalScale;

        private bool _isOvered = false;

        public void Awake()
        {
            var imageComponent = transform.GetComponent<Image>();
            imageComponent.sprite = config.image;

            _audioComponent = GetComponent<AudioSource>();

            _originalScale = transform.localScale;
        }

        public void StartBuilding()
        {
            //var audioComponent = GetComponent<AudioSource>();
            _audioComponent.Play();
            manager.HandleNewBuildingAction(config);
        }

        public void SetButtonState(bool newState)
        {
            if (_buttonState != newState)
            {
                transform.GetComponent<Button>().interactable = newState;
                var spriteComponent = transform.Find("Square").GetComponent<SpriteRenderer>();
                spriteComponent.sprite = newState ? manager.buttonOnImage : manager.buttonOffImage;
                spriteComponent.color = new Color(255, 255, 255, 0.8f);
            }
            _buttonState = newState;
        }

        public int GetAmount()
        {
            return _amount;
        }

        public void SetAmount(int newAmount)
        {
            _amount = newAmount;
            var amountComponent = 
                transform.GetChild(0).Find("OptionsUI").Find("Amount");
            var textComponent = amountComponent.GetComponent<TextMeshProUGUI>();
            textComponent.text = $"{_amount}";

            SetButtonState(_amount != 0);
        }
        
        public void AddToAmount(int newAmount)
        {
            
            SetAmount(_amount + newAmount);
        }

        public void SwitchOveredEffect(bool isOvered, bool ignoreState = false)
        {
            _isOvered = isOvered;
            
            if (!ignoreState)
            {
                if (_amount > 0)
                {
                    if (isOvered)
                    {
                        StartCoroutine(ShowHintWIthDelay(true));
                    }
                    else
                    {
                        manager.hintManager.SetActive(false, config);
                    }
                }
            }
            
            if (!isOvered)
            {
                transform.localScale = _originalScale;
            }
            else
            {
                if (ignoreState || _buttonState)
                {
                    transform.localScale = new Vector3(1.05f, 1.05f, 1.05f);
                }
            }
        }

        public void SwitchOveredEffect(bool isOvered)
        { 
            SwitchOveredEffect(isOvered, false);
        }

        public IEnumerator ShowHintWIthDelay(bool isOvered)
        {
            yield return new WaitForSeconds(1);
            if (_isOvered == isOvered)
            {
                manager.hintManager.SetActive(isOvered, config);
            }
        }
    }
}