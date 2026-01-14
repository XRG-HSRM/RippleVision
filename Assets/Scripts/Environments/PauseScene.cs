using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PauseScene : Environments
{
    public List<ContinueButton> buttons = new();

    public override void SetupEnvironment()
    {
        base.SetupEnvironment();
        EnableButtons();
        controller.GetLaserPointer().ActivateLaserPointer(true);
    }

    private void EnableButtons()
    {
        foreach (var button in buttons)
        {
            button.SetupConfirmButton(controller);
        }
    }
}
