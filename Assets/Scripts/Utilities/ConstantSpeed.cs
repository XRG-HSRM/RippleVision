using UnityEngine;

public class ConstantSpeed : MonoBehaviour
{
    public float targetSpeed;
    private Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    void FixedUpdate()
    {
        rb.linearVelocity = rb.linearVelocity.normalized * targetSpeed;
    }
}