using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine;

public class PlayerTouchWall : MonoBehaviour
{
    [SerializeField] private GameObject targetHandIK = null;
    [SerializeField] private string wallTag = "Wall";
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float shoulderHeight = 1.5f;

    private List<GameObject> wallDetected = new List<GameObject>();
    private SphereCollider wallCollider = null;
    private Animator animator;
    private bool touchWall = false;
    public CCD ccd = null;
    private bool wallOnLeft = false;

    private void Start()
    {
        wallCollider = GetComponent<SphereCollider>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if(!targetHandIK || wallDetected.Count == 0)
            return;

        Vector3 closestPointWall = FindNeareastPoint(GetNearestWall());
        if (closestPointWall == Vector3.zero)
        {
            touchWall = false;
            return; 
        }

        touchWall = true;
        targetHandIK.transform.position = closestPointWall;

        Vector3 directionToTarget = (closestPointWall - transform.position).normalized;
        float dotProduct = Vector3.Dot(transform.right, directionToTarget);
        if (dotProduct > 0)
            wallOnLeft = false;
        else
            wallOnLeft = true;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (!touchWall)
            return;

        ccd.NewIK(wallOnLeft);

        if(wallOnLeft)
        {
            animator.SetBoneLocalRotation(HumanBodyBones.LeftUpperArm, ccd.res[0]);
            animator.SetBoneLocalRotation(HumanBodyBones.LeftLowerArm, ccd.res[1]);
            animator.SetBoneLocalRotation(HumanBodyBones.LeftHand, ccd.res[2]);
            animator.SetBoneLocalRotation(HumanBodyBones.LeftMiddleProximal, ccd.res[3]);
        }
        else
        {
            animator.SetBoneLocalRotation(HumanBodyBones.RightUpperArm, ccd.res[0]);
            animator.SetBoneLocalRotation(HumanBodyBones.RightLowerArm, ccd.res[1]);
            animator.SetBoneLocalRotation(HumanBodyBones.RightHand, ccd.res[2]);
            animator.SetBoneLocalRotation(HumanBodyBones.RightMiddleProximal, ccd.res[3]);
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

        Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right, Vector3.up, Vector3.down };
        Vector3 startPoint = transform.position + new Vector3(0, shoulderHeight, 0);
        foreach(Vector3 direction in directions)
        {
            if(Physics.Raycast(startPoint, direction, out hit, wallCollider.radius, ~wallLayer.value))
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
