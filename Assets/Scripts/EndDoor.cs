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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "PLAYER")
        {
            int curSceneIndex = SceneManager.GetActiveScene().buildIndex;
            string nextLevel = SceneManager.GetSceneByBuildIndex(curSceneIndex + 1).name;
            Debug.Log("loading scene");
            SceneManager.LoadScene(nextLevel);            
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
