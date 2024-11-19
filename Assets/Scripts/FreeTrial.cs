using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;

public class FreeTrial : MonoBehaviour
{
    [SerializeField] private CCD ikPlayer = null;
    [SerializeField] private Limb arm;

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnAnimatorIK(int layerIndex)
    {
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
}
