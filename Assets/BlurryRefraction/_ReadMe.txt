This scene shows a very basic "blurry refraction" technique using
command buffers.

The idea is: after opaque objects & skybox is rendered, copy the image
into a temporary render target, blur it and set up a global shader property
with the result. Objects that are rendered after skybox (i.e. all
semitransparent objects) can then sample this "blurred scene image". This is
like GrabPass, just a bit better!

See CommandBufferBlurRefraction.cs script that is attached to the glass object
in the scene.

Caveat: right now it does not capture the scene view properly though; the
texture ends up only containing the skybox :(

Caveat: this is small example code, and not a production-ready refractive
blurred glass system. It very likely won't deal with multiple objects having
the script attached to them, etc.



这个场景展示了一个非常基础的，使用了命令缓冲的“模糊折射”技术。

原理是：在不透明物体和天空盒子渲染完之后，复制一个图像到临时渲染纹理中，处理模糊并设置一个全局shader属性实现。
对于那些在天空盒子之后渲染的物体（包括所有半透明物体），需要采样这个“模糊处理过的图像”。
这个效果就像用GrabPass，只是好一点点。

可见 CommandBufferBlurRefraction.cs 附着在的玻璃对象。

警告：此时还没有捕获场景视图的思想。纹理最终只包含天空盒子（即直接用颜色作为场景环境光是无法实现的）。

警告：这只是个样例代码，不是一个完整反射玻璃系统。对于多个这样的玻璃处理效果是不会逐一处理的。