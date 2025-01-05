using UnityEngine;
using static UnityEngine.UI.Image;

public class Hammer : MonoBehaviour
{
    Rigidbody rb;
    [SerializeField]
    GameObject rotationPoint;
    [SerializeField]
    float spin;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Quaternion deltaRotation = Quaternion.AngleAxis(spin, -Vector3.forward);
        rb.MovePosition(deltaRotation * (rb.transform.position - rotationPoint.transform.position) + rotationPoint.transform.position);
        rb.MoveRotation(rb.transform.rotation * deltaRotation);
    }
}
