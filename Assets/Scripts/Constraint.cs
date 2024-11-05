using UnityEngine;

[System.Serializable]
public struct BoneConstraint
{
    public float minXAngle;
    public float maxXAngle;
    public float minYAngle;
    public float maxYAngle;
    public float minZAngle;
    public float maxZAngle;
}

public class Constraint : MonoBehaviour
{
    [SerializeField] private BoneConstraint boneConstraint;
    
    public BoneConstraint GetBoneContraint()
    {
        return boneConstraint;
    }
};
