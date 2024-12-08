/*
@Authors - Landon
@Description - Class that keeps track of the time during the level. Used to set variables in game manager, and to display
level time.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class Clock : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI clock;

    void Start()
    {
        GameManager.levelTime = 0.0; // Reset timer at the start of the level
    }

    void Update()
    {
        GameManager.levelTime += Time.deltaTime;
        int minutes = (int)(GameManager.levelTime / 60);
        double seconds = GameManager.levelTime % 60;

        // Update the clock display
        clock.text = $"Time: {minutes:D2}:{seconds:F2}";
    }
}


// public class Clock : MonoBehaviour
// {
//     [SerializeField] private TextMeshProUGUI clock;
//     public static double rawSeconds = 0.0;

//     // Start is called before the first frame update
//     void Start()
//     {
//         rawSeconds = GameManager.levelTime;
//     }

//     // Update is called once per frame
//     void Update()
//     {
//         rawSeconds += Time.deltaTime;
//         double seconds = System.Math.Round(rawSeconds % 60, 2);
//         int minutes = (int)(rawSeconds / 60) % 60;

//         // uncomment if we include hours in the timer
//         // OR make timer dynamic and change size as it increases
//         /*clock.text = "Time: ";
//         if (hours < 10)
//             clock.text += "0";
//         clock.text += (hours + ":");*/

//         clock.text = "Time: ";
//         if (minutes < 10)
//             clock.text += "0";
//         clock.text += (minutes + ":");

//         if (seconds < 10)
//             clock.text += "0";
//         clock.text += seconds;
//     }
// }
