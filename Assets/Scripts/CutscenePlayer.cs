using UnityEngine;
using UnityEngine.InputSystem;

public class CutscenePlayer : MonoBehaviour
{
    public WinScreen winScreen;

    public void SkipActivated(InputAction.CallbackContext context)
    {
        if ((context.started || context.performed) && !winScreen.skippedCutscene)
        {
            winScreen.skippedCutscene = true;

            if (!winScreen.cutsceneCompleted)
            {
                winScreen.StartWinScreen();
            }
        }
    }
}
