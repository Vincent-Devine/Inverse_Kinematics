using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


public class PlayerTouchWall : MonoBehaviour
{
    [SerializeField] private GameObject playerChest = null;
    [SerializeField] private GameObject playerNeck = null;
    [SerializeField] private GameObject target = null;
    [SerializeField] private CCD ikPlayer = null;
    
    [SerializeField] private float maxSizeTouchWall = .65f;
    [SerializeField] private float minSizeTouchWall = .4f;
    private bool touchWall = false;
    private Limb arm = Limb.LEFT_ARM;

    private Animator animator;

    [SerializeField] private Transform leftHandBone;
    [SerializeField] private Transform rightHandBone;
    [SerializeField] private Transform leftBasePosHand;
    [SerializeField] private Transform rightBasePosHand;
    [SerializeField] private float lerpSpeed = .5f;
    private Vector3 fromPosition;
    private float lerpCount = 0f;
    private bool startLerpIkToAnim = false;
    private bool needToLerpIkToAnim = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        Vector3 closestPointWall = FindNeareastPoint();
        if (closestPointWall == Vector3.zero)
        {
            if(needToLerpIkToAnim)
                LerpWallToAnimation();
            return; 
        }

        startLerpIkToAnim = false;
        needToLerpIkToAnim = true;
        touchWall = true;
        SetRightHand(closestPointWall);
        LerpAnimationToWall(closestPointWall);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (!touchWall)
            return;
        
        Quaternion[] result = ikPlayer.IK(arm);

        if (arm == Limb.LEFT_ARM)
        {
            animator.SetBoneLocalRotation(HumanBodyBones.LeftUpperArm, result[0]);
            animator.SetBoneLocalRotation(HumanBodyBones.LeftLowerArm, result[1]);
            animator.SetBoneLocalRotation(HumanBodyBones.LeftHand, result[2]);
        }
        else // Right arm
        {
            animator.SetBoneLocalRotation(HumanBodyBones.RightUpperArm, result[0]);
            animator.SetBoneLocalRotation(HumanBodyBones.RightLowerArm, result[1]);
            animator.SetBoneLocalRotation(HumanBodyBones.RightHand, result[2]);
        }
    }

    private Vector3 FindNeareastPoint()
    {
        RaycastHit hit;
        Vector3 neareastPoint = Vector3.zero;
        float closestDistance = float.MaxValue;

        Vector3[] directions = { playerChest.transform.forward, -playerChest.transform.forward, -playerChest.transform.right, playerChest.transform.right, playerChest.transform.up, -playerChest.transform.up };
        Vector3 startPoint = playerNeck.transform.position;
        foreach(Vector3 direction in directions)
        {
            if(Physics.Raycast(startPoint, direction, out hit, maxSizeTouchWall))
            {
                float temp = Vector3.Distance(startPoint, hit.point);
                if(temp < closestDistance)
                {
                    closestDistance = temp;
                    neareastPoint = hit.point;
                }
            }
        }

        // Don't touch the wall is too near the wall
        if(closestDistance < minSizeTouchWall)
            return Vector3.zero;

        return neareastPoint;
    }

    private void SetRightHand(Vector3 position)
    {
        Vector3 directionToTarget = (position - transform.position).normalized;
        float dotProduct = Vector3.Dot(transform.right, directionToTarget);

        if (dotProduct > 0)
            arm = Limb.RIGHT_ARM; // Wall on right
        else
            arm = Limb.LEFT_ARM;  // Wall on left
    }

    private void LerpAnimationToWall(Vector3 closestPointWall)
    {
        if (lerpCount == 0f)
        {
            if (arm == Limb.LEFT_ARM)
                fromPosition = leftHandBone.position;
            else
                fromPosition = rightHandBone.position;
        }

        if (lerpCount < 1f)
        {
            target.transform.position = Vector3.Lerp(fromPosition, closestPointWall, lerpCount);
            lerpCount += Time.deltaTime * lerpSpeed;
        }
        else
        {
            target.transform.position = closestPointWall;
        }
    }

    private void LerpWallToAnimation()
    {
        if (!startLerpIkToAnim)
        {
            fromPosition = target.transform.position; // hand position
            startLerpIkToAnim = true;
            lerpCount = 0f;
        }

        if (lerpCount < 1f)
        {
            Vector3 toPosition = rightBasePosHand.position;
            if(arm == Limb.LEFT_ARM)
                toPosition = leftBasePosHand.position;

            target.transform.position = Vector3.Lerp(fromPosition, toPosition, lerpCount);
            lerpCount += Time.deltaTime * lerpSpeed;
        }
        else
        {
            lerpCount = 0;
            touchWall = false;
            startLerpIkToAnim = false;
            needToLerpIkToAnim = false;
        }
    }
}
