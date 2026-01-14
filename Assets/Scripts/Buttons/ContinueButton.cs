using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ContinueButton : VRButton
{
    SceneController controller;
    public float delayedEnable;
    
    public void SetupConfirmButton(SceneController controller)
    {
        GetComponent<Image>().color = Color.gray;
        isEnabled = false;
        StartCoroutine(EnableAfterDelay(delayedEnable));
        this.controller = controller;
    }

    private IEnumerator EnableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        EnableButton();
    }

    public void EnableButton()
    {
        isEnabled = true;
        GetComponent<Image>().color = Color.white;
    }

    public override void PressButton()
    {
        if (!isEnabled) return;
        controller.fadeController.FadeOutNoDelay();
        controller.ResumeScene();
    }
}
