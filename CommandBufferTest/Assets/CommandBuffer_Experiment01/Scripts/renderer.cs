using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

struct ParticleData
{

    public Vector3 pos;

    public ParticleData(Vector3 pos)
    {
        this.pos = pos;
    }
}
public class renderer : MonoBehaviour
{

    [SerializeField] int particleNum = 128 * 10;
    [SerializeField] Shader shader;
    [SerializeField] ComputeShader kernel;
    int kernelID;
    Material mat;
    ComputeBuffer buffer;
    

    void Start()
    {
        mat = new Material(shader);
        InitializeComputeBuffer();

        kernel.SetBuffer(0, "particles", buffer);
        //kernel.SetFloat("DeltaTime", Time.deltaTime);
        kernel.Dispatch(0, 128, 1, 1);
    }

    void Update()
    {
        
    }

    void OnRenderObject()
    {


        // テクスチャ、バッファをマテリアルに設定
        mat.SetBuffer("particles", buffer);

        // レンダリングを開始
        mat.SetPass(0);

        // 1万個のオブジェクトをレンダリング
        Graphics.DrawProcedural(MeshTopology.Points, particleNum);
    }


    void InitializeComputeBuffer()
    {
        // 弾数は1万個
        buffer = new ComputeBuffer(10000, Marshal.SizeOf(typeof(ParticleData)));

        // 配列に初期値を代入する
        ParticleData[] particles = new ParticleData[particleNum];
        for(int i = 0; i < particleNum; i++)
        {
            particles[i] =
                new ParticleData (new Vector3(Random.Range(-10.0f, 10.0f), Random.Range(-10.0f, 10.0f), 0.0f));
                   
        }

        // バッファに適応
        buffer.SetData(particles);
    }
}
