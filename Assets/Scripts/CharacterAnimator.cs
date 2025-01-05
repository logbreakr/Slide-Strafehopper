using UnityEditor.Animations;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    Rigidbody parentRb;
    Transform camTransform;
    Animator animator;

    float rotateThresh = 30;
    bool bodyCamAllign = false;

    void Start()
    {
        parentRb = transform.parent.GetComponent<Rigidbody>();
        camTransform = transform.parent.Find("CamHolder").transform;
        animator = GetComponent<Animator>();
    }

    void Update()
    {

        // animation control
        Vector3 velocity = Vector3.zero;

        if (transform.parent.parent != null)
        {
            velocity = parentRb.linearVelocity - transform.parent.parent.GetComponent<Rigidbody>().GetPointVelocity(transform.position);
        }
        else
        {
            velocity = parentRb.linearVelocity;
        }

        velocity = Vector3.ProjectOnPlane(velocity, Vector3.up);

        float speed = velocity.magnitude / 11;
        animator.SetFloat("Speed", speed);

        
        // rotation control FIX ANGLE DIFF, RETURNS SUPER HIGH NUMBERS DUE TO EULER ANGLES GOING FROM 0 TO 365 INSTEAD OF 0-180 TWICE
        Quaternion rotation = transform.rotation;

        Quaternion bodyRotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
        Quaternion CamRotation = Quaternion.Euler(0f, camTransform.rotation.eulerAngles.y, 0f);

        if (Quaternion.Angle(bodyRotation, CamRotation) > rotateThresh)
        {
            bodyCamAllign = false;

        } else if (Mathf.Round(transform.rotation.eulerAngles.y - camTransform.rotation.eulerAngles.y) == 0f)
        {
            bodyCamAllign = true;
        }

        if (!bodyCamAllign)
        {
            rotation = Quaternion.Euler(Vector3.up * camTransform.rotation.eulerAngles.y);
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 5f * Time.deltaTime);
        
    }
}