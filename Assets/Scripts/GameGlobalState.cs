using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class GameGlobalState : MonoBehaviour
    {
        public static GameGlobalState instance { get; private set; }
        public bool tutorialState = true;
        public AudioSource mainTheme;

        public void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                instance = this;
            }
            
            DontDestroyOnLoad(this.gameObject);
        }

        public void Start()
        {
            mainTheme.Play();
        }
    }
}