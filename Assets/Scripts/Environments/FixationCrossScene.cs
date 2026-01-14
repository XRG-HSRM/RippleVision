using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixationCrossScene : Environments
{
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material inactive;
    [SerializeField] private Material active;
    [SerializeField] private List<Renderer> fixationCrossRenderers = new();
    [SerializeField] private List<Renderer> cylinderRenderers = new();
    [SerializeField] private Transform sceneCenter;
    float fixationCrossTimer = 1.5f;
    private float timer = 0.0f;
    private bool isExecutingFunction = false;
    private float switchTimer = 0.0f;
    private bool defaultMaterialActive = true;
    private bool lookingAtFixationCross = false;

    private void Update()
    {
        if (controller == null)
        {
            return;
        }
        if (IsAbove(controller.GetCamera(), sceneCenter))
        {
            SetCircleMaterial(active);
        }
        else
        {
            SetCircleMaterial(inactive);
            SetCrossMaterial(defaultMaterial);
            return;
        }
        if (controller.currentEyeTrackingScript.lookingAtSearchObject)
        {
            // eyeTracking
            timer += Time.deltaTime;
            if (timer >= fixationCrossTimer && !isExecutingFunction)
            {
                GoToNextScene();
                isExecutingFunction = true;
            }

            // material
            switchTimer = 0.0f;
            if (!defaultMaterialActive)
            {
                SetCrossMaterial(defaultMaterial);
                defaultMaterialActive = true;
            }
            if (!lookingAtFixationCross)
            {
                SetCrossMaterial(active);
            }

            lookingAtFixationCross = true;
        }
        else
        {
            // eyeTracking
            timer = 0.0f;
            isExecutingFunction = false;

            // material
            if (lookingAtFixationCross)
            {
                SetCrossMaterial(inactive);
            }

            switchTimer += Time.deltaTime;
            if (switchTimer >= 0.5f)
            {
                if (defaultMaterialActive)
                {
                    SetCrossMaterial(inactive);
                }
                else
                {
                    SetCrossMaterial(defaultMaterial);
                }
                defaultMaterialActive = !defaultMaterialActive;
                switchTimer = 0.0f;
            }

            lookingAtFixationCross = false;
        }
    }

    private void SetCrossMaterial(Material material)
    {
        foreach(Renderer renderer in fixationCrossRenderers)
        {
            renderer.material = material;
        }
    }

    private void SetCircleMaterial(Material material)
    {
        foreach (Renderer renderer in cylinderRenderers)
        {
            renderer.material = material;
        }
    }

    bool IsAbove(Transform objectA, Transform objectB, float margin = 0.5f)
    {
        Vector3 posA = objectA.position;
        Vector3 posB = objectB.position;

        bool isAbove = posA.y > posB.y;
        bool withinXMargin = Mathf.Abs(posA.x - posB.x) <= margin;
        bool withinZMargin = Mathf.Abs(posA.z - posB.z) <= margin;

        return isAbove && withinXMargin && withinZMargin;
    }

    private void GoToNextScene()
    {
        controller.fadeController.FadeOutNoDelay();
        controller.ResumeScene();
    }
}
