using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CmdBufUtils
{
    public class CommandBufferUtils : MonoBehaviour
    {
        /*
        void Start()
        {

        }

        void Update()
        {

        }*/

        public void DestroyMaterial(ref Material m)
        {
            if(m != null)
            {
                if(Application.isPlaying)
                    Material.Destroy(m);
                else
                    Material.DestroyImmediate(m);
            }
            m = null;
        }
    }
}
