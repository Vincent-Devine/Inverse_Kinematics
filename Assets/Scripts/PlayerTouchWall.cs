using System.Collections.Generic;
using UnityEngine;


public class PlayerTouchWall : MonoBehaviour
{
    [SerializeField] private GameObject playerChest = null;
    [SerializeField] private GameObject playerNeck = null;
    [SerializeField] private GameObject target = null;
    [SerializeField] private CCD ikPlayer = null;
    
    private List<GameObject> wallDetected = new List<GameObject>();
    private string wallTag = "Wall";
    private bool touchWall = false;
    private Limb arm = Limb.LEFT_ARM;
    
    private Animator animator;
    private SphereCollider wallCollider;

    private Vector3 fromPosition;
    private Vector3 basePosition;
    [SerializeField] private Transform[] leftArmBone;
    [SerializeField] private Transform[] rightArmBone;
    [SerializeField] private float lerpSpeed = .5f;
    [SerializeField] private float lerpCount = 0f;
    [SerializeField] private bool startLerpIkToAnim = false;

    private void Start()
    {
        wallCollider = GetComponent<SphereCollider>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if(!target || wallDetected.Count == 0)
            return;

        Vector3 closestPointWall = FindNeareastPoint(GetNearestWall());
        Debug.Log(closestPointWall);
        if (closestPointWall == Vector3.zero)
        {
            // LerpWallToAnimation();
            touchWall = false;
            lerpCount = 0f;
            return; 
        }

        startLerpIkToAnim = false;
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

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag(wallTag))
            wallDetected.Add(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        wallDetected.Remove(other.gameObject);
    }

    private GameObject GetNearestWall()
    {
        if(wallDetected.Count == 1)
            return wallDetected[0];
        
        float nearestWallDistance = float.MaxValue;
        GameObject nearestWall = null;

        foreach(GameObject wall in wallDetected)
        {
            float temp = Vector3.Distance(transform.position, wall.transform.position);
            if(temp < nearestWallDistance)
            {
                nearestWall = wall;
                nearestWallDistance = temp;
            }
        }

        return nearestWall;
    }

    private Vector3 FindNeareastPoint(GameObject wall)
    {
        RaycastHit hit;
        Vector3 neareastPoint = Vector3.zero;
        float closestDistance = float.MaxValue;

        Vector3[] directions = { playerChest.transform.forward, -playerChest.transform.forward, -playerChest.transform.right, playerChest.transform.right, playerChest.transform.up, -playerChest.transform.up };
        Vector3 startPoint = playerNeck.transform.position;
        foreach(Vector3 direction in directions)
        {
            if(Physics.Raycast(startPoint, direction, out hit, wallCollider.radius))
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
        if(closestDistance < 0.4f)
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
                fromPosition = leftArmBone[leftArmBone.Length - 1].position; // hand position
            else
                fromPosition = rightArmBone[rightArmBone.Length - 1].position; // hand position

            basePosition = fromPosition - transform.position;
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
        }

        if (lerpCount > 0.0f)
        {
            target.transform.position = Vector3.Lerp(fromPosition, basePosition + transform.position, lerpCount);
            lerpCount -= Time.deltaTime * lerpSpeed;
        }
        else
        {
            lerpCount = 0;
            touchWall = false;
        }
    }
}
