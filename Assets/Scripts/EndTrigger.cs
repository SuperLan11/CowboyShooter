using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndTrigger : MonoBehaviour
{
    private SceneTransfer fadeAnim;

    //prevents multiple yee-haws
    private bool yeehawPlayed = false;

    // Start is called before the first frame update
    void Start()
    {
        fadeAnim = GameObject.Find("FadePanel").GetComponent<SceneTransfer>();
        // assuming we want the level end to be an invisible trigger
        //GetComponent<MeshRenderer>().enabled = false;
    }

    private void OnTriggerEnter(Collider collider)
    {
        fadeAnim.enabled = true;
        //comment this book out of condition if you're using old end trigger instead of tornado
        bool movingTowardsTornado = (Player.player.currentMovementState == Player.movementState.FLYING) || 
                                    (Player.player.currentMovementState == Player.movementState.SWINGING);

        if (collider.gameObject.name == "Player" && movingTowardsTornado)
        {
            //!if we want any extra functionality for the tornado like screen shake, do it here!
            
            GameManager.gameManager.StoreTimerValue(Clock.rawSeconds);
            GameManager.totalTime += GameManager.storedTime;
            GameManager.totalTime += Time.timeSinceLevelLoad; 
            GameManager.gameManager.ResetTimerValue();
            GameManager.currentCheckpoint = 0;
    
            if (!yeehawPlayed)
            {
                Player.player.yeehawSfx.Play();
                yeehawPlayed = true;
            }
            //prevents player from infinitely going back and forth
            Player.player.SetKinematicRigidbody();
            int curSceneIndex = SceneManager.GetActiveScene().buildIndex;
            StartCoroutine(fadeAnim.FadeToNewScene(curSceneIndex + 1, 1f));
                        
        }
        /*
        else if (collider.gameObject.name == "Player")
        {
            GameManager.gameManager.ResetTimerValue();
            GameManager.currentCheckpoint = 0;

            Player.player.yeehawSfx.Play();
            int curSceneIndex = SceneManager.GetActiveScene().buildIndex;
            StartCoroutine(fadeAnim.FadeToNewScene(curSceneIndex + 1, 1f));
        }*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
