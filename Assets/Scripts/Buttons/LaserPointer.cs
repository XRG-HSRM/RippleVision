using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class LaserPointer : MonoBehaviour
{
    [Header("Current hand-controller transform: Use child transform of controller to get the direction right")]
    public Transform leftHandController;
    public Transform rightHandController;
    public LineRenderer lineRenderer;
    public float maxDistance = 10f;
    public LayerMask uiLayer;

    private SceneController controller;
    private Transform currentHandController;
    private VRButton currentVRButton;
    private VRButton lastButton;

    private float interval = 0.25f;
    private float timer = 0f;

    private void Awake()
    {
        currentHandController = rightHandController;
        controller = GetComponent<SceneController>();
        if (controller == null)
        {
            Debug.LogError("LaserPoint has to be assigned to the controlling GameObject");
        }
        if (lineRenderer == null)
        {
            lineRenderer = transform.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.005f;
            lineRenderer.endWidth = 0.005f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = Color.yellow;
            lineRenderer.endColor = Color.yellow;
        }
    }

    public void ActivateLaserPointer(bool active)
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = active;
        }
        else
        {
            Debug.LogError("LineRenderer is not assigned in LaserPointer script.");
        }
        if (active)
        {
            currentVRButton = null; // Reset the current VR button when activating the laser pointer
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        SwitchActiveController();
        HandleLaserPointer();
    }

    private void SwitchActiveController()
    {
        if (Input.GetKeyDown(KeyCode.S) ||
            controller.triggerLeft.action.ReadValue<float>() > 0.9f && currentHandController == rightHandController ||
            controller.triggerRight.action.ReadValue<float>() > 0.9f && currentHandController == leftHandController)
        {
            currentHandController = currentHandController == rightHandController ? leftHandController : rightHandController;
            Debug.Log("Switch main controller to the " + (currentHandController == rightHandController ? "right" : "left") + " hand.");
        }
    }

    private void HandleLaserPointer()
    {
        if (currentHandController == null || lineRenderer == null) return;
        Ray ray = new Ray(currentHandController.position, currentHandController.forward);
        lineRenderer.SetPosition(0, currentHandController.position);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, uiLayer))
        {
            lineRenderer.SetPosition(1, hit.point);
            VRButton hitVRButton = hit.collider.gameObject.GetComponent<VRButton>();
            if (hitVRButton != currentVRButton)
            {
                currentVRButton?.Hover(false);
                currentVRButton = hitVRButton;
                currentVRButton?.Hover(true);
            }

            bool leftPressed = controller.triggerLeft.action.ReadValue<float>() > 0.9f;
            bool rightPressed = controller.triggerRight.action.ReadValue<float>() > 0.9f;

            if ((leftPressed || rightPressed) && currentVRButton != null)
            {
                if (timer >= interval)
                {
                    if (lastButton != currentVRButton || currentVRButton.mainButton != null)
                    {
                        lastButton = currentVRButton;
                        currentVRButton.PressButton();
                    }
                    timer = 0f;
                }
            }
        }
        else
        {
            lineRenderer.SetPosition(1, currentHandController.position + currentHandController.forward * maxDistance);
            currentVRButton = null;
        }
    }
}
