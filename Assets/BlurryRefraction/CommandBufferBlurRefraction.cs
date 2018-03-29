using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

// 见 _ReadMe.txt 综述
[ExecuteInEditMode]
public class CommandBufferBlurRefraction : MonoBehaviour
{
	public Shader m_BlurShader;
	private Material m_Material;

	private Camera m_Cam;

    // 添加给所有相机这个渲染命令缓冲。
	private Dictionary<Camera,CommandBuffer> m_Cameras = new Dictionary<Camera,CommandBuffer>();

    // 移除所有相机中，自己添加的命令缓冲
	private void Cleanup()
	{
		foreach (var cam in m_Cameras)
			if (cam.Key)
				cam.Key.RemoveCommandBuffer (CameraEvent.AfterSkybox, cam.Value);

		m_Cameras.Clear();
		DestroyImmediate (m_Material);
	}

	public void OnEnable()
	{
		Cleanup();
	}

	public void OnDisable()
	{
		Cleanup();
	}

    // 在渲染物体前执行。如果MonoBehaviour禁止，就不会执行。
    public void OnWillRenderObject()
	{
		var act = gameObject.activeInHierarchy && enabled;
		if (!act)
		{
			Cleanup();
			return;
		}
		
		var cam = Camera.current;
		// 如果当前相机不存在 或 这个相机已经有这个命令缓冲，直接返回
		if (!cam || m_Cameras.ContainsKey(cam))
			return;

		if (!m_Material)
		{
			m_Material = new Material(m_BlurShader);
			m_Material.hideFlags = HideFlags.HideAndDontSave;
		}

		CommandBuffer buf = new CommandBuffer();
		buf.name = "Grab screen and blur";
		m_Cameras[cam] = buf;

        // 将场景赋值到临时纹理（名为“_ScreenCopyTexture”）中
        int screenCopyID = Shader.PropertyToID("_ScreenCopyTexture");
		buf.GetTemporaryRT (screenCopyID, -1, -1, 0, FilterMode.Bilinear);
		buf.Blit (BuiltinRenderTextureType.CurrentActive, screenCopyID);
		
		// 获取两个更小的临时纹理
		int blurredID = Shader.PropertyToID("_Temp1");
		int blurredID2 = Shader.PropertyToID("_Temp2");
		buf.GetTemporaryRT (blurredID, -2, -2, 0, FilterMode.Bilinear);
		buf.GetTemporaryRT (blurredID2, -2, -2, 0, FilterMode.Bilinear);
		
        // 降采样原来的场景纹理 到 小纹理中，释放原来的场景纹理
		buf.Blit (screenCopyID, blurredID);
		buf.ReleaseTemporaryRT (screenCopyID); 
		
		// 水平模糊
		buf.SetGlobalVector("offsets", new Vector4(2.0f/Screen.width,0,0,0));
		buf.Blit (blurredID, blurredID2, m_Material);
		// 垂直模糊
		buf.SetGlobalVector("offsets", new Vector4(0,2.0f/Screen.height,0,0));
		buf.Blit (blurredID2, blurredID, m_Material);
        // 水平模糊
        buf.SetGlobalVector("offsets", new Vector4(4.0f/Screen.width,0,0,0));
		buf.Blit (blurredID, blurredID2, m_Material);
        // 垂直模糊
        buf.SetGlobalVector("offsets", new Vector4(0,4.0f/Screen.height,0,0));
		buf.Blit (blurredID2, blurredID, m_Material);

        // 将模糊处理好的纹理放到名为“_GrabBlurTexture”的纹理中
        buf.SetGlobalTexture("_GrabBlurTexture", blurredID);

        // 最后把这些渲染命令放在相机渲染完天空盒子之后
		cam.AddCommandBuffer (CameraEvent.AfterSkybox, buf);
	}	
}
