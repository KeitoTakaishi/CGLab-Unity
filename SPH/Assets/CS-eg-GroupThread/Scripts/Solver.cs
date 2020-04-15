using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace CS_eg_GroupThread
{

    
    public class Solver : MonoBehaviour
    {
        const int SIMULATION_BLOCK_SIZE = 128;
        [SerializeField] ComputeShader kernel;
        ComputeBuffer buffer;
        float[] readBuffer;
        int kernelID;
        int n = 128 * 8;
        
        void Start()
        {
            readBuffer = new float[n];
            buffer = new ComputeBuffer(n, Marshal.SizeOf(typeof(float)));

            float[] temp = new float[n];
            for(int i = 0; i < n; i++)
            {
                temp[i] = Mathf.Floor(i / 128) / 128.0f;
               
            }
            buffer.SetData(temp);
            kernelID = kernel.FindKernel("calc");
            kernel.SetBuffer(kernelID, "buffer", buffer);
            kernel.Dispatch(kernelID, SIMULATION_BLOCK_SIZE, 1, 1);
            buffer.GetData(readBuffer);
            for(int i = 0; i <readBuffer.Length; i++)
            {
               Debug.Log(i.ToString() + " : " + readBuffer[i].ToString());
            }
        }

        void Update()
        {

        }
    }
}