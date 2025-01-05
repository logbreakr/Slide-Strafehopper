using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;

[RequireComponent(typeof(Camera))]
public class Camera : MonoBehaviour
{
    [SerializeField]
    GameObject cameraHolder;
    [SerializeField]
    Rigidbody rb;
    
    [SerializeField]
    Material material;

    float alphaModify;

    void Start()
    {
        material.SetFloat("_Alpha", 0f);
        material.SetFloat("_Alpha", 0.5f);
        alphaModify = 0f;
    }

    void Update()
    {
        transform.rotation = cameraHolder.transform.rotation;
        transform.position = cameraHolder.transform.position;
        alphaModify = Mathf.Max(((rb.linearVelocity.magnitude - 10) / 40f), 0f);
        material.SetFloat("_Alpha", alphaModify);
        material.SetFloat("_Speed", alphaModify);
    }
}
