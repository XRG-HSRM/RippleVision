using UnityEngine;
using UnityEngine.UI;

public class ConfirmButton : VRButton
{
    SceneController controller;

    public void SetupConfirmButton(SceneController controller)
    {
        GetComponent<Image>().color = Color.gray;
        isEnabled = false;
        this.controller = controller;
    }

    public void EnableButton()
    {
        isEnabled = true;
        GetComponent<Image>().color = Color.white;
    }

    public override void PressButton()
    {
        if (!isEnabled) return;
        controller.ConfirmAnswers();
    }
}
