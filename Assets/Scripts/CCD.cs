using System;
using UnityEngine;

[System.Serializable]
public struct JointLimits
{
    public float minXAngle;
    public float maxXAngle;
    public float minYAngle;
    public float maxYAngle;
    public float minZAngle;
    public float maxZAngle;
}

public class CCD : MonoBehaviour
{
    [SerializeField] private GameObject[] leftJoints;
    [SerializeField] private GameObject[] rightJoints;
    [SerializeField] private GameObject target;

    private GameObject[] joints;
    public Quaternion[] res;
    public JointLimits[] jointLimits;

    private void Start()
    {
        res = new Quaternion[leftJoints.Length];
    }

    public void NewIK(bool leftHand)
    {
        if (leftHand)
            joints = leftJoints;
        else
            joints = rightJoints;

        for (int i = joints.Length - 1; i >= 0; i--)
        {
            Matrix4x4 inverseTranformMatrix = joints[i].transform.localToWorldMatrix.inverse;
            Vector3 endEffectorDirection = Matrix4x4MultTranslation(joints[joints.Length - 1].transform.position, inverseTranformMatrix).normalized;
            Vector3 targetDirection = Matrix4x4MultTranslation(target.transform.position, inverseTranformMatrix).normalized;

            float dotProduct = Vector3.Dot(endEffectorDirection, targetDirection);
            if(dotProduct < 1.0f - 1.0e-6f)
            {
                float rotationAngle = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;
                Vector3 rotationAxis = Vector3.Cross(endEffectorDirection, targetDirection).normalized;
                joints[i].transform.Rotate(rotationAxis, rotationAngle);

                // Vector3 newAngle = joints[i].transform.rotation.eulerAngles;
                // newAngle.x = Mathf.Clamp(newAngle.x, jointLimits[i].minXAngle, jointLimits[i].maxXAngle);
                // newAngle.y = Mathf.Clamp(newAngle.y, jointLimits[i].minYAngle, jointLimits[i].maxYAngle);
                // newAngle.z = Mathf.Clamp(newAngle.z, jointLimits[i].minZAngle, jointLimits[i].maxZAngle);
                // joints[i].transform.eulerAngles = newAngle;
            }
            res[i] = joints[i].transform.localRotation;
        }
    }

    private Vector3 Matrix4x4MultTranslation(Vector3 translation, Matrix4x4 transformationMatrix)
    {
        Vector4 tempVector4 = new Vector4(translation.x, translation.y, translation.z, 1);
        tempVector4 = transformationMatrix * tempVector4;
        translation = new Vector3(tempVector4.x, tempVector4.y, tempVector4.z);
        return translation;
    }
}
