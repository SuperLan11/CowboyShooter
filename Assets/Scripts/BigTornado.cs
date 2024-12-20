using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigTornado : MonoBehaviour
{
    [SerializeField] private Transform stopLoc;
    [SerializeField] private float tornadoAccel;
    public static float tornadoTopY;
    public static float playerDegreesPerSecond = 360*5f;
    public static float spinAccel = 0.3f;
    public static float upVel = 30f;

    bool yeehawPlayed = false;

    private AudioSource tornadoSfx;

    // Start is called before the first frame update
    void Start()
    {        
        tornadoTopY = GetComponent<MeshRenderer>().bounds.max.y;
        tornadoSfx = GetComponent<AudioSource>();
        stopLoc = GameObject.Find("TornadoStop").transform;
        if (tornadoSfx != null)
            tornadoSfx.Play();
    }

    private void OnTriggerEnter(Collider collider)
    {
        //comment this book out of condition if you're using old end trigger instead of tornado
        bool movingTowardsTornado = (Player.player.currentMovementState == Player.movementState.FLYING) ||
                                    (Player.player.currentMovementState == Player.movementState.SWINGING);

        if (collider.gameObject.name == "Player" && movingTowardsTornado)
        {
            // teleport player to center of tornado
            Vector3 teleportPos = transform.position;
            teleportPos.y += 45f;
            Player.player.transform.position = teleportPos;
            Player.player.currentMovementState = Player.movementState.TORNADO;        
            

            //GameManager.gameManager.StoreTimerValue(Clock.rawSeconds);
            GameManager.totalTime += GameManager.levelTime;
            //GameManager.totalTime += Time.timeSinceLevelLoad;
            GameManager.gameManager.ResetTimerValue();
            
            GameManager.gameManager.gameIsEnding = true;
            //Dr. Towle, please forgive me
            Destroy(GameObject.Find("HUD"));//.GetComponent<PlayerHUD>().DisableHUD();

            GameManager.currentCheckpoint = 0;
            Player.hasCheckpoint = false;

            if (!yeehawPlayed)
            {
                Player.player.yeehawSfx.Play();
                yeehawPlayed = true;
            }            
        }
    }

    // Update is called once per frame
    void Update()
    {        
        transform.position = Vector3.Lerp(transform.position, stopLoc.position, tornadoAccel*Time.deltaTime);
    }
}
