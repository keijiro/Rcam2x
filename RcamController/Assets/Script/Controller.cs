using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Klak.Ndi;

namespace Rcam2 {

sealed class Controller : MonoBehaviour
{
    #region External scene object references

    [Space]
    [SerializeField] Camera _camera = null;
    [SerializeField] ARCameraManager _cameraManager = null;
    [SerializeField] ARCameraBackground _cameraBackground = null;
    [SerializeField] AROcclusionManager _occlusionManager = null;
    [SerializeField] InputHandle _input = null;

    #endregion

    #region Editable parameters

    [Space]
    [SerializeField] float _minDepth = 0.2f;
    [SerializeField] float _maxDepth = 3.2f;

    #endregion

    #region Hidden external asset references

    [SerializeField, HideInInspector] NdiResources _ndiResources = null;
    [SerializeField, HideInInspector] Shader _shader = null;

    #endregion

    #region Internal objects

    const int _width = 2048;
    const int _height = 1024;

    NdiSender _ndiSender;
    Matrix4x4 _projection;
    RenderTexture _senderRT;
    (Material bg, Material mux) _material;

    #endregion

    #region Internal methods

    Metadata MakeMetadata()
      => new Metadata { CameraPosition = _camera.transform.position,
                        CameraRotation = _camera.transform.rotation,
                        ProjectionMatrix = _projection,
                        DepthRange = new Vector2(_minDepth, _maxDepth),
                        InputState = _input.InputState };

    #endregion

    #region Public method (UI callback)

    public void ResetOrigin()
      => _camera.transform.parent.position = -_camera.transform.localPosition;

    #endregion

    #region Camera callbacks

    void OnCameraFrameReceived(ARCameraFrameEventArgs args)
    {
        // We expect there is at least one texture.
        if (args.textures.Count == 0) return;

        // Try receiving Y/CbCr textures.
        for (var i = 0; i < args.textures.Count; i++)
        {
            var id = args.propertyNameIds[i];
            var tex = args.textures[i];
            if (id == ShaderID.TextureY)
                _material.mux.SetTexture(ShaderID.TextureY, tex);
            else if (id == ShaderID.TextureCbCr)
                _material.mux.SetTexture(ShaderID.TextureCbCr, tex);
        }

        // Try receiving the projection matrix.
        if (args.projectionMatrix.HasValue)
        {
            _projection = args.projectionMatrix.Value;

            // Aspect ratio compensation (camera vs. 16:9)
            _projection[1, 1] *= (16.0f / 9) / _camera.aspect;
        }

        // Use the first texture to calculate the source texture aspect ratio.
        var tex1 = args.textures[0];
        var texAspect = (float)tex1.width / tex1.height;

        // Aspect ratio compensation factor for the multiplexer
        var aspectFix = texAspect / (16.0f / 9);
        _material.bg.SetFloat(ShaderID.AspectFix, aspectFix);
        _material.mux.SetFloat(ShaderID.AspectFix, aspectFix);
    }

    void OnOcclusionFrameReceived(AROcclusionFrameEventArgs args)
    {
        // Try receiving stencil/depth textures.
        for (var i = 0; i < args.textures.Count; i++)
        {
            var id = args.propertyNameIds[i];
            var tex = args.textures[i];
            if (id == ShaderID.HumanStencil)
                _material.mux.SetTexture(ShaderID.HumanStencil, tex);
            else if (id == ShaderID.EnvironmentDepth)
                _material.mux.SetTexture(ShaderID.EnvironmentDepth, tex);
        }
    }

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        // Shader setup
        _material = (new Material(_shader), new Material(_shader));
        _material.bg.EnableKeyword("RCAM_MONITOR");
        _material.mux.EnableKeyword("RCAM_MULTIPLEXER");

        // Custom background material
        _cameraBackground.customMaterial = _material.bg;
        _cameraBackground.useCustomMaterial = true;

        // Render texture for the NDI source
        _senderRT = new RenderTexture(_width, _height, 0);
        _senderRT.wrapMode = TextureWrapMode.Clamp;
        _senderRT.Create();

        // NDI sender instantiation
        _ndiSender = gameObject.AddComponent<NdiSender>();
        _ndiSender.SetResources(_ndiResources);
        _ndiSender.ndiName = "Rcam";
        _ndiSender.captureMethod = CaptureMethod.Texture;
        _ndiSender.sourceTexture = _senderRT;
    }

    void OnDestroy()
    {
        Destroy(_material.bg);
        Destroy(_material.mux);
        Destroy(_senderRT);
    }

    void OnEnable()
    {
        // Camera callback setup
        _cameraManager.frameReceived += OnCameraFrameReceived;
        _occlusionManager.frameReceived += OnOcclusionFrameReceived;
    }

    void OnDisable()
    {
        // Camera callback termination
        _cameraManager.frameReceived -= OnCameraFrameReceived;
        _occlusionManager.frameReceived -= OnOcclusionFrameReceived;
    }

    void Update()
    {
        // Parameter update
        var range = new Vector2(_minDepth, _maxDepth);
        _material.bg.SetVector(ShaderID.DepthRange, range);
        _material.mux.SetVector(ShaderID.DepthRange, range);

        // NDI sender RT update
        Graphics.Blit(null, _senderRT, _material.mux, 0);
    }

    //
    // Update the NDI metadata in OnRenderObject
    //
    // The camera transform is updated in Application.onBeforeRender, so we
    // have to update the metadata (containing the camera transform data) after
    // it. Also we have to do it before the NDI sender update that happens in
    // WaitForEndOfFrame.
    //
    // It may look strange to do this here, but this is the most
    // straightforward way to collect the latest data.
    //
    void OnRenderObject()
      => _ndiSender.metadata = MakeMetadata().Serialize();

    #endregion
}

} // namespace Rcam2
