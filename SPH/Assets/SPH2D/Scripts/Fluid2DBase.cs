using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;



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
        public Vector2 Acceleration;
    };

    public struct FluidPartcile2D
    {
        public Vector2 Position;
        public Vector2 Velocity;
    };





public class Fluid2DBase : MonoBehaviour { 

     public int particleNum;
    [SerializeField] float smoothlen = 0.012f;
    [SerializeField] float pressureStiffness = 200.0f;
    [SerializeField] protected float restDensity = 1000.0f;              
    [SerializeField] protected float particleMass = 0.0002f;             
    [SerializeField] protected float viscosity = 0.1f;
    [SerializeField] protected float maxAllowableTimestep = 0.005f;
    [SerializeField] protected float wallStiffness = 3000.0f;
    [SerializeField] protected int iterations = 4;         
    [SerializeField] protected Vector2 gravity = new Vector2(0.0f, -0.5f);   
    [SerializeField] protected Vector2 range = new Vector2(16, 9);
    //[SerializeField] protected bool simulate = true;


    //kernel
    private int numParticles = 1024 * 8;                                                           
    private float timeStep;                                     
    private float densityCoef;                                  
    private float gradPressureCoef;                             
    private float lapViscosityCoef;

    #region compute shader
    [SerializeField] ComputeShader cs;
    int THREAD_SIZE_X = 1024;
    public ComputeBuffer particlesBufferRead;
    private ComputeBuffer particlesBufferWrite;
    private ComputeBuffer particlesPressureBuffer;
    private ComputeBuffer particleDensitiesBuffer;
    private ComputeBuffer particleForcesBuffer;
    #endregion

    public void Start()
    {
        initBuffer();
        
    }

    public void Update()
    {
        timeStep = Mathf.Min(maxAllowableTimestep, Time.deltaTime);

        // 2Dカーネル係数
        densityCoef = particleMass * 4f / (Mathf.PI * Mathf.Pow(smoothlen, 8));
        gradPressureCoef = particleMass * -30.0f / (Mathf.PI * Mathf.Pow(smoothlen, 5));
        lapViscosityCoef = particleMass * 20f / (3 * Mathf.PI * Mathf.Pow(smoothlen, 5));

        cs.SetInt("_NumParticles", numParticles);
        cs.SetFloat("_TimeStep", timeStep);
        cs.SetFloat("_Smoothlen", smoothlen);
        cs.SetFloat("_PressureStiffness", pressureStiffness);
        cs.SetFloat("_RestDensity", restDensity);
        cs.SetFloat("_Viscosity", viscosity);
        cs.SetFloat("_DensityCoef", densityCoef);
        cs.SetFloat("_GradPressureCoef", gradPressureCoef);
        cs.SetFloat("_LapViscosityCoef", lapViscosityCoef);
        cs.SetFloat("_WallStiffness", wallStiffness);
        cs.SetVector("_Range", range);
        cs.SetVector("_Gravity", gravity);

        for(int i = 0; i < iterations; i++)
        {
            solver();
        }
    }


    void solver()
    {
        int kernelID = -1;
        int threadGroupsX = numParticles / THREAD_SIZE_X;


        //density
        kernelID = cs.FindKernel("DensityCS");
        cs.SetBuffer(kernelID, "_ParticlesBufferRead", particlesBufferRead);
        cs.SetBuffer(kernelID, "_ParticlesDensityBufferWrite", particleDensitiesBuffer);
        cs.Dispatch(kernelID, threadGroupsX, 1, 1);

        // Pressure
        kernelID = cs.FindKernel("PressureCS");
        cs.SetBuffer(kernelID, "_ParticlesDensityBufferRead", particleDensitiesBuffer);
        cs.SetBuffer(kernelID, "_ParticlesPressureBufferWrite", particlesPressureBuffer);
        cs.Dispatch(kernelID, threadGroupsX, 1, 1);

        // Force
        kernelID = cs.FindKernel("ForceCS");
        cs.SetBuffer(kernelID, "_ParticlesBufferRead", particlesBufferRead);
        cs.SetBuffer(kernelID, "_ParticlesDensityBufferRead", particleDensitiesBuffer);
        cs.SetBuffer(kernelID, "_ParticlesPressureBufferRead", particlesPressureBuffer);
        cs.SetBuffer(kernelID, "_ParticlesForceBufferWrite", particleForcesBuffer);
        cs.Dispatch(kernelID, threadGroupsX, 1, 1);

        // Integrate
        kernelID = cs.FindKernel("IntegrateCS");
        cs.SetBuffer(kernelID, "_ParticlesBufferRead", particlesBufferRead);
        cs.SetBuffer(kernelID, "_ParticlesForceBufferRead", particleForcesBuffer);
        cs.SetBuffer(kernelID, "_ParticlesBufferWrite", particlesBufferWrite);
        cs.Dispatch(kernelID, threadGroupsX, 1, 1);

        SwapComputeBuffer(ref particlesBufferRead, ref particlesBufferWrite);
    }

    public void OnDestroy()
    {
        particlesBufferRead.Release();
        particlesBufferWrite.Release();
        particlesPressureBuffer.Release();
        particleDensitiesBuffer.Release();
        particleForcesBuffer.Release();
    }

    void initBuffer()
    {
        Debug.Log("initBuffer");
        particlesBufferRead = new ComputeBuffer(numParticles, Marshal.SizeOf(typeof(FluidPartcile2D)));
        var particles = new FluidPartcile2D[numParticles];
        initParticleData(ref particles);
        particlesBufferRead.SetData(particles);

        //other bufferes
        particlesBufferWrite = new ComputeBuffer(numParticles, Marshal.SizeOf(typeof(FluidPartcile2D)));
        particlesPressureBuffer = new ComputeBuffer(numParticles, Marshal.SizeOf(typeof(FluidParticlePressure)));
        particleDensitiesBuffer = new ComputeBuffer(numParticles, Marshal.SizeOf(typeof(FluidParticleDensity)));
        particleForcesBuffer = new ComputeBuffer(numParticles, Marshal.SizeOf(typeof(FluidParticleForces)));
    }

    void initParticleData(ref FluidPartcile2D[] particles)
    {
        for(int i = 0; i < particles.Length; i++)
        {
            particles[i].Position = range / 2f + Random.insideUnitCircle * 3f   ;
            particles[i].Velocity = Vector2.zero;
        }
    }

    private void SwapComputeBuffer(ref ComputeBuffer ping, ref ComputeBuffer pong)
    {
        ComputeBuffer temp = ping;
        ping = pong;
        pong = temp;
    }
}
