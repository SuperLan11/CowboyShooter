using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Potion : MonoBehaviour
{
    private AudioSource potionSfx;
    private MeshRenderer potionMesh;
    private bool playingSound = false;

    // Start is called before the first frame update
    void Start()
    {
        potionSfx = GetComponent<AudioSource>();
        potionMesh = GetComponent<MeshRenderer>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            bool hitPlayer = collision.GetContact(i).otherCollider.gameObject.name == "Player";
            if (hitPlayer && !playingSound)
            {
                potionSfx.Play();
                playingSound = true;
                potionMesh.enabled = false;
                int curHealth = Player.player.GetHealth();
                int maxHealth = Player.player.GetMaxHealth();
                if (curHealth < maxHealth)
                    Player.player.SetHealth(curHealth + 1);
                break;
            }
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
