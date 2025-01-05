using UnityEngine;

public class IKFootPass : MonoBehaviour
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

    Vector3 bodyRootPos;
    float bodyDisp;

    void Start()
    {
        animator = GetComponent<Animator>();
        bodyRootPos = transform.localPosition;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (animator)
        {
            // Left Foot
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, animator.GetFloat("IKLeftFootWeight"));
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, animator.GetFloat("IKLeftFootWeight"));
            animator.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, 1f);

            RaycastHit hitL;
            Ray rayL = new Ray(animator.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up, Vector3.down);

            bool lFloored = Physics.Raycast(rayL, out hitL, distanceToGround + 1f, layerMask);
            if (lFloored)
            {
                if (hitL.transform.tag == "walkable")
                {
                    Vector3 forward = Vector3.ProjectOnPlane(-lFoot.transform.forward, hitL.normal);
                    Vector3 footPosition = hitL.point;
                    footPosition.y += distanceToGround;

                    animator.SetIKHintPosition(AvatarIKHint.LeftKnee, lKnee.transform.position);

                    animator.SetIKPosition(AvatarIKGoal.LeftFoot, footPosition);

                    animator.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(forward, hitL.normal));

                    animator.SetFloat("IKLeftFootWeight", 1f / hitL.distance);
                }
            }
            

            // Right Foot
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, animator.GetFloat("IKRightFootWeight"));
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, animator.GetFloat("IKRightFootWeight"));
            animator.SetIKHintPositionWeight(AvatarIKHint.RightKnee, 1f);

            RaycastHit hitR;
            Ray rayR = new Ray(animator.GetIKPosition(AvatarIKGoal.RightFoot) + Vector3.up, Vector3.down);

            bool rFloored = Physics.Raycast(rayR, out hitR, distanceToGround + 1f, layerMask);
            if (rFloored)
            {
                if (hitR.transform.tag == "walkable")
                {
                    Vector3 forward = Vector3.ProjectOnPlane(-rFoot.transform.forward, hitR.normal);
                    Vector3 footPosition = hitR.point;
                    footPosition.y += distanceToGround;

                    animator.SetIKHintPosition(AvatarIKHint.RightKnee, rKnee.transform.position);

                    animator.SetIKPosition(AvatarIKGoal.RightFoot, footPosition);

                    animator.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.LookRotation(forward, hitR.normal));

                    animator.SetFloat("IKRightFootWeight", 1f / hitR.distance);
                }
            }
            // FIX, current has a ray for each foot starting at a fixed y pos to get the ground distance at each foot

            Ray rayLDist = new Ray(new Vector3 (lFoot.transform.position.x, 1f, lFoot.transform.position.z), Vector3.down);
            Ray rayRDist = new Ray(new Vector3(rFoot.transform.position.x, 1f, rFoot.transform.position.z), Vector3.down);

            RaycastHit rayLDistHit;
            RaycastHit rayRDistHit;

            Physics.Raycast(rayLDist, out rayLDistHit, Mathf.Infinity, layerMask);
            Physics.Raycast(rayRDist, out rayRDistHit, Mathf.Infinity, layerMask);

            bodyDisp = Mathf.Abs(rayLDistHit.distance - rayRDistHit.distance) / 2f;

            Debug.Log(rayLDistHit.distance + "  L      R  " + rayRDistHit.distance);
            transform.localPosition = bodyRootPos - (Vector3.up * bodyDisp);

        }

    }
}
