using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;

// See _ReadMe.txt

public class DeferredDecalSystem
{
	static DeferredDecalSystem m_Instance;
	static public DeferredDecalSystem instance {
		get {
			if (m_Instance == null)
				m_Instance = new DeferredDecalSystem();
			return m_Instance;
		}
	}

	internal HashSet<Decal> m_DecalsDiffuse = new HashSet<Decal>();
	internal HashSet<Decal> m_DecalsNormals = new HashSet<Decal>();
	internal HashSet<Decal> m_DecalsBoth = new HashSet<Decal>();

	public void AddDecal (Decal d)
	{
		RemoveDecal (d);
		if (d.m_Kind == Decal.Kind.DiffuseOnly)
			m_DecalsDiffuse.Add (d);
		if (d.m_Kind == Decal.Kind.NormalsOnly)
			m_DecalsNormals.Add (d);
		if (d.m_Kind == Decal.Kind.Both)
			m_DecalsBoth.Add (d);
	}
	public void RemoveDecal (Decal d)
	{
		m_DecalsDiffuse.Remove (d);
		m_DecalsNormals.Remove (d);
		m_DecalsBoth.Remove (d);
	}
}

[ExecuteInEditMode]
public class DeferredDecalRenderer : MonoBehaviour
{
	public Mesh m_CubeMesh;
	private Dictionary<Camera,CommandBuffer> m_Cameras = new Dictionary<Camera,CommandBuffer>();

	public void OnDisable()
	{
		foreach (var cam in m_Cameras)
			if (cam.Key)
				cam.Key.RemoveCommandBuffer (CameraEvent.BeforeLighting, cam.Value);
	}

	public void OnWillRenderObject()
	{
		var act = gameObject.activeInHierarchy && enabled;
		if (!act)
		{
			OnDisable();
			return;
		}

		var cam = Camera.current;
		if (!cam)
			return;

		CommandBuffer buf = null;
		if (m_Cameras.ContainsKey(cam))
		{
            buf = m_Cameras[cam];
            buf.Clear();
        }
		else
		{
			buf = new CommandBuffer();
			buf.name = "Deferred decals";
			m_Cameras[cam] = buf;

			// 在延迟渲染的光照前添加这个缓冲
			cam.AddCommandBuffer (CameraEvent.BeforeLighting, buf);
		}

        //@TODO: in a real system should cull decals, and possibly only
        // recreate the command buffer when something has changed.
        // 需要做的：在真实系统中，应该对贴花进行剔除，在一些变换出现时，需要重新创建命令缓冲

        var system = DeferredDecalSystem.instance;

		var normalsID = Shader.PropertyToID("_NormalsCopy");
		buf.GetTemporaryRT (normalsID, -1, -1);

        // BuiltinRenderTextureType.GBuffer2 ：g-buffer的法线纹理
        buf.Blit (BuiltinRenderTextureType.GBuffer2, normalsID);

        // 渲染漫反射纹理到漫反射通道
        buf.SetRenderTarget (BuiltinRenderTextureType.GBuffer0, BuiltinRenderTextureType.CameraTarget);
		foreach (var decal in system.m_DecalsDiffuse)
			buf.DrawMesh (m_CubeMesh, decal.transform.localToWorldMatrix, decal.m_Material);

		// 渲染法线纹理到法线纹理通道
		buf.SetRenderTarget (BuiltinRenderTextureType.GBuffer2, BuiltinRenderTextureType.CameraTarget);
		foreach (var decal in system.m_DecalsNormals)
			buf.DrawMesh (m_CubeMesh, decal.transform.localToWorldMatrix, decal.m_Material);

		// 渲染漫反射+法线纹理
		RenderTargetIdentifier[] mrt = {BuiltinRenderTextureType.GBuffer0, BuiltinRenderTextureType.GBuffer2};
		buf.SetRenderTarget (mrt, BuiltinRenderTextureType.CameraTarget);
		foreach (var decal in system.m_DecalsBoth)
			buf.DrawMesh (m_CubeMesh, decal.transform.localToWorldMatrix, decal.m_Material);

		// 释放临时法线纹理
		buf.ReleaseTemporaryRT (normalsID);
	}
}
