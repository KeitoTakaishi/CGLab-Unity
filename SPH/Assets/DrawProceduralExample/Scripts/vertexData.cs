using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public struct VertexData
{
    public Vector3 pos;
};


public class vertexData : MonoBehaviour
{

    [SerializeField] ComputeShader cs;
    public ComputeBuffer positionBuffer;
    public int particleNum = 1024;

    
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void initBuffer()
    {
        positionBuffer = new ComputeBuffer(particleNum, Marshal.SizeOf(typeof(VertexData)));
        //Vector3[] pos = new Vector3[particleNum];
        var v = new VertexData[particleNum];
        for(int i = 0; i < particleNum; i++)
        {
            v[i].pos = Random.insideUnitSphere * 5.0f;
            
        }
        positionBuffer.SetData(v);
    }
}
