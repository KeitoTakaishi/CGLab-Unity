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
        [SerializeField] Shader _renderGBufferShader;

        [SerializeField] int particleNum;
        int kernelID;
        Material _depthMat;
        Material _renderGBufferMat;
        public ComputeBuffer buffer;
        [SerializeField] float speed;


        //----------------------
        //command buffer
        CommandBuffer cmd;
        Camera cam;
        Mesh quad;


        [SerializeField] float particleSize = 0.1f;


        void Start()
        {
            

            CreateComputeBuffer();
            kernelID = 0;
            quad = GenerateQuad();

            //CommandBuffer
            _depthMat = new Material(_depthShader);
            _renderGBufferMat = new Material(_renderGBufferShader);

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
          
            var _cmd = cmd;
            _cmd.Clear();
            //render particle----------------
            int depthBufferId = Shader.PropertyToID("-DepthBuffer_SSFR");
            _cmd.GetTemporaryRT(depthBufferId, -1, -1, 24, FilterMode.Point, RenderTextureFormat.RFloat);
            
            _cmd.SetRenderTarget
                (
                    new RenderTargetIdentifier(depthBufferId),  // デプス
                    new RenderTargetIdentifier(depthBufferId)   // デプス書き込み用
                );
            _cmd.ClearRenderTarget(true, true, Color.clear);
            
            _depthMat.SetBuffer("particles", buffer);
            _depthMat.SetFloat("particleSize", particleSize);
            _cmd.DrawProcedural(Matrix4x4.identity, _depthMat, 0, MeshTopology.Points, particleNum);

            //write gbuffer------------------
            
            _cmd.SetGlobalTexture("_DepthBuffer", depthBufferId);
            _cmd.SetRenderTarget
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

            _cmd.DrawMesh(quad, Matrix4x4.identity, _renderGBufferMat);
            _cmd.ReleaseTemporaryRT(depthBufferId);
            
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
    }
}