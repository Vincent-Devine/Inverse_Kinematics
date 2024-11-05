using UnityEngine;

public class CopyTransform : MonoBehaviour
{
    [SerializeField] private Transform transformToCopy;

    void Start()
    {
        transform.localPosition = transformToCopy.localPosition;
        transform.localRotation = transformToCopy.localRotation;
        transform.localScale = transformToCopy.localScale;
    }
}
