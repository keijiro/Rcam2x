using UnityEngine;

namespace Rcam2 {

sealed class RcamRawImageSetter : MonoBehaviour
{
    Material _material;

    void Start()
      => _material = GetComponent<MeshRenderer>().material;

    void Update()
      => _material.mainTexture = Singletons.Receiver.ColorTexture;
}

} // namespace Rcam2
