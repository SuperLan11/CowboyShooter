using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    public void EnableHUD()
    {
        this.gameObject.SetActive(true);
    }

    public void DisableHUD()
    {
        this.gameObject.SetActive(false);
    }
}
