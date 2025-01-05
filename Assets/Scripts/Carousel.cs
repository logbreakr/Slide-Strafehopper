using UnityEngine;

public class Carousel : MonoBehaviour
{
    [SerializeField]
    private float spinSpeed = 100;
    private Vector3 rbAngularVelocity;
    Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rbAngularVelocity = new Vector3(0, spinSpeed, 0);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Quaternion deltaRotation = Quaternion.Euler(rbAngularVelocity * Time.fixedDeltaTime);
        rb.MoveRotation(rb.rotation * deltaRotation);
    }
}
