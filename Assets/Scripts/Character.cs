/*
@Authors - Patrick, Landon
@Description - Character class that defines behavior for all objects that can shoot. Basically an interface but with 
datafields.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Character : MonoBehaviour
{
    //serialize needed to customize prefabs
    [SerializeField] protected int health;
    // in seconds
    [SerializeField] protected float maxShootCooldown;    
    [SerializeField] protected float shootCooldown;
    [SerializeField] protected float speed;

    protected Weapon weapon;
    protected Rigidbody rigidbody;
    

    virtual protected void Shoot()
    {
        Debug.LogWarning("You better implement the Shoot() method, or you'll be in a heap of trouble, partner.");
    }
}
