/*
@Authors - Patrick and Landon
@Description - Recycled main menu code from UI lab
*/

using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;

using Image = UnityEngine.UI.Image;

public class HUD : MonoBehaviour
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

    private bool pointedAtObj(){
        return Player.player.ObjAimedAt() != null;    
    }

    private bool pointedAtEnemy()
    {
        return Player.player.ObjAimedAt().GetComponent<Enemy>() != null;
    }

    private bool pointedAtHook()
    {
        return Player.player.ObjAimedAt().tag == "HOOK";
    }

    private bool pointedAtEnemyOrHook()
    {
        return pointedAtEnemy() || pointedAtHook();
    }

    // Update is called once per frame
    void Update()
    {
        if (player.shootCooldown < player.maxShootCooldown)
        {
            if (pointedAtObj() && pointedAtHook())
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
        if (!pointedAtObj())
        {
            crosshairImage.color = Color.white;
            return;
        }

        if (!pointedAtEnemyOrHook())
        {
            crosshairImage.color = Color.white;            
        }
        else if (pointedAtEnemy())
        {
            crosshairImage.color = Color.red;
        }
        else if (pointedAtHook())
        {
            crosshairImage.color = orange;
        }
    }
}
