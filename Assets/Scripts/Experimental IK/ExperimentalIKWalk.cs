using Unity.VisualScripting;
using UnityEngine;

public class ExperimentalIKWalk : MonoBehaviour
{
    Animator animator;

    public LayerMask layerMask;

    [Range(0f, 1f)]
    public float distanceToGround = 0.07f;

    [SerializeField]
    GameObject rKnee;
    [SerializeField]
    GameObject lKnee;
    [SerializeField]
    GameObject lFoot;
    [SerializeField]
    GameObject rFoot;

    [SerializeField]
    private float sideOffset;
    [SerializeField]
    private float forwardOffset;

    [SerializeField]
    private float stepDistance;

    private float lerpL;
    private float lerpR;

    [SerializeField]
    private float legSpeed;
    [SerializeField]
    private float stepHeight;

    Vector3 newPositionL;
    Vector3 newPositionR;

    Vector3 oldPositionL;
    Vector3 oldPositionR;

    Vector3 bodyRootPos;
    float bodyDisp;

    void Start()
    {
        animator = GetComponent<Animator>();
        bodyRootPos = transform.localPosition;
    }

    private void Update()
    {
        
    }

    private void OnAnimatorIK(int layerIndex)
    {
        // Left Foot
        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);
        animator.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, 1f);
        animator.SetIKHintPosition(AvatarIKHint.LeftKnee, lKnee.transform.position);

        animator.SetIKPosition(AvatarIKGoal.LeftFoot, oldPositionL);

        Ray rayL = new Ray(transform.position + Vector3.up - (transform.right * sideOffset) + (transform.forward * forwardOffset), Vector3.down);
        if (Physics.Raycast(rayL, out RaycastHit hitL, 2f, layerMask))
        {
            if (Vector3.Distance(newPositionL, hitL.point) > stepDistance)
            {
                lerpL = 0;
                newPositionL = hitL.point;
            }
        }
        if (lerpL < 1)
        {
            Vector3 footPosition = Vector3.Lerp(oldPositionL, newPositionL, lerpL);
            footPosition.y += Mathf.Sin(lerpL * Mathf.PI) * stepHeight;

            animator.SetIKPosition(AvatarIKGoal.LeftFoot, footPosition);
            lerpL += Time.deltaTime * legSpeed;
        }
        else
        {
            oldPositionL = newPositionL;
        }




        /*
        if (moveLFoot)
        {
            //animator.SetIKPosition(AvatarIKGoal.LeftFoot, lFootTargetVector);
            animator.SetIKPosition(AvatarIKGoal.LeftFoot, Vector3.Slerp(lFootStick, lFootTargetVector, legSpeed * Time.deltaTime));

            if (Vector3.Distance(lFootStick, lFootTargetVector) <= 0.01f)
            {
                moveLFoot = false;
            }
        }
        else
        {
            animator.SetIKPosition(AvatarIKGoal.LeftFoot, lFootTargetVector);
        }

        */

        // Right Foot
        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
        animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);
        animator.SetIKHintPositionWeight(AvatarIKHint.RightKnee, 1f);
        animator.SetIKHintPosition(AvatarIKHint.RightKnee, rKnee.transform.position);


    }
}
