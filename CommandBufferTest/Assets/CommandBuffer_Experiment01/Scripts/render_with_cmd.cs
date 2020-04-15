using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


public class render_with_cmd : MonoBehaviour
{
    [SerializeField] int particleNum = 128 * 10;
    [SerializeField] Shader shader;
    [SerializeField] ComputeShader kernel;
    int kernelID;
    Material mat;
    ComputeBuffer buffer;
    CommandBuffer commandb;

    void Start()
    {
        InitializeComputeBuffer();
        mat = new Material(shader);
        mat.SetBuffer("particles", buffer);
        commandb = new CommandBuffer();
        Camera cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        commandb.name = "draw procedual";
        commandb.DrawProcedural(cam.cameraToWorldMatrix, mat, 0, MeshTopology.Points
            , particleNum, 1);
        //cam.AddCommandBuffer(CameraEvent.AfterForwardOpaque, commandb);
        cam.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, commandb);
        //cam.AddCommandBuffer(CameraEvent.AfterSkybox, commandb);
        //cam.AddCommandBuffer(CameraEvent.AfterLighting, commandb);
    }

    void Update()
    {
       
        kernelID = kernel.FindKernel("Calc");
        kernel.SetFloat("time", Time.realtimeSinceStartup);
        kernel.SetBuffer(kernelID, "buffer", buffer);
        kernel.Dispatch(kernelID, 128, 1, 1);
        mat.SetBuffer("particles", buffer);


    }

    void InitializeComputeBuffer()
    {
        // 弾数は1万個
        buffer = new ComputeBuffer(10000, System.Runtime.InteropServices.Marshal.SizeOf(typeof(ParticleData)));

        // 配列に初期値を代入する
        ParticleData[] particles = new ParticleData[particleNum];
        for(int i = 0; i < particleNum; i++)
        {
            particles[i] =
                new ParticleData(new Vector3(Random.Range(-10.0f, 10.0f), Random.Range(-10.0f, 10.0f), 0.0f));

        }

        // バッファに適応
        buffer.SetData(particles);
    }
}
