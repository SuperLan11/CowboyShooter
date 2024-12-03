/*
@Authors - Patrick and Landon
@Description - Recycled main menu code from UI lab
*/

using System.Collections;
using System.Collections.Generic;
//using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;

using Image = UnityEngine.UI.Image;

public class Crosshair : MonoBehaviour
{
    [SerializeField] private GameObject crosshair;
    private Player player;
    private Image crosshairImage;
    private Color32 orange = new Color32(255, 165, 0, 255);


    // Start is called before the first frame update
    void Start()
    {
        if (crosshair == null)
        {
            Debug.LogError("I reckon you don't have a crosshair, partner!");
            return;
        }
        player = FindObjectOfType<Player>();

        crosshairImage = crosshair.GetComponent<Image>();
    }

    private bool PointedAtObj(){
        return Player.player.ObjAimedAt() != null;
    }

    private bool PointedAtEnemy()
    {
        return Player.player.ObjAimedAt().GetComponent<Enemy>() != null;
    }

    private bool PointedAtHook()
    {
        return Player.player.ObjAimedAt().tag == "HOOK";
    }

    private bool PointedAtTornado()
    {
        return Player.player.ObjAimedAt().tag == "TORNADO";
    }

    private bool PointedAtImportantObject()
    {
        return PointedAtEnemy() || PointedAtHook() || PointedAtTornado();
    }

    public void EnableCrosshair()
    {
        crosshairImage.enabled = true;
    }

    public void DisableCrosshair()
    {
        crosshairImage.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {        
        Debug.Log(Player.player.ObjAimedAt().tag);
        
        if (player.shootCooldown < player.maxShootCooldown)
        {
            if (PointedAtObj() && (PointedAtHook() || PointedAtTornado()))
            {
                crosshairImage.color = orange;
            }
            else
            { 
                crosshairImage.color = Color.grey;
            }
            return;
        }

        //!needs to be two separate if-statement blocks so that it doesn't try to access null obj
        if (!PointedAtObj())
        {
            crosshairImage.color = Color.white;
            return;
        }

        if (!PointedAtImportantObject())
        {
            crosshairImage.color = Color.white;            
        }
        else if (PointedAtEnemy())
        {
            crosshairImage.color = Color.red;
        }
        else if (PointedAtHook() || PointedAtTornado())
        {
            crosshairImage.color = orange;
        }
    }
}
