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
        if (closestPointWall == Vector3.zero)
        {
            touchWall = false;
            return; 
        }

        touchWall = true;
        target.transform.position = closestPointWall;

        Vector3 directionToTarget = (closestPointWall - transform.position).normalized;
        float dotProduct = Vector3.Dot(transform.right, directionToTarget);
        
        if (dotProduct > 0)
            arm = Limb.RIGHT_ARM; // Wall on right
        else
            arm = Limb.LEFT_ARM;  // Wall on left
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (!touchWall)
            return;

        Quaternion[] rotation = ikPlayer.IK(arm);

        if (arm == Limb.LEFT_ARM)
        {
            animator.SetBoneLocalRotation(HumanBodyBones.LeftUpperArm, rotation[0]);
            animator.SetBoneLocalRotation(HumanBodyBones.LeftLowerArm, rotation[1]);
            animator.SetBoneLocalRotation(HumanBodyBones.LeftHand, rotation[2]);
        }
        else // Right arm
        {
            animator.SetBoneLocalRotation(HumanBodyBones.RightUpperArm, rotation[0]);
            animator.SetBoneLocalRotation(HumanBodyBones.RightLowerArm, rotation[1]);
            animator.SetBoneLocalRotation(HumanBodyBones.RightHand, rotation[2]);
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

        return neareastPoint;
    }
}
