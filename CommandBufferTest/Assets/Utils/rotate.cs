using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotate : MonoBehaviour
{
    [SerializeField] Vector3 axisSpeed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var t = Time.realtimeSinceStartup;
        this.transform.localEulerAngles = new Vector3(t * axisSpeed.x, t * axisSpeed.y, t * axisSpeed.z );
    }
}
