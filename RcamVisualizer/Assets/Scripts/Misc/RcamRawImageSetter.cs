using UnityEngine;
using UnityEngine.UI;

namespace Rcam2 {

sealed class RcamRawImageSetter : MonoBehaviour
{
    RawImage _ui;

    void Start()
      => _ui = GetComponent<RawImage>();

    void Update()
      => _ui.texture = Singletons.Receiver.ColorTexture;
}

} // namespace Rcam2
