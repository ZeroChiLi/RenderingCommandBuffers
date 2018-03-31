using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;

// See _ReadMe.txt

public class CustomLightSystem
{
	static CustomLightSystem m_Instance;
	static public CustomLightSystem instance {
		get {
			if (m_Instance == null)
				m_Instance = new CustomLightSystem();
			return m_Instance;
		}
	}

	internal HashSet<CustomLight> m_Lights = new HashSet<CustomLight>();

	public void Add (CustomLight o)
	{
		Remove (o);
		m_Lights.Add (o);
	}
	public void Remove (CustomLight o)
	{
		m_Lights.Remove(o);
	}
}


[ExecuteInEditMode]
public class CustomLightRenderer : MonoBehaviour
{
	public Shader m_LightShader;
	private Material m_LightMaterial;

	public Mesh m_CubeMesh;
	public Mesh m_SphereMesh;

    // 两个命令缓冲：
    // 第一个：在常规光照之后，计算所有自定义光照。
    // 第二个：在透明物体渲染前，绘制光照本身（灯泡）。
	private struct CmdBufferEntry
	{
		public CommandBuffer m_AfterLighting;
		public CommandBuffer m_BeforeAlpha;
	}

	private Dictionary<Camera,CmdBufferEntry> m_Cameras = new Dictionary<Camera,CmdBufferEntry>();

	public void OnDisable()
	{
		foreach (var cam in m_Cameras)
			if (cam.Key)
			{
				cam.Key.RemoveCommandBuffer (CameraEvent.AfterLighting, cam.Value.m_AfterLighting);
				cam.Key.RemoveCommandBuffer (CameraEvent.BeforeForwardAlpha, cam.Value.m_BeforeAlpha);
			}
		DestroyImmediate (m_LightMaterial);
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

		// 创建光照材质
		if (!m_LightMaterial)
		{
			m_LightMaterial = new Material(m_LightShader);
			m_LightMaterial.hideFlags = HideFlags.HideAndDontSave;
		}			

		CmdBufferEntry buf = new CmdBufferEntry();
		if (m_Cameras.ContainsKey(cam))
		{
			// use existing command buffers: clear them
            // 如果已经存在这个命令，清除掉。
			buf = m_Cameras[cam];
			buf.m_AfterLighting.Clear ();
			buf.m_BeforeAlpha.Clear ();
		}
		else
		{
			// create new command buffers
            // 创建新的命令缓冲
			buf.m_AfterLighting = new CommandBuffer();
			buf.m_AfterLighting.name = "Deferred custom lights";
			buf.m_BeforeAlpha = new CommandBuffer();
			buf.m_BeforeAlpha.name = "Draw light shapes";
			m_Cameras[cam] = buf;

			cam.AddCommandBuffer (CameraEvent.AfterLighting, buf.m_AfterLighting);
			cam.AddCommandBuffer (CameraEvent.BeforeForwardAlpha, buf.m_BeforeAlpha);
		}

		//@TODO: in a real system should cull lights, and possibly only
		// recreate the command buffer when something has changed.
        // 需要做的：在真实系统中需要剔除灯光，在一些变换出现时，需要重新创建命令缓冲

		var system = CustomLightSystem.instance;

		var propParams = Shader.PropertyToID("_CustomLightParams");
		var propColor = Shader.PropertyToID("_CustomLightColor");
        var propShapeAlpha = Shader.PropertyToID("_CustomShapeAlpha");
		Vector4 param = Vector4.zero;
		Matrix4x4 trs = Matrix4x4.identity;

		// construct command buffer to draw lights and compute illumination on the scene
        // 添加光照参数，绘制轮廓
		foreach (var o in system.m_Lights)
		{
			// light parameters we'll use in the shader
            // x：灯光长度，y：光照大小，z：半径二次幂的倒数，w：灯光类型
			param.x = o.m_TubeLength;
			param.y = o.m_Size;
			param.z = 1.0f / (o.m_Range * o.m_Range);
			param.w = (float)o.m_Kind;
			buf.m_AfterLighting.SetGlobalVector (propParams, param);
			buf.m_AfterLighting.SetGlobalColor (propColor, o.GetLinearColor()); // 设置光照颜色

            // 计算场景位置，绘制光照区域球体。
			// draw sphere that covers light area, with shader
			// pass that computes illumination on the scene
			trs = Matrix4x4.TRS(o.transform.position, o.transform.rotation, new Vector3(o.m_Range*2,o.m_Range*2,o.m_Range*2));
			buf.m_AfterLighting.DrawMesh (m_SphereMesh, trs, m_LightMaterial, 0, 0);
		}

		// construct buffer to draw light shapes themselves as simple objects in the scene
        // 绘制光照形状
		foreach (var o in system.m_Lights)
		{
			buf.m_BeforeAlpha.SetGlobalColor (propColor, o.GetLinearColor());
            buf.m_BeforeAlpha.SetGlobalFloat(propShapeAlpha, o.m_ShapeAlpha);

			if (o.m_Kind == CustomLight.Kind.Sphere)
			{
				trs = Matrix4x4.TRS(o.transform.position, o.transform.rotation, new Vector3(o.m_Size*2,o.m_Size*2,o.m_Size*2));
				buf.m_BeforeAlpha.DrawMesh (m_SphereMesh, trs, m_LightMaterial, 0, 1);
			}
			else if (o.m_Kind == CustomLight.Kind.Tube)
			{
				trs = Matrix4x4.TRS(o.transform.position, o.transform.rotation, new Vector3(o.m_TubeLength*2,o.m_Size*2,o.m_Size*2));
				buf.m_BeforeAlpha.DrawMesh (m_CubeMesh, trs, m_LightMaterial, 0, 1);
			}
		}		
	}
}
