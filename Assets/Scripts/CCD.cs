using System;
using System.Collections.Generic;
using UnityEngine;

public class CCD : MonoBehaviour
{
    [SerializeField] private Constraint[] leftArmBone;
    [SerializeField] private Constraint[] rightArmBone;
    
    [SerializeField] private float epsilon = 0.1f;
    [SerializeField] private GameObject target;
    [SerializeField] private int maxIteration = 20;

    [SerializeField] private bool addConstraint = true;

    public Quaternion[] IK(Limb limb)
    {
        Constraint[] boneUsed = GetBoneToIK(limb);
        Quaternion[] result = new Quaternion[boneUsed.Length];
        
        for (int j = 0; j < maxIteration; j++)
        {
            for(int i = 0; i < boneUsed.Length; i++)
                result[i] = boneUsed[i].transform.localRotation;

            if (CheckIKSolve(boneUsed))
                return result;

            for (int i = boneUsed.Length - 1; i >= 0; i--)
            {
                Matrix4x4 inverseTranformMatrix = boneUsed[i].transform.localToWorldMatrix.inverse;
                Vector3 endEffectorDirection = Matrix4x4MultTranslation(boneUsed[boneUsed.Length - 1].transform.position, inverseTranformMatrix).normalized;
                Vector3 targetDirection = Matrix4x4MultTranslation(target.transform.position, inverseTranformMatrix).normalized;

                float dotProduct = Vector3.Dot(endEffectorDirection, targetDirection);
                if(dotProduct < 1.0f - 1.0e-6f)
                {
                    float rotationAngle = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;
                    Vector3 rotationAxis = Vector3.Cross(endEffectorDirection, targetDirection).normalized;

                    Quaternion rotation = Quaternion.AngleAxis(rotationAngle, rotationAxis);
                    Quaternion newLocalRotation = boneUsed[i].transform.localRotation * rotation;

                    newLocalRotation = AddConstraint(newLocalRotation, boneUsed[i]);

                    boneUsed[i].transform.localRotation = newLocalRotation;
                    result[i] = boneUsed[i].transform.localRotation;
                }
            }
        }
        return result;
    }

    private Vector3 Matrix4x4MultTranslation(Vector3 translation, Matrix4x4 transformationMatrix)
    {
        Vector4 tempVector4 = new Vector4(translation.x, translation.y, translation.z, 1);
        tempVector4 = transformationMatrix * tempVector4;
        translation = new Vector3(tempVector4.x, tempVector4.y, tempVector4.z);
        return translation;
    }

    private Constraint[] GetBoneToIK(Limb limb)
    {
        switch (limb)
        {
            case Limb.LEFT_ARM:
                return leftArmBone;

            case Limb.RIGHT_ARM:
                return rightArmBone;

            default:
                return leftArmBone;
        }
    }

    private bool CheckIKSolve(Constraint[] boneUsed)
    {
        float x = Mathf.Abs(boneUsed[boneUsed.Length - 1].transform.position.x - target.transform.position.x);
        float y = Mathf.Abs(boneUsed[boneUsed.Length - 1].transform.position.y - target.transform.position.y);
        float z = Mathf.Abs(boneUsed[boneUsed.Length - 1].transform.position.z - target.transform.position.z);
        if (x < epsilon && y < epsilon && z < epsilon)
            return true;
        return false;
    }

    private Quaternion AddConstraint(Quaternion rotation, Constraint boneUsed)
    {
        if (!addConstraint)
            return rotation;

        BoneConstraint boneConstraint = boneUsed.GetBoneContraint();

        Vector3 euler = rotation.eulerAngles;
        euler.x = ClampAngle(euler.x, boneConstraint.minXAngle, boneConstraint.maxXAngle);
        euler.y = ClampAngle(euler.y, boneConstraint.minYAngle, boneConstraint.maxYAngle);
        euler.z = ClampAngle(euler.z, boneConstraint.minZAngle, boneConstraint.maxZAngle);

        return Quaternion.Euler(euler);
    }

    float ClampAngle(float angle, float min, float max)
    {
        angle = (angle > 180) ? angle - 360 : angle; // Normalize angle to range [-180, 180]
        return Mathf.Clamp(angle, min, max);
    }
}
