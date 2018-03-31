This scene shows a basic implementation of "deferred decals" - i.e.
it shows how command buffers can be used to modify the deferred
shading g-buffer before lighting is done.

Idea behind them is described by Emil Persson here:
http://www.humus.name/index.php?page=3D&ID=83 and Pope Kim here:
http://www.popekim.com/2012/10/siggraph-2012-screen-space-decals-in.html

The idea is: after g-buffer is done, draw each "shape" of the decal (e.g. box)
and modify the g-buffer contents. This is very similar to how lights are done
in deferred shading, except instead of accumulating the lighting
we modify the g-buffer textures.

Note: this is small example code, and not a production-ready decal system!

In this example, each decal should have Decal.cs script attached; transform
placement and scale defines decal area of influence. Decals can be of three kinds:

* Diffuse only (only affects underlying surface's diffuse color)
* Normals only (only affects normals of the underlying surface; does not change color)
* Both diffuse & normals

Each type should use appropriate shader to render its effect, see shaders in DeferredDecals
folder.

DeferredDecalRenderer.cs is a script that should be assigned to some "always visible"
object (e.g. ground). When that object becomes visible by any camera, it will add
command buffers with all the decals to that camera. This way this works for scene view
cameras too, but is not an ideal setup for a proper decal system.

One caveat though: in current Unity's deferred shading implementation, lightmaps, ambient
and reflection probes are done as part of g-buffer rendering pass. Since decals only
modify the g-buffer after it's done, they *do not affect* ambient/lightmaps! This means
that for example in the shadow of a light (where no other lights are shining),
decals will not be visible.


这个场景展示了一个基础实例：“延迟渲染的贴花效果”。即使用命令缓冲修改延迟着色器的g-buffer（在光照前完成）。

原理：在 g-buffer 完成以后，绘制所有“贴花”，并且修改g-buffer的内容。类似延迟渲染的光照，只不过不是积累光照，而是修改g-buffer。

注意：这只是个样例代码，并非完整贴花系统。

在这个样例中，每一个贴花对象需要 Decal.cs 脚本；transform会控制贴花位置大小等。
贴花包含三种类型：
 - Diffuse only ：只有表面漫反射颜色。
 - Normals only ：只有法线纹理变换，不改变颜色。
 - Both ：前面两个都有。

上面每一个类型需要分别使用不同的shader。

DeferredDecalRenderer.cs 需要放在总是可见的对象中。对所有相机生效（包括场景相机）。但这不是一个完整的贴花系统。

警告：在当前Unity的延迟着色实现上，光照图、环境光、反射探针都在g-buffer渲染pass块中执行。
当贴花只在g-buffer完成后修改，他们不会影响光照图/环境光等。这意味着，如果没有光照，贴花将不可见。



