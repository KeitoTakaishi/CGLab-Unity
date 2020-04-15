using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Camera))]
public class ShowGBuffer : MonoBehaviour
{
    enum GBufferType
    {
        diffuse,
        specular,
        normal,
        emission,
        depth,
        source,
    };

    [SerializeField] GBufferType _gbufferType = GBufferType.source;

    [SerializeField]
    Material _mat;

    void Start()
    {
        GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
    }

    void Update()
    {
        
    }

    [ImageEffectOpaque]
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        foreach(var keyword in _mat.shaderKeywords)
        {
            //Debug.Log(keyword);
            _mat.DisableKeyword(keyword);

            switch(_gbufferType)
            {
                case GBufferType.diffuse:
                    _mat.EnableKeyword("_GBUFFERTYPE_DIFFUSE");
                    break;
                case GBufferType.specular:
                    _mat.EnableKeyword("_GBUFFERTYPE_SPECULAR");
                    break;
                case GBufferType.normal:
                    _mat.EnableKeyword("_GBUFFERTYPE_WORLDNORMAL");
                    break;
                case GBufferType.emission:
                    _mat.EnableKeyword("_GBUFFERTYPE_EMISSION");
                    break;
                case GBufferType.depth:
                    _mat.EnableKeyword("_GBUFFERTYPE_DEPTH");
                    break;
                default:
                    _mat.EnableKeyword("_GBUFFERTYPE_SOURCE");
                    break;
            }
            Graphics.Blit(source, destination, _mat);

        }
    }
}
