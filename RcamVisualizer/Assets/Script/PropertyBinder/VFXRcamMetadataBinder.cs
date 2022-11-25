using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace Rcam2 {

[AddComponentMenu("VFX/Property Binders/Rcam/Metadata Binder")]
[VFXBinder("Rcam/Metadata")]
class VFXRcamMetadataBinder : VFXBinderBase
{
    public string ColorMapProperty
      { get => (string)_colorMapProperty;
        set => _colorMapProperty = value; }

    public string DepthMapProperty
      { get => (string)_depthMapProperty;
        set => _depthMapProperty = value; }

    public string ProjectionVectorProperty
      { get => (string)_projectionVectorProperty;
        set => _projectionVectorProperty = value; }

    public string InverseViewMatrixProperty
      { get => (string)_inverseViewMatrixProperty;
        set => _inverseViewMatrixProperty = value; }

    [VFXPropertyBinding("UnityEngine.Texture2D"), SerializeField]
    ExposedProperty _colorMapProperty = "ColorMap";

    [VFXPropertyBinding("UnityEngine.Texture2D"), SerializeField]
    ExposedProperty _depthMapProperty = "DepthMap";

    [VFXPropertyBinding("UnityEngine.Vector4"), SerializeField]
    ExposedProperty _projectionVectorProperty = "ProjectionVector";

    [VFXPropertyBinding("UnityEngine.Matrix4x4"), SerializeField]
    ExposedProperty _inverseViewMatrixProperty = "InverseViewMatrix";

    public override bool IsValid(VisualEffect component)
      => component.HasTexture(_colorMapProperty) &&
         component.HasTexture(_depthMapProperty) &&
         component.HasVector4(_projectionVectorProperty) &&
         component.HasMatrix4x4(_inverseViewMatrixProperty);

    public override void UpdateBinding(VisualEffect component)
    {
        var recv = Singletons.Receiver;
        var prj = ProjectionUtil.VectorFromReceiver;
        var v2w = Singletons.Receiver.CameraToWorldMatrix;
        component.SetTexture(_colorMapProperty, recv.ColorTexture);
        component.SetTexture(_depthMapProperty, recv.DepthTexture);
        component.SetVector4(_projectionVectorProperty, prj);
        component.SetMatrix4x4(_inverseViewMatrixProperty, v2w);
    }

    public override string ToString()
      => $"Rcam Metadata : {_colorMapProperty}, {_depthMapProperty}";
}

} // namespace Rcam2
