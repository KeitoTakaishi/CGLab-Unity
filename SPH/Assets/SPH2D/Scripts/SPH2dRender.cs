using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPH2dRender : MonoBehaviour
{
    [SerializeField] Fluid2DBase fluid2d;
    [SerializeField] Material mat;
    void Start()
    {
        //fluid2d.Start();

    }

    private void Update()
    {
        //fluid2d.Update();   
    }
    void OnRenderObject()
    {
       
        DrawParticle();
    }

    void DrawParticle()
    {
        mat.SetPass(0);
        mat.SetBuffer("particle", fluid2d.particlesBufferRead);
        Graphics.DrawProcedural(MeshTopology.Points, fluid2d.particleNum);
    }
}
