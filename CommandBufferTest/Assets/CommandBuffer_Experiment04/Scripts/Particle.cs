using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.Rendering;

namespace Demo04
{
    struct ParticleData
    {
        public Vector3 pos;
        public ParticleData(Vector3 pos)
        {
            this.pos = pos;
        }
    };
}

namespace Demo04
{
    public class Particle : MonoBehaviour
    {
        [SerializeField] ComputeShader kernel;
        [SerializeField] Shader _depthShader;
        [SerializeField] Shader _blurShader;
        [SerializeField] Shader _calcNormalShader;
        [SerializeField] Shader _renderGBufferShader;
        Material _depthMat;
        Material _blurMat;
        Material _renderGBufferMat;
        Material _calcNormalMat;

        [SerializeField] int particleNum;
        int kernelID;
        
        public ComputeBuffer buffer;
        [SerializeField] float speed;



        //----------------------
        //Shader-Parameters
        [SerializeField] float _blurFilterRadius;
        [SerializeField] float _blurScale;
        [SerializeField] float _blurDepthFallOff;
        //----------------------
        //command buffer
        CommandBuffer cmd;
        Camera cam;
        Mesh quad;
        [SerializeField] float particleSize = 0.1f;

        private struct CmdBufferInfo{
            public CameraEvent pass;
            public CommandBuffer buffer;
        };
        Dictionary<Camera, CmdBufferInfo> _cameras = new Dictionary<Camera, CmdBufferInfo>();
        //----------------------
        public Color _diffuse = new Color(0.0f, 0.2f, 1.0f, 1.0f);
        public Color _specular = new Color(0.25f, 0.25f, 0.25f, 0.25f);
        public Color _emission = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        public float _roughness = 0.5f;


        private void OnEnable()
        {
            CleanUp();
            CreateMaterial(ref _depthMat, _depthShader);
            CreateMaterial(ref _renderGBufferMat, _renderGBufferShader);
            CreateMaterial(ref _blurMat, _blurShader);
            CreateMaterial(ref _calcNormalMat, _calcNormalShader);
        }

        void Start()
        {
            CreateComputeBuffer();
            kernelID = 0;
            quad = GenerateQuad();

            //CommandBuffer
            //_depthMat = new Material(_depthShader);
            //_renderGBufferMat = new Material(_renderGBufferShader);

            cam = Camera.main;
            cmd = new CommandBuffer();
            cmd.name = "***SSFR_Deffered***";
            cam.AddCommandBuffer(CameraEvent.BeforeGBuffer, cmd);
            
        }

        void Update()
        {
            kernel.SetBuffer(kernelID, "buffer", buffer);
            kernel.SetFloat("speed", speed);
            kernel.Dispatch(kernelID, 128, 1, 1);
        }


