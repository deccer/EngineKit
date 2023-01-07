using System.Runtime.InteropServices;

namespace SpaceGame.Development.RenderDoc;

public partial class RenderDoc
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct RenderDocApi140
    {
        public GetAPIVersionDelegate GetApiVersionDelegate;

        public SetCaptureOptionU32Delegate SetCaptureOptionU32Delegate;
        public GetCaptureOptionU32Delegate GetCaptureOptionU32Delegate;
        
        public SetCaptureOptionF32Delegate SetCaptureOptionF32Delegate;
        public GetCaptureOptionF32Delegate GetCaptureOptionF32Delegate;

        public SetFocusToggleKeysDelegate SetFocusToggleKeysDelegate;
        public SetCaptureKeysDelegate SetCaptureKeysDelegate;

        public GetOverlayBitsDelegate GetOverlayBitsDelegate;
        public MaskOverlayBitsDelegate MaskOverlayBitsDelegate;

        public ShutdownDelegate ShutdownDelegate;
        public UnloadCrashHandlerDelegate UnloadCrashHandlerDelegate;

        public SetCaptureFilePathTemplateDelegate SetCaptureFilePathTemplateDelegate;
        public GetCaptureFilePathTemplateDelegate GetCaptureFilePathTemplateDelegate;

        public GetNumCapturesDelegate GetNumCapturesDelegate;
        public GetCaptureDelegate GetCaptureDelegate;

        public TriggerCaptureDelegate TriggerCaptureDelegate;

        public IsTargetControlConnectedDelegate IsTargetControlConnectedDelegate;
        public LaunchReplayUIDelegate LaunchReplayUiDelegate;

        public SetActiveWindowDelegate SetActiveWindowDelegate;

        public StartFrameCaptureDelegate StartFrameCaptureDelegate;
        public IsFrameCapturingDelegate IsFrameCapturingDelegate;
        public EndFrameCaptureDelegate EndFrameCaptureDelegate;

        // new function in 1.1.0
        public TriggerMultiFrameCaptureDelegate TriggerMultiFrameCaptureDelegate;

        // new function in 1.2.0
        public SetCaptureFileCommentsDelegate SetCaptureFileCommentsDelegate;

        // new function in 1.4.0
        public DiscardFrameCaptureDelegate DiscardFrameCaptureDelegate;
    }
}