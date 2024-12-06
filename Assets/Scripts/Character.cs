/*
@Authors - Patrick, Landon
@Description - Character class that's basically an interface but with datafields.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Character : MonoBehaviour
{
    //serialize needed to customize prefabs
    [SerializeField] protected int health;
    [SerializeField] protected int maxHealth;            
    protected AudioSource shootSfx;

    protected Weapon weapon;
    protected Rigidbody rigidbody;    

    virtual protected void Death()
    {
        Debug.LogWarning("You better implement the Death() method, or you'll be in a heap of trouble, partner.");
    }

    virtual public void TakeDamage(int damage)
    {
        Debug.LogWarning("You better implement the TakeDamage() method, or you'll be in a heap of trouble, partner.");
    }
}
