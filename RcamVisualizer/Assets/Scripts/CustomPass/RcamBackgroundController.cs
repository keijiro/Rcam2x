using UnityEngine;

namespace Rcam2 {

//
// Controller and data provider for RcamBackgroundPass
//
public sealed class RcamBackgroundController : MonoBehaviour
{
    #region Public properties

    public bool IsActive => _opacity.back > 0 || _opacity.front > 0;
    public int PassNumber => _currentEffect;
    public bool BackFill { get; set; }
    public bool FrontFill { get; set; }
    public int EffectNumber { get; set; }
    public float EffectDirection { get; set; }
    public float EffectParameter { get; set; }
    public float EffectIntensity { get; set; }

    #endregion

    #region Private variables

    (float back, float front, float effect) _opacity;
    int _currentEffect;

    #endregion

    #region Shader and material

    [SerializeField] Shader _shader = null;

    Material _material;

    public Material SharedMaterial => UpdateMaterial();

    Material UpdateMaterial()
    {
        if (_material == null) _material = new Material(_shader);

        var oparams = new Vector3(_opacity.back, _opacity.front, _opacity.effect);
        var phi = EffectDirection * Mathf.PI * 2;
        var eparams = new Vector4
          (EffectParameter, EffectIntensity, Mathf.Sin(phi), Mathf.Cos(phi));

        _material.SetVector("_Opacity", oparams);
        _material.SetVector("_EffectParams", eparams);

        return _material;
    }

    #endregion

    #region MonoBehaviour implementation

    void OnDestroy()
      => Destroy(_material);

    void Update()
    {
        var delta = Time.deltaTime * 10;

        // BG opacity animation
        var dir = BackFill ? 1 : -1;
        _opacity.back = Mathf.Clamp01(_opacity.back + dir * delta);

        // FG opacity animation
        dir = FrontFill ? 1 : -1;
        _opacity.front = Mathf.Clamp01(_opacity.front + dir * delta);

        // Effect opacity animation
        dir = _currentEffect == EffectNumber ? 1 : -1;
        _opacity.effect = Mathf.Clamp01(_opacity.effect + dir * delta);

        // We can switch the effect when the opacity becomes zero.
        if (_currentEffect != EffectNumber && _opacity.effect == 0)
            _currentEffect = EffectNumber;
    }

    #endregion
}

} // namespace Rcam2
