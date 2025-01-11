using UnityEngine;

public class ExperimentalIKWalk2 : MonoBehaviour
{
    Vector3 currentPosition;

    [SerializeField]
    Transform body;
    [SerializeField]
    float footSpacing;
    [SerializeField]
    float forwardOffset;
    [SerializeField]
    LayerMask layer;
    Vector3 oldPosition;
    Vector3 newPosition;
    [SerializeField]
    float stepDistance;
    float lerp;
    [SerializeField]
    float stepHeight;
    [SerializeField]
    float speed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = currentPosition;

        Ray ray = new Ray(body.position + (body.right * footSpacing) + (body.forward * forwardOffset), Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 2, layer))
        {
            if ((Vector3.Distance(newPosition, hit.point)) > stepDistance)
            {
                lerp = 0;
                newPosition = hit.point;
            }
        }
        if (lerp < 1)
        {
            Vector3 footPosition = Vector3.Lerp(oldPosition, newPosition, lerp);
            footPosition.y += Mathf.Sin(lerp * Mathf.PI) * stepHeight;

            currentPosition = footPosition;
            lerp += Time.deltaTime * speed;
        }
        else
        {
            oldPosition = newPosition;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(newPosition, 0.25f);
    }
}
