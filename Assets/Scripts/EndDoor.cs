using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndDoor : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.name == "Player")
        {
            int curSceneIndex = SceneManager.GetActiveScene().buildIndex;
            Debug.Log("curSceneIndex: " + curSceneIndex);
            Debug.Log("num build scenes: " + SceneManager.sceneCountInBuildSettings);
            Scene nextLevel = SceneManager.GetSceneByBuildIndex(curSceneIndex + 1);                        
            SceneManager.SetActiveScene(nextLevel);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
