using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SSFR
{
    public class DebugRender : MonoBehaviour
    {
        [SerializeField] SPHSimulation simulation;
        [SerializeField] Material mat;

        private void Awake()
        {
            //simulation = this.GetComponent<SPHSimulation>();
        }
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


        void DrawParticle()
        {
            mat.SetPass(0);
            mat.SetBuffer("particle", simulation.GetParticlesBuffer());
            //Debug.Log(simulation.GetParticleNum());
            Graphics.DrawProcedural(MeshTopology.Points, simulation.GetParticleNum());
        }
    }

}