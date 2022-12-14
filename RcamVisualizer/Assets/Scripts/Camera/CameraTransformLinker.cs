using UnityEngine;

namespace Rcam2 {

sealed class CameraTransformLinker : MonoBehaviour
{
    void LateUpdate()
    {
        var recv = Singletons.Receiver;
        transform.position = recv.CameraPosition;
        transform.rotation = recv.CameraRotation;
    }
}

} // namespace Rcam2
