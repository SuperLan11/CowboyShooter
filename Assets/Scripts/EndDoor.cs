using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndDoor : MonoBehaviour
{
    private Animator fadeAnim;
    // Start is called before the first frame update
    void Start()
    {
        fadeAnim = GameObject.Find("FadePanel").GetComponent<Animator>();                
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.name == "Player")
        {
            int curSceneIndex = SceneManager.GetActiveScene().buildIndex;            
            StartCoroutine(FadeToNewScene(curSceneIndex+1, 1f));
        }
    }

    // while fadeTime is customizable, the fade animation length will not change unless you change it manually
    private IEnumerator FadeToNewScene(int buildIndex, float fadeTime)
    {        
        fadeAnim.Play("FadeOut");
        yield return new WaitForSecondsRealtime(fadeTime);
        SceneManager.LoadScene(buildIndex);
        // The animator will automatically play a fade in animation when the scene loads
        // as long as the HUD prefab is in that scene.
        // To adjust the fade in/out animation timing, change the FadeIn/FadeOut clips
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
