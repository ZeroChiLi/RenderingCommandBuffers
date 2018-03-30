This scene shows a basic implementation of "custom deferred lights" - i.e.
it shows how command buffers can be used to implement custom lights that
compute scene illumination in deferred shading.

The idea is: after regular deferred shading light pass is done,
draw a sphere for each custom light, with a shader that computes illumination
and adds it to the lighting buffer.

Note: this is small example code, and not a production-ready custom light system!

In this example, each custom light has a CustomLight.cs script attached; two kinds of
custom lights are implemented:

* Sphere: similar to a point light, but with an actual "area light size".
* Tube: a cylinder-shaped light, has a lengh along X axis and cylinder radius (size).

The shader to compute illumination is CustomLightShader.shader, and uses "closest point"
approximations Brian Karis' SIGGRAPH 2013 course
(http://blog.selfshadow.com/publications/s2013-shading-course/).

CustomLightRenderer.cs is a script that should be assigned to some "always visible"
object (e.g. ground). When that object becomes visible by any camera, it will add
command buffers with all the lights to that camera. This way this works for scene view
cameras too, but is not an ideal setup for a proper custom light system.

Caveats: for simplicity reasons, the example code does a not-too-efficient
rendering of light shapes. Would be more efficient to use stencil marking as in typical
deferred shading; here we just render the sphere backfaces with depth testing off.


这个场景展示了“自定义延迟光照”的实现。即使用命令缓冲可以实现延迟渲染的自定义光照。

原理：在常规光照Pass完成后，给所有自定义光照，绘制一个球型，还有计算光照的shader，并添加到光照缓冲中。

注意：这只是个小样例代码，并不是完整的自定义光照系统。

在这个样例中，自定义关照都带着 CustomLight.cs 脚本。有两种自定义光照：
 - 球型：类似点光源，但是他是有区域光大小的（即球体里面光照满值）。
 - 管型（长方体）：包括X长度，和Size半径。

实现光照计算的shader：CustomLightShader.shader，使用了“最近点”取近似值。

CustomLightRenderer.cs：自定义光照渲染器，建议放在总是存在可见的对象中。他会添加命令缓冲到所有相机。
同样包括场景视图的相机，但并非一个理想的自定义光照系统。

警告：为了简单，这个样例代码在渲染灯光形状时，就不是太效率了。如果想要更高效，使用模版标记在典型延迟渲染，
可以在渲染球形背面，关闭深度测试。
