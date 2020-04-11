using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;


namespace SPH3D
{
    struct FluidParticleDensity
    {
        public float Density;
    };

    struct FluidParticlePressure
    {
        float pressure;
    };

    struct FluidParticleForces
    {
        public Vector3 Acceleration;
    };

    public struct FluidPartcile3D
    {
        public Vector3 Position;
        public Vector3 Velocity;
    };

    public class SPH3DSolver : MonoBehaviour
    {
        public int particleNum = 8192;
        [SerializeField] float smoothlen = 0.012f;
        [SerializeField] float pressureStiffness = 200.0f;
        [SerializeField] float restDensity = 1000.0f;
        [SerializeField] float particleMass = 0.0002f;
        [SerializeField] float viscosity = 0.1f;
        [SerializeField] float wallStiffness = 3000.0f;
        [SerializeField] int iterations = 4;
        [SerializeField] protected float maxAllowableTimestep = 0.005f;
        [SerializeField] Vector3 gravity = new Vector3(0, -0.5f, 0);
        [SerializeField] public Vector3 range = new Vector3(10, 10, 10);
        [SerializeField] float initSpherRadius = 1.0f;

        private float timeStep;
        private float densityCoef;
        private float gradPresCoef;
        private float lapViscCoef;

        #region ComputeShader
        [SerializeField] ComputeShader cs;
        int THREAD_SIZE_X = 1024;
        public ComputeBuffer particlesBufferRead;
        private ComputeBuffer particlesBufferWrite;
        private ComputeBuffer particlesPressureBuffer;
        private ComputeBuffer particleDensitiesBuffer;
        private ComputeBuffer particleForcesBuffer;
        #endregion

        void Start()
        {
            initBuffer();
            densityCoef = particleMass * 315f / (64f * Mathf.PI * Mathf.Pow(smoothlen, 9));
            gradPresCoef = particleMass * -45.0f / (Mathf.PI * Mathf.Pow(smoothlen, 6));
            lapViscCoef = particleMass * 45f / (Mathf.PI * Mathf.Pow(smoothlen, 6));

        }

        void Update()
        {
            timeStep = Mathf.Min(maxAllowableTimestep, Time.deltaTime);

            
            cs.SetInt("_NumParticles", particleNum);
            cs.SetFloat("_TimeStep", timeStep);
            cs.SetFloat("_Smoothlen", smoothlen);
            cs.SetFloat("_PressureStiffness", pressureStiffness);
            cs.SetFloat("_RestDensity", restDensity);
            cs.SetFloat("_Viscosity", viscosity);
            cs.SetFloat("_DensityCoef", densityCoef);
            cs.SetFloat("_GradPressureCoef", gradPresCoef);
            cs.SetFloat("_LapViscosityCoef", lapViscCoef);
            cs.SetFloat("_WallStiffness", wallStiffness);
            cs.SetVector("_Range", range);
            cs.SetVector("_Gravity", gravity);

            for(int i = 0; i < iterations; i++)
            {
                simulate();
            }
            SwapComputeBuffer(ref particlesBufferRead, ref particlesBufferWrite);
            
        }

        void OnDestroy()
        {
            Debug.Log("ReleaseBuffer");
            particlesBufferRead.Release();
            particlesBufferWrite.Release();
            particlesPressureBuffer.Release();
            particleDensitiesBuffer.Release();
            particleForcesBuffer.Release();
        }

        void initBuffer()
        {
            Debug.Log("initBuffer");
            particlesBufferRead = new ComputeBuffer(particleNum, Marshal.SizeOf(typeof(FluidPartcile3D)));
            var particles = new FluidPartcile3D[particleNum];
            initParticleData(ref particles);
            particlesBufferRead.SetData(particles);

            particlesBufferWrite = new ComputeBuffer(particleNum, Marshal.SizeOf(typeof(FluidPartcile3D)));
            particlesPressureBuffer = new ComputeBuffer(particleNum, Marshal.SizeOf(typeof(FluidParticlePressure)));
            particleDensitiesBuffer = new ComputeBuffer(particleNum, Marshal.SizeOf(typeof(FluidParticleDensity)));
            particleForcesBuffer = new ComputeBuffer(particleNum, Marshal.SizeOf(typeof(FluidParticleForces)));
        }

        void initParticleData(ref FluidPartcile3D[] particles)
        {
            for(int i = 0; i < particles.Length; i++)
            {
                particles[i].Position = range / 2.0f + Random.insideUnitSphere * (float)initSpherRadius;
                particles[i].Velocity = Vector3.zero;
            }
        }

        void simulate()
        {
            int kernelID = -1;
            int threadGroupsX = particleNum / THREAD_SIZE_X;

            //density
            kernelID = cs.FindKernel("DensityKer");
            cs.SetBuffer(kernelID, "_ParticlesBufferRead", particlesBufferRead);
            cs.SetBuffer(kernelID, "_ParticlesDensityBufferWrite", particleDensitiesBuffer);
            cs.Dispatch(kernelID, threadGroupsX, 1, 1);

            kernelID = cs.FindKernel("PressureKer");
            cs.SetBuffer(kernelID, "_ParticlesDensityBufferRead", particleDensitiesBuffer);
            cs.SetBuffer(kernelID, "_ParticlesPressureBufferWrite", particlesPressureBuffer);
            cs.Dispatch(kernelID, threadGroupsX, 1, 1);

            kernelID = cs.FindKernel("ForceKer");
            cs.SetBuffer(kernelID, "_ParticlesBufferRead", particlesBufferRead);
            cs.SetBuffer(kernelID, "_ParticlesDensityBufferRead", particleDensitiesBuffer);
            cs.SetBuffer(kernelID, "_ParticlesPressureBufferRead", particlesPressureBuffer);
            cs.SetBuffer(kernelID, "_ParticlesForceBufferWrite", particleForcesBuffer);
            cs.Dispatch(kernelID, threadGroupsX, 1, 1);

            kernelID = cs.FindKernel("IntegrateKer");
            cs.SetBuffer(kernelID, "_ParticlesBufferRead", particlesBufferRead);
            cs.SetBuffer(kernelID, "_ParticlesForceBufferRead", particleForcesBuffer);
            cs.SetBuffer(kernelID, "_ParticlesBufferWrite", particlesBufferWrite);
            cs.Dispatch(kernelID, threadGroupsX, 1, 1);


        }

        private void SwapComputeBuffer(ref ComputeBuffer ping, ref ComputeBuffer pong)
        {
            ComputeBuffer temp = ping;
            ping = pong;
            pong = temp;
        }
    }
}
