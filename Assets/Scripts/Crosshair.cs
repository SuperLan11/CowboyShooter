using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;

using Image = UnityEngine.UI.Image;

public class HUD : MonoBehaviour
{
    [SerializeField] private GameObject crosshair;
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

        crosshairImage = crosshair.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        bool pointedAtObj = Player.player.ObjAimedAt() != null;            
        
        //!needs to be two separate if-statement blocks so that it doesn't try to access null obj
        if (!pointedAtObj)
        {
            crosshairImage.color = Color.white;
            return;
        }

        bool pointedAtEnemy = Player.player.ObjAimedAt().GetComponent<Enemy>() != null;
        bool pointedAtHook = Player.player.ObjAimedAt().tag == "HOOK";
        bool pointedAtEnemyOrHook = (pointedAtEnemy || pointedAtHook);

        if (!pointedAtEnemyOrHook)
        {
            crosshairImage.color = Color.white;            
        }
        else if (pointedAtEnemy)
        {
            crosshairImage.color = Color.red;
        }
        else if (pointedAtHook)
        {
            crosshairImage.color = orange;
        }
    }
}
