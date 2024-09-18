using UnityEngine;

public class CCD : MonoBehaviour
{
    [SerializeField] private GameObject[] leftJoints;
    [SerializeField] private GameObject[] rightJoints;
    [SerializeField] private GameObject target;

    private GameObject[] joints;
    [SerializeField] private float[] theta;
    [SerializeField] private float[] sin;
    [SerializeField] private float[] cos;

    public Quaternion[] res;

    [SerializeField] private float epsilon = .1f;

    private void Start()
    {
        theta = new float[leftJoints.Length];
        sin = new float[leftJoints.Length];
        cos = new float[leftJoints.Length];
        res = new Quaternion[leftJoints.Length];
    }

    public void IK(bool leftHand)
    {
        if (leftHand)
            joints = leftJoints;
        else
            joints = rightJoints;

        if (CheckIKSolve())
            return;

        // Start at -2 because the last joints doesn't move
        for(int i = joints.Length - 1; i >= 0; i--)
        {
            // vector from the current joint to the last
            Vector3 r1 = joints[joints.Length - 1].transform.position - joints[i].transform.position;
            // vector from the current joint to the target
            Vector3 r2 = target.transform.position - joints[i].transform .position;
        
            float totalMagnitude = r1.magnitude * r2.magnitude;
            if (totalMagnitude < .00001f) // avoid divide by tiny value
            {
                cos[i] = 1;
                sin[i] = 0;
            }
            else
            {
                cos[i] = Vector3.Dot(r1, r2) / totalMagnitude;
                sin[i] = Vector3.Cross(r1, r2).magnitude / totalMagnitude;
            }
        
            Vector3 axis = Vector3.Cross(r1, r2) / totalMagnitude;
        
            // find the angle between r1 and r2
            // clamp value between -1 and 1 (normally already between)
            theta[i] = Mathf.Acos(Mathf.Clamp(cos[i], -1.0f, 1.0f));
        
            if (sin[i] < 0.0f)
                theta[i] = -theta[i];
        
            theta[i] = (float)SimpleAngle(theta[i]) * Mathf.Rad2Deg;
            joints[i].transform.Rotate(axis, theta[i], Space.World);
        
            res[i] = joints[i].transform.localRotation;
        }
    }

    // convert angle to simplest form bewteen -pi and pi
    private double SimpleAngle(double theta)
    {
        theta = theta % (2.0 *  Mathf.PI);
        if (theta < -Mathf.PI)
            theta += 2.0 * Mathf.PI;
        else if(theta > Mathf.PI)
            theta -= 2.0 * Mathf.PI;

        return theta;
    }

    private bool CheckIKSolve()
    {
        float x = Mathf.Abs(joints[joints.Length - 1].transform.position.x - target.transform.position.x);
        float y = Mathf.Abs(joints[joints.Length - 1].transform.position.y - target.transform.position.y);
        float z = Mathf.Abs(joints[joints.Length - 1].transform.position.z - target.transform.position.z);

        if (x < epsilon && y < epsilon && z < epsilon)
            return true;
        return false;
    }
}
