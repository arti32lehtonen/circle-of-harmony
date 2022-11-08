using UnityEngine;
using UnityEngine.SceneManagement;

namespace Menu
{
    public class StartSceneButton : MonoBehaviour
    {
        public string sceneName;
        
        public void LoadLevel()
        {
            GetComponent<AudioSource>().Play();
            SceneManager.LoadScene(sceneName);
        }
    }
}