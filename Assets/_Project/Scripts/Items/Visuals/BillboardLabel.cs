using UnityEngine;

public class BillboardLabel : MonoBehaviour
{
    Transform cameraTransform;

    void LateUpdate()
    {
        if (cameraTransform == null)
        {
            var cam = Camera.main;
            if (cam == null)
            {
                return;
            }

            cameraTransform = cam.transform;
        }

        Vector3 forward = cameraTransform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f)
        {
            forward = Vector3.forward;
        }
        else
        {
            forward.Normalize();
        }

        transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }
}
