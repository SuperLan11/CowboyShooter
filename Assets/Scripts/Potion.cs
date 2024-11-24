using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Potion : MonoBehaviour
{
    private AudioSource potionSfx;
    private bool playingSound = false;

    // Start is called before the first frame update
    void Start()
    {
        potionSfx = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.name == "Player" && !playingSound)
        {
            potionSfx.Play();
            playingSound = true;
            GetComponent<MeshRenderer>().enabled = false;
            int curHealth = Player.player.GetHealth();
            //Player.player.SetHealth(3);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // when sound effect ends
        if (playingSound && !potionSfx.isPlaying)
            Destroy(this.gameObject);
    }
}
