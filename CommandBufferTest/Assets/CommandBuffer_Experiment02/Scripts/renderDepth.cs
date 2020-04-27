using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
public class renderDepth : MonoBehaviour
{
    private Camera m_Camera;
    private RenderTexture m_ColorBuffer;
    private RenderTexture m_DepthBuffer;


    void Start()
    {
        m_Camera = GetComponent<Camera>();

        m_ColorBuffer = new RenderTexture(Screen.width, Screen.height, 0);
        m_ColorBuffer.Create();

        m_DepthBuffer = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.Depth);
        m_DepthBuffer.Create();

        m_Camera.SetTargetBuffers(m_ColorBuffer.colorBuffer, m_DepthBuffer.depthBuffer);
    }

    
    void Update()
    {
        
    }

    private void addCommand()
    {
        CommandBuffer cmd = new CommandBuffer();
        cmd.name = "Set Depth Texture";
        cmd.SetGlobalTexture("_DepthTexture", m_DepthBuffer);
        m_Camera.AddCommandBuffer(CameraEvent.BeforeImageEffects, cmd);

        {
            CommandBuffer command = new CommandBuffer();
            command.name = "blit to Back buffer";

            // (注)
            // カメラの書き込み先がRenderTextureなのに、CameraEvent.AfterEverythingのタイミングで
            // CameraTargetがback bufferを示すのは正しいのだろうか・・・
            // 確認バージョン：Unity5.6.1f1
            command.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
            command.Blit(m_ColorBuffer, BuiltinRenderTextureType.CurrentActive);

            m_Camera.AddCommandBuffer(CameraEvent.AfterEverything, command);
        }

    }
}
