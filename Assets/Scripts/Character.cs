/*
@Authors - Patrick, Landon
@Description - Character class that defines behavior for all objects that can shoot. Basically an interface but with 
datafields.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Character : MonoBehaviour
{
    //serialize needed to customize prefabs
    [SerializeField] protected int health;
    // in seconds
    [SerializeField] protected float maxShootCooldown;    
    [SerializeField] protected float shootCooldown;
    [SerializeField] protected float speed;

    protected AudioSource shootSfx;

    protected Weapon weapon;
    protected Rigidbody rigidbody;
    

    virtual protected void Shoot(GameObject obj)
    {
        Debug.LogWarning("You better implement the Shoot() method, or you'll be in a heap of trouble, partner.");
    }

    virtual protected void Death()
    {
        Debug.LogWarning("You better implement the Death() method, or you'll be in a heap of trouble, partner.");
    }

    virtual public void TakeDamage(int damage)
    {
        health -= damage;
        //Debug.Log("health: " + health);
        if (this.name == "Player")
        {
            // this should get the hearts in the hierarchy order so you don't need to sort
            Image[] images = FindObjectsOfType<Image>();
            List<Image> hearts = new List<Image>();
            foreach(Image img in images)
            {
                if (img.gameObject.name.Contains("Heart"))
                    hearts.Add(img);

            }            
            Destroy(hearts[hearts.Count - 1].gameObject);
        }

        if (health <= 0)
            Death();        
    }
}