        private void OnWillRenderObject()
        {

            var cam = Camera.current;

            if(!_cameras.ContainsKey(cam))
            {
                //現在レンダリングしているカメラにCommandBufferがない場合 
                var cmdInfo = new CmdBufferInfo();
                cmdInfo.pass = CameraEvent.BeforeGBuffer;
                cmdInfo.buffer = new CommandBuffer();
                cmdInfo.buffer.name = "** SSFR Command Buffer **";
                cam.AddCommandBuffer(cmdInfo.pass, cmdInfo.buffer);
                _cameras.Add(cam, cmdInfo);
            } else
            {
                var buf = _cameras[cam].buffer;
                buf.Clear();
                //render particle----------------
                #region RenderDepth_Block
                int depthBufferId = Shader.PropertyToID("-DepthBuffer_SSFR");
                buf.GetTemporaryRT(depthBufferId, -1, -1, 24, FilterMode.Point, RenderTextureFormat.RFloat);

                buf.SetRenderTarget
                    (
                        new RenderTargetIdentifier(depthBufferId),  // デプス
                        new RenderTargetIdentifier(depthBufferId)   // デプス書き込み用
                    );
                buf.ClearRenderTarget(true, true, Color.clear);

                _depthMat.SetBuffer("particles", buffer);
                _depthMat.SetFloat("particleSize", particleSize);
                buf.DrawProcedural(Matrix4x4.identity, _depthMat, 0, MeshTopology.Points, particleNum);
                #endregion

                #region Blur_Block
                /*
                string TempDepthStr = "_TempDepthbufferId";
                var tempDepthBufferId = Shader.PropertyToID(TempDepthStr);
                //blur
                _blurMat.SetFloat("_FilterRadius", _blurFilterRadius);
                _blurMat.SetFloat("_BlurScale", _blurScale);
                _blurMat.SetFloat("_BlurDepthFallOff", _blurDepthFallOff);
                buf.SetGlobalVector("_BlurDir", new Vector2(0.0f, 1.0f));
                buf.Blit(depthBufferId, tempDepthBufferId, _blurMat);
                buf.SetGlobalVector("_BlurDir", new Vector2(1.0f, 0.0f));
                buf.Blit(tempDepthBufferId, depthBufferId, _blurMat);
                */

                
                var tempDepthBufferId = Shader.PropertyToID("_TempDepthBufferId");
                buf.GetTemporaryRT(tempDepthBufferId, -1, -1, 0, FilterMode.Trilinear, RenderTextureFormat.RFloat);

                // ブラーエフェクトのプロパティをセット
                _blurMat.SetFloat("_FilterRadius", _blurFilterRadius);
                _blurMat.SetFloat("_BlurScale", _blurScale);
                _blurMat.SetFloat("_BlurDepthFallOff", _blurDepthFallOff);

                // 水平方向のブラーを実行
                buf.SetGlobalVector("_BlurDir", new Vector2(0.0f, 1.0f));
                buf.Blit(depthBufferId, tempDepthBufferId, _blurMat);

                // 垂直方向のブラーを実行
                buf.SetGlobalVector("_BlurDir", new Vector2(1.0f, 0.0f));
                buf.Blit(tempDepthBufferId, depthBufferId, _blurMat);


                #endregion

                #region calcNormal

                //int normalBufferId = Shader.PropertyToID("_NormalBuffer");
                //buf.GetTemporaryRT(normalBufferId, -1, -1, 0, FilterMode.Point, RenderTextureFormat.ARGBFloat);
                //buf.SetRenderTarget(normalBufferId);
                //buf.ClearRenderTarget(false, true, Color.clear);
                //var view = cam.worldToCameraMatrix;
                //_calcNormalMat.SetMatrix("_ViewMatrix", view);
                //buf.SetGlobalTexture("_DepthBuffer", depthBufferId);
                //buf.Blit(null, normalBufferId, _calcNormalMat);

                int normalBufferId = Shader.PropertyToID("_NormalBuffer");
                buf.GetTemporaryRT(normalBufferId, -1, -1, 0, FilterMode.Point, RenderTextureFormat.ARGBFloat);
                // 法線バッファをレンダーターゲットにセット
                buf.SetRenderTarget(normalBufferId);
                // クリア
                buf.ClearRenderTarget(false, true, Color.clear);
                // ビュー変換マトリックスをセット
                var view = cam.worldToCameraMatrix;
                _calcNormalMat.SetMatrix("_ViewMatrix", view);
                // デプスバッファをシェーダにセット
                buf.SetGlobalTexture("_DepthBuffer", depthBufferId);
                // デプスから法線をスクリーンスペースで計算
                buf.Blit(null, normalBufferId, _calcNormalMat);

                #endregion
                //write gbuffer------------------
                _renderGBufferMat.SetColor("_Diffuse", _diffuse);
                _renderGBufferMat.SetColor("_Specular", new Vector4(_specular.r, _specular.g, _specular.b, 1.0f - _roughness));
                _renderGBufferMat.SetColor("_Emission", _emission);
                buf.SetGlobalTexture("_DepthBuffer", depthBufferId);
                buf.SetGlobalTexture("_NormalBuffer", normalBufferId);
                buf.SetRenderTarget
                    (
                        new RenderTargetIdentifier[4]
                        {
                        BuiltinRenderTextureType.GBuffer0, // Diffuse
                        BuiltinRenderTextureType.GBuffer1, // Specular + Roughness
                        BuiltinRenderTextureType.GBuffer2, // World Normal
                        BuiltinRenderTextureType.GBuffer3  // Emission
                        },
                        BuiltinRenderTextureType.CameraTarget  // Depth
                    );

                buf.DrawMesh(quad, Matrix4x4.identity, _renderGBufferMat);
                buf.ReleaseTemporaryRT(depthBufferId);
                buf.ReleaseTemporaryRT(tempDepthBufferId);
                buf.ReleaseTemporaryRT(normalBufferId);
            }
            
        }


        void CreateComputeBuffer()
        {
            buffer = new ComputeBuffer(particleNum, Marshal.SizeOf(typeof(ParticleData)));
            ParticleData[] particleData = new ParticleData[particleNum];
            for(int i = 0; i < particleNum; i++) {
                particleData[i] = new ParticleData(new Vector3(Random.Range(-10.0f, 10.0f),
                    Random.Range(-10.0f, 10.0f),
                    Random.Range(-5.0f, 5.0f)));
            }
            buffer.SetData(particleData);
        }

        Mesh GenerateQuad()
        {
            var mesh = new Mesh();
            mesh.vertices = new Vector3[4]
            {
                new Vector3( 1.0f,  1.0f, 0.0f),
                new Vector3(-1.0f,  1.0f, 0.0f),
                new Vector3(-1.0f, -1.0f, 0.0f),
                new Vector3( 1.0f, -1.0f, 0.0f),
            };
            mesh.triangles = new int[6] { 0, 1, 2, 2, 3, 0 };
            return mesh;
        }

        //---------------------------
        public void CleanUp()
        {
            //cam, cmdBufferInfo
            foreach(var pair in _cameras)
            {
                var cam = pair.Key;
                var info = pair.Value;
                if(cam)
                {
                    cam.RemoveCommandBuffer(info.pass, info.buffer);
                }
            }
            _cameras.Clear();
            DestroyMaterial(ref _depthMat);
        }


        //--------------------------
        public void CreateMaterial(ref Material m, Shader s)
        {
            if(m == null && s!= null)
            {
                m = new Material(s) {
                    hideFlags = HideFlags.DontSave
                };
            }
        }
        public void DestroyMaterial(ref Material m)
        {
            if(m != null)
            {
                if(Application.isPlaying)
                    Material.Destroy(m);
                else
                    Material.DestroyImmediate(m);
            }
            m = null;
        }
    }
}