﻿using System;
using System.Linq;
using droid.Runtime.Utilities.NeodroidCamera.Synthesis;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace droid.Runtime.Utilities.NeodroidCamera.Experimental {
  /// <inheritdoc />
  /// <summary>
  /// </summary>
  [ExecuteInEditMode]
  public class MultiPassCamera : MonoBehaviour {
    /// <summary>
    /// </summary>
    Renderer[] _all_renders = null;

    /// <summary>
    /// </summary>
    MaterialPropertyBlock _block = null;

    [SerializeField] RenderTexture depthRenderTexture=null;
    [SerializeField] RenderTexture objectIdRenderTexture=null;
    [SerializeField] RenderTexture tagIdRenderTexture=null;
    [SerializeField] RenderTexture flowRenderTexture=null;

    /// <summary>
    /// </summary>
    void Start() { this.Setup(); }

    void Awake() {
      //this._asf= new TextureFlipper();
    }

    void CheckBlock() {
      if (this._block == null) {
        this._block = new MaterialPropertyBlock();
      }
    }

    [SerializeField] CapturePassMaterial[] _capture_passes;

    [SerializeField] Camera _camera;
    [SerializeField] Boolean debug = true;
    [SerializeField] Boolean always_re = true;
    [SerializeField] Mesh m_quad;
    [SerializeField] GUISkin gui_style = null;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static Mesh CreateFullscreenQuad()
    {
      var r = new Mesh {
                           vertices = new[] {
                                                         new Vector3(1.0f, 1.0f, 0.0f),
                                                         new Vector3(-1.0f, 1.0f, 0.0f),
                                                         new Vector3(-1.0f, -1.0f, 0.0f),
                                                         new Vector3(1.0f, -1.0f, 0.0f),
                                                     },
                           triangles = new[] {0, 1, 2, 2, 3, 0}
                       };
      r.UploadMeshData(true);
      return r;
    }


    /// <summary>
    /// </summary>
    void Setup() {
      if(!this.gui_style) {
        this.gui_style = Resources.FindObjectsOfTypeAll<GUISkin>().First(a => a.name =="BoundingBox");
      }

      this._all_renders = FindObjectsOfType<Renderer>();
      if (this._capture_passes == null || this._capture_passes.Length == 0 || this.always_re) {
        this._capture_passes = new[] {
                                         new CapturePassMaterial(CameraEvent
                                                                                        .AfterDepthTexture,
                                                                                    BuiltinRenderTextureType
                                                                                        .Depth) {

                                                                                                            _SupportsAntialiasing
                                                                                                                = false,
                                                                                                            _RenderTexture
                                                                                                                = this
                                                                                                                    .depthRenderTexture,

                                                                                                        },
                                         new CapturePassMaterial(CameraEvent
                                                                                        .AfterForwardAlpha,
                                                                                    BuiltinRenderTextureType
                                                                                        .MotionVectors) {

                                                                                                            _SupportsAntialiasing
                                                                                                                = false,
                                                                                                            _RenderTexture
                                                                                                                = this
                                                                                                                    .flowRenderTexture
                                                                                                        },
                                         new CapturePassMaterial(CameraEvent
                                                                     .AfterForwardAlpha,
                                                                                    BuiltinRenderTextureType.None  ) {
                                                                                                            _SupportsAntialiasing
                                                                                                                = false,
                                                                                                            _RenderTexture
                                                                                                                = this
                                                                                                                    .objectIdRenderTexture

                                                                                                            ,
                                                                                                            _TextureId  = Shader.PropertyToID("_TmpFrameBuffer")
                                                                                                        },
                                         new CapturePassMaterial(CameraEvent
                                         .AfterDepthTexture,
                                         BuiltinRenderTextureType.None  ) {
                                         _SupportsAntialiasing
                                         = false,
                                         _RenderTexture
                                             = this
                                                 .tagIdRenderTexture

                                         ,
                                         _TextureId  = Shader.PropertyToID("_CameraDepthTexture")
                                         }
                                     };
      }


      if (this.m_quad == null) {
        this.m_quad = CreateFullscreenQuad();
      }

      this._camera = this.GetComponent<Camera>();
      //this._camera.SetReplacementShader(this.uberMaterial.shader,"");

      this._camera.RemoveAllCommandBuffers(); // cleanup capturing camera

      this._camera.depthTextureMode = DepthTextureMode.Depth | DepthTextureMode.MotionVectors;

      foreach (var capture_pass in this._capture_passes) {
        var cb = new CommandBuffer {name = capture_pass.Source.ToString()};

        cb.Clear();

        if (capture_pass._Material) {
          cb.GetTemporaryRT(capture_pass._TextureId, -1, -1, 0, FilterMode.Point);
          //cb.Blit(capture_pass.Source, capture_pass._RenderTexture, capture_pass._Material);
          cb.Blit(capture_pass.Source, capture_pass._TextureId);
          cb.SetRenderTarget(new RenderTargetIdentifier[] {capture_pass._RenderTexture},
                             capture_pass._RenderTexture);
          cb.DrawMesh(this.m_quad, Matrix4x4.identity, capture_pass._Material, 0, 0);
          cb.ReleaseTemporaryRT(capture_pass._TextureId);
        } else {
          cb.Blit(capture_pass.Source, capture_pass._RenderTexture);

        }


        this._camera.AddCommandBuffer(capture_pass.When, cb);
      }



      this.CheckBlock();
      foreach (var r in this._all_renders) {
        r.GetPropertyBlock(this._block);
        var sm = r.sharedMaterial;
        if (sm) {
          var id = sm.GetInstanceID();
          var color = ColorEncoding.EncodeIdAsColor(id);

          this._block.SetColor(SynthesisUtilities._Shader_MaterialId_Color_Name, color);
          r.SetPropertyBlock(this._block);
        }
      }
    }

    const int _size = 100;
    const int _margin = 20;


    void OnGUI() {
      if (this.debug) {
        var index = 0;

        foreach (var pass in this._capture_passes) {
          var xi = (_size + _margin) * index++;
          var x = xi % (Screen.width - _size);
          var y = (_size + _margin) * (xi / (Screen.width - _size));
          var r = new Rect(_margin + x, _margin + y, _size, _size);
          //this._asf?.Flip(pass._RenderTexture);

          GUI.DrawTexture(r, pass._RenderTexture, ScaleMode.ScaleToFit);
          GUI.TextField(r, pass.Source.ToString(),this.gui_style.box);
        }
      }
    }

    TextureFlipper _asf;
  }


  /// <summary>
  ///
  /// </summary>
  [Serializable]
  public struct CapturePassMaterial {
    public bool _SupportsAntialiasing;
    public bool _NeedsRescale;
    public Material _Material;
    public RenderTexture _RenderTexture;
    public CameraEvent When;
    public BuiltinRenderTextureType Source;
    public int _TextureId;

    public CapturePassMaterial(CameraEvent when = CameraEvent.AfterEverything,
                               BuiltinRenderTextureType source = BuiltinRenderTextureType.CurrentActive) {
      this.When = when;
      this.Source = source;
      this._Material = null;
      this._RenderTexture = null;
      this._SupportsAntialiasing = false;
      this._NeedsRescale = false;
      this._TextureId = 0;
    }
  }


    public class TextureFlipper : IDisposable
    {
      Shader          m_shVFlip;
      Material        m_VFLipMaterial;
      RenderTexture   m_WorkTexture;

      public TextureFlipper()
      {
        this.m_shVFlip = Shader.Find("Neodroid/Experimental/VerticalFlipper");
        if(this.m_shVFlip) {
          this.m_VFLipMaterial = new Material(this.m_shVFlip);
        }
      }

      public void Flip(RenderTexture target)
      {
        if (this.m_WorkTexture == null || this.m_WorkTexture.width != target.width || this.m_WorkTexture.height != target.height)
        {
          UnityHelpers.Destroy(this.m_WorkTexture);
          this.m_WorkTexture = new RenderTexture(target.width, target.height, target.depth, target.format, RenderTextureReadWrite.Linear);
        }
        if(this.m_VFLipMaterial){
        Graphics.Blit( target, this.m_WorkTexture, this.m_VFLipMaterial );
        Graphics.Blit(this.m_WorkTexture, target );
        }
      }

      public void Dispose() {
        UnityHelpers.Destroy(this.m_WorkTexture);
        this.m_WorkTexture = null;
        if (this.m_VFLipMaterial) {
          UnityHelpers.Destroy(this.m_VFLipMaterial);
          this.m_VFLipMaterial = null;
        }
      }

    }


    /// <summary>
    /// What is this:
    /// Motivation  :
    /// Notes:
    /// </summary>
    public static class UnityHelpers
    {
      public static void Destroy(Object obj, bool allowDestroyingAssets = false)
      {
        if (obj == null) {
          return;
        }
        #if UNITY_EDITOR
        if (UnityEditor.EditorApplication.isPlaying) {
          Object.Destroy(obj);
        } else {
          Object.DestroyImmediate(obj, allowDestroyingAssets);
        }
        #else
            Object.Destroy(obj);
        #endif
        obj = null;
      }

      public static bool IsPlaying()
      {
        #if UNITY_EDITOR
        return UnityEditor.EditorApplication.isPlaying;
        #else
            return true;
        #endif
      }
    }
  }

