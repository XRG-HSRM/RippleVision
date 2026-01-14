using UnityEngine;

public class TemporalVision : VisionCatcher
{
    static float speedR = 1f / 180f;

    public override void StartVisionCatcher()
    {
        if (running) { return; }
        running = true;
    }

    public override void StopVisionCatcher()
    {
        if (!running) { return; }
        running = false;
        DisableTemporalGuidance();
    }

    private void Update()
    {
        if (!running) { return; }
        Time.timeScale = 1f - CalculateEyeGazeAngle();
    }

    private void DisableTemporalGuidance()
    {
        Time.timeScale = 1;
    }

    private float CalculateEyeGazeAngle()
    {
        float result = 0f;
        if (controller != null)
        {
            result = Vector3.Angle(controller.currentEyeTrackingScript.eyetrackingTransform.position - mainCamera.transform.position, transformToCatch.position - mainCamera.transform.position);
        }
        // to allow for a margin of error
        result -= 5f;
        if (result < 0f) result = 0f;
        result *= speedR;
        return result;
    }
}
