using EngineKit.Native.OpenGL;
using Serilog;

namespace EngineKit.HelloWindow
{
    internal class HelloWindowApplication : Application
    {
        public HelloWindowApplication(ILogger logger)
            : base(logger)
        {
        }

        protected override bool Load()
        {
            return base.Load();
        }

        protected override void Render()
        {
            GL.BindFramebuffer(GL.FramebufferTarget.Framebuffer, 0);
        }
    }
}
