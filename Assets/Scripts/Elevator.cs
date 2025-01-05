using UnityEngine;

public class Elevator : MonoBehaviour
{
    Rigidbody rb;
    public float speed;
    public GameObject[] points;
    private int i;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(Vector3.Distance(transform.position, points[i].transform.position) < 0.02f)
        {
            i++;
            if (i == points.Length)
            {
                i = 0;
            }
        }
        rb.MovePosition(Vector3.MoveTowards(transform.position, points[i].transform.position, speed * Time.fixedDeltaTime));
    }
}
