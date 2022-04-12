using System.Runtime.InteropServices;

namespace EngineKit.Native.OpenGL
{
    public static unsafe partial class GL
    {
        private static delegate* unmanaged<uint, uint, void> _glBindFramebufferDelegate = &glBindFramebuffer;

        public static void BindFramebuffer(FramebufferTarget framebufferTarget, uint framebuffer)
        {
            _glBindFramebufferDelegate((uint)framebufferTarget, framebuffer);
        }

        [UnmanagedCallersOnly]
        private static void glBindFramebuffer(uint frameBufferTarget, uint frameBuffer)
        {
            _glBindFramebufferDelegate = (delegate* unmanaged<uint, uint, void>)Glfw.Glfw.GetProcAddress(nameof(glBindFramebuffer));
        }
    }
}