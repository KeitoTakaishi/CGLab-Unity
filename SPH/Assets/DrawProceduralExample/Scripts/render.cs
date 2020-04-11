using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class render : MonoBehaviour
{
    [SerializeField] vertexData vertexdata;
    [SerializeField] Material mat;
    void Start()
    {
        vertexdata.initBuffer();
    }


    void Update()
    {
        //DrawParticle();
    }

    void OnRenderObject()
    {
        DrawParticle();
    }

    void DrawParticle()
    {
        mat.SetPass(0);
        mat.SetBuffer("_vertexData", vertexdata.positionBuffer);
        Graphics.DrawProcedural(MeshTopology.Points, vertexdata.particleNum);
    }
}
