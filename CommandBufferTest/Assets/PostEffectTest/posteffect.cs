using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class posteffect : MonoBehaviour
{
    [SerializeField]
    private Shader _shader;
    private Material _material;

    [SerializeField] RenderTexture tex;
    private void OnEnable()
    {
        _material = new Material(_shader);
    }

    private void Update()
    {
        _material.SetTexture("_tex", tex);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture dest)
    {
        Graphics.Blit(source, dest, _material);
    }
}
