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
        //GetComponent<MeshRenderer>().enabled = false;
    }

    private void OnTriggerEnter(Collider collider)
    {
        //comment this book out of condition if you're using old end trigger instead of tornado
        bool movingTowardsTornado = (Player.player.currentMovementState == Player.movementState.FLYING) || 
                                    (Player.player.currentMovementState == Player.movementState.SWINGING);

        if (collider.gameObject.name == "Player" && movingTowardsTornado)
        {
            GameManager.gameManager.ResetTimerValue();

            Player.player.yeehawSfx.Play();
            int curSceneIndex = SceneManager.GetActiveScene().buildIndex;
            StartCoroutine(fadeAnim.FadeToNewScene(curSceneIndex+1, 1f));            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
