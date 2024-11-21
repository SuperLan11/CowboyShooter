using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndTrigger : MonoBehaviour
{
    private SceneTransfer fadeAnim;
    // Start is called before the first frame update
    void Start()
    {
        fadeAnim = GameObject.Find("FadePanel").GetComponent<SceneTransfer>();
        // assuming we want the level end to be an invisible trigger
        GetComponent<MeshRenderer>().enabled = false;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.name == "Player")
        {
            int curSceneIndex = SceneManager.GetActiveScene().buildIndex;
            StartCoroutine(fadeAnim.FadeToNewScene(curSceneIndex+1, 1f));            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
