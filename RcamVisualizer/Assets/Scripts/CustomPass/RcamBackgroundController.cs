using UnityEngine;

namespace Rcam2 {

// Controller and data provider for RcamBackgroundPass
public sealed class RcamBackgroundController : MonoBehaviour
{
    #region Public members

    public bool IsActive => BackFill || FrontFill;
    public int PassNumber => EffectNumber;
    public Material SharedMaterial => UpdateMaterial();

    public bool BackFill { get; set; }
    public bool FrontFill { get; set; }
    public int EffectNumber { get; set; }
    public float EffectDirection { get; set; }

    #endregion

    #region Shader and material

    [SerializeField] Shader _shader = null;

    Material _material;

    Material UpdateMaterial()
    {
        if (_material == null) _material = new Material(_shader);

        var phi = EffectDirection * Mathf.PI * 2;
        var direction = new Vector2(Mathf.Sin(phi), Mathf.Cos(phi));
        var opacity = new Vector2(BackFill ? 1 : 0, FrontFill ? 1 : 0);

        _material.SetVector("_Direction", direction);
        _material.SetVector("_Opacity", opacity);

        return _material;
    }

    #endregion

    #region MonoBehaviour implementation

    void OnDestroy()
      => Destroy(_material);

    #endregion
}

} // namespace Rcam2
