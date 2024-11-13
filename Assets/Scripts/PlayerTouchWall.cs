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

    private Quaternion[] fromRotation;
    private Quaternion[] toRotation;
    [SerializeField] private Transform[] leftArmBone;
    [SerializeField] private Transform[] rightArmBone;
    [SerializeField] private float lerpSpeed = .5f;
    [SerializeField] private float lerpCount = 0f;

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
        {
            lerpCount = 0f;
            return;
        }

        if (lerpCount == 0f)
        {
            fromRotation = new Quaternion[leftArmBone.Length];
            for (int i = 0; i < fromRotation.Length; i++)
            {
                if (arm == Limb.LEFT_ARM)
                    fromRotation[i] = leftArmBone[i].localRotation;
                else
                    fromRotation[i] = rightArmBone[i].localRotation;
            }
            toRotation = ikPlayer.IK(arm);

            Debug.Log("from: " + fromRotation[0]);
            Debug.Log("to: " + toRotation[0]);
        }

        Quaternion[] result = new Quaternion[toRotation.Length];

        if (lerpCount < 1f)
        {
            for(int i = 0; i < toRotation.Length; i++)
                result[i] = Quaternion.Slerp(fromRotation[i], toRotation[i], lerpCount * lerpSpeed);

            Debug.Log("lerp: " + result[0]);
            lerpCount += Time.deltaTime;
        }
        else
        {
            result = ikPlayer.IK(arm);
        }


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
}
