using System;
using System.Collections.Generic;
using UnityEngine;


namespace DefaultNamespace
{
    public class TutorialManager : MonoBehaviour
    {
        public List<GameObject> tutorialObjects;
        public GameObject pauseCanvas;
        private int _currentIndex = -1;
        private float _delay;

        public void Awake()
        {
            if (!GameGlobalState.instance.tutorialState)
            {
                SkipTutorial();
            }
        }

        public void ReturnToGame(bool playSound = false)
        {
            if (playSound)
            {
                GetComponent<AudioSource>().Play();
            }
            
            pauseCanvas.SetActive(false);
            tutorialObjects[_currentIndex].SetActive(false);
        }

        public void ShowNextTutorial(bool playSound = false)
        {
            if (playSound)
            {
                GetComponent<AudioSource>().Play();
            }
            
            if (_currentIndex >= 0 && _currentIndex < tutorialObjects.Count)
            {
                tutorialObjects[_currentIndex].SetActive(false);
            }
            
            if (_currentIndex >= tutorialObjects.Count - 1)
            {
                // can't show anything
                GameGlobalState.instance.tutorialState = false;
                return;
            }
            
            _currentIndex += 1;
            pauseCanvas.SetActive(true);
            tutorialObjects[_currentIndex].SetActive(true);
        }

        public void SkipTutorial(bool playSound = false)
        {
            if (playSound)
            {
                GetComponent<AudioSource>().Play();
            }   
            pauseCanvas.SetActive(false);
            if (_currentIndex >= 0 && _currentIndex < tutorialObjects.Count)
            {
                tutorialObjects[_currentIndex].SetActive(false);
            }
            _currentIndex = tutorialObjects.Count;
            GameGlobalState.instance.tutorialState = false;
        }
    }
}