using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SPH3D;
public class SPH3DRender : MonoBehaviour
{
    [SerializeField] SPH3DSolver solver;
    [SerializeField] Material mat;

     void Start()
    {
        
    }

    void Update()
    {
        
    }

    void OnRenderObject()
    {

        DrawParticle();
    }

    void OnDrawGizmosSelected()
    {
        // Draw a semitransparent blue cube at the transforms position
        Gizmos.color = new Color(1, 1, 1, 0.5f);
        Gizmos.DrawWireCube(solver.range/2.0f, solver.range);
    }

    void DrawParticle()
    {
        mat.SetPass(0);
        mat.SetBuffer("particle", solver.particlesBufferRead);
        Graphics.DrawProcedural(MeshTopology.Points, solver.particleNum);
    }
}
