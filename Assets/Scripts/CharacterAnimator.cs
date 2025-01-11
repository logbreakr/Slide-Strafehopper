using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.ProBuilder;

public class CharacterAnimator : MonoBehaviour
{
    Rigidbody parentRb;
    Transform camTransform;
    Animator animator;

    float rotateThresh = 30;
    bool bodyCamAllign = false;

    private bool isCrouching, isSliding;

    void Start()
    {
        parentRb = transform.parent.GetComponent<Rigidbody>();
        camTransform = transform.parent.Find("CamHolder").transform;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        isCrouching = transform.parent.GetComponent<PlayerController>().isCrouching;
        isSliding = transform.parent.GetComponent<PlayerController>().isSliding;

        animator.SetBool("IsCrouching", isCrouching);
        animator.SetBool("IsSliding", isSliding);

        // animation control
        Vector3 velocity = Vector3.zero;

        if (transform.parent.parent != null)
        {
            velocity = parentRb.linearVelocity - transform.parent.parent.GetComponent<Rigidbody>().GetPointVelocity(transform.parent.transform.position);
        }
        else
        {
            velocity = parentRb.linearVelocity;
        }

        velocity = Vector3.ProjectOnPlane(velocity, Vector3.up);

        float speed = velocity.magnitude / 11;
        animator.SetFloat("Speed", speed);

        
        // rotation control
        Quaternion rotation = transform.rotation;

        Quaternion bodyRotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
        Quaternion CamRotation = Quaternion.Euler(0f, camTransform.rotation.eulerAngles.y, 0f);

        if (!animator.GetBool("IsSliding"))
        {
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

        // sliding rotate to normal
        if (animator.GetBool("IsSliding"))
        {
            if (transform.parent.GetComponent<PlayerController>().IsGrounded(false, 0.6f))
            {
                Vector3 normal = transform.parent.GetComponent<PlayerController>().Normal(transform.parent.transform.position);

                transform.rotation = Quaternion.LookRotation(Vector3.Cross(Quaternion.Euler(0f, 90f, 0f) * parentRb.linearVelocity.normalized, normal));
            }
            else
            {
                transform.rotation = Quaternion.LookRotation(Vector3.Cross(Quaternion.Euler(0f, 90f, 0f) * parentRb.linearVelocity.normalized, Vector3.up));
            }
        }
    }
}