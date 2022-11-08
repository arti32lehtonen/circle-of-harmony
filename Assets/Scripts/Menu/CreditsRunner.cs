using System.Collections;
using UnityEngine;

namespace Menu
{
    public class CreditsRunner : MonoBehaviour
    {
        public GameObject buttonMenu;
        public GameObject creditsObject;
        public int creditsTime;

        private IEnumerator _stopCoroutine;

        public void RunCredits()
        {
            buttonMenu.SetActive(false);
            creditsObject.SetActive(true);
            _stopCoroutine = StopCreditsAfterTimer();
            StartCoroutine(_stopCoroutine);
        }

        public IEnumerator StopCreditsAfterTimer()
        {
            yield return new WaitForSeconds(creditsTime);
            creditsObject.SetActive(false);
            buttonMenu.SetActive(true);
            _stopCoroutine = null;
        }

        public void ClickBackButton()
        {
            if (_stopCoroutine != null)
            {
                StopCoroutine(_stopCoroutine);
                _stopCoroutine = null;
            }
            creditsObject.SetActive(false);
            buttonMenu.SetActive(true);
        }
    }
}