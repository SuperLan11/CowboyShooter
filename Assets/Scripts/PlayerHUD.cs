using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private GameObject fadePanel;

    public void EnableHUD()
    {
        this.gameObject.SetActive(true);
        fadePanel.SetActive(false);
    }

    public void DisableHUD()
    {
        this.gameObject.SetActive(false);
    }
}
