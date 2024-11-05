using System;
using System.Collections.Generic;
using UnityEngine;

public class CCD : MonoBehaviour
{
    [SerializeField] private Constraint[] leftArmBone;
    [SerializeField] private Constraint[] rightArmBone;
    
    [SerializeField] private float epsilon = 0.1f;
    [SerializeField] private GameObject target;

    [SerializeField] private bool addConstraint = true;

    public Quaternion[] IK(Limb limb)
    {
        Constraint[] boneUsed = GetBoneToIK(limb);
        Quaternion[] result = new Quaternion[boneUsed.Length];
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
                boneUsed[i].transform.Rotate(rotationAxis, rotationAngle);

                AddConstraint(boneUsed[i]);
            }
            result[i] = boneUsed[i].transform.localRotation;
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

    private void AddConstraint(Constraint boneUsed)
    {
        if (!addConstraint)
            return;

        Vector3 constraintAngle;
        BoneConstraint boneConstraint = boneUsed.GetBoneContraint();

        constraintAngle.x = Mathf.Clamp((boneUsed.transform.localEulerAngles.x + 180) % 360 - 180, boneConstraint.minXAngle, boneConstraint.maxXAngle);
        constraintAngle.y = Mathf.Clamp((boneUsed.transform.localEulerAngles.y + 180) % 360 - 180, boneConstraint.minYAngle, boneConstraint.maxYAngle);
        constraintAngle.z = Mathf.Clamp((boneUsed.transform.localEulerAngles.z + 180) % 360 - 180, boneConstraint.minZAngle, boneConstraint.maxZAngle);
        boneUsed.transform.localEulerAngles = constraintAngle;
    }
}
