using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class Arrow : VisionCatcher
{
    [SerializeField] private GameObject arrow;

    GameObject staticArrow;
    GameObject dynamicArrow;

    public override void StartVisionCatcher()
    {
        if (running) { return; }
        CreateArrows();
        running = true;
    }

    public override void StopVisionCatcher()
    {
        if (!running) { return; }
        running = false;
        DestroyArrows();
    }

    private void CreateArrows()
    {
        DynamicArrow();
        FixedArrow();
    }

    private void FixedArrow()
    {
        staticArrow = Instantiate(arrow);
        staticArrow.transform.localScale = new Vector3(3f, 3f, 3f);
    }

    private void SetFixedArrow()
    {
        staticArrow.transform.position = transformToCatch.position + Vector3.up * 2f;
        Vector3 direction = transformToCatch.position - staticArrow.transform.position;
        staticArrow.transform.rotation = Quaternion.LookRotation(direction);
    }

    private void DynamicArrow()
    {
        dynamicArrow = Instantiate(arrow, transform.position, Quaternion.identity, transform);
    }

    private void DestroyArrows()
    {
        Destroy(staticArrow);
        Destroy(dynamicArrow);
    }

    private void Update()
    {
        if (!running) { return; }
        SetPosition();
        SetRotation();
        SetFixedArrow();
    }

    void SetPosition()
    {
        transform.position = mainCamera.transform.position + mainCamera.transform.forward * 2f + mainCamera.transform.up * 1f;
    }

    void SetRotation()
    {
        if (transformToCatch != null)
        {
            Vector3 direction = (transformToCatch.position - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
