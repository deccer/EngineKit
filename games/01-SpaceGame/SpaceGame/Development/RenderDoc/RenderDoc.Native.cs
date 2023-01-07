namespace SpaceGame.Development.RenderDoc;

public partial class RenderDoc
{
    internal unsafe delegate int GetRenderDocApi(RenderDocVersion version, void** outAPIPointers);

    internal unsafe delegate void GetAPIVersionDelegate(int* major, int* minor, int* patch);

    internal delegate int SetCaptureOptionU32Delegate(CaptureOption opt, uint val);

    internal delegate int SetCaptureOptionF32Delegate(CaptureOption opt, float val);

    internal delegate uint GetCaptureOptionU32Delegate(CaptureOption opt);

    internal delegate float GetCaptureOptionF32Delegate(CaptureOption opt);

    internal unsafe delegate void SetFocusToggleKeysDelegate(InputButton* keys, int num);

    internal unsafe delegate void SetCaptureKeysDelegate(InputButton* keys, int num);

    internal delegate uint GetOverlayBitsDelegate();

    internal delegate void MaskOverlayBitsDelegate(uint and, uint or);

    internal delegate void ShutdownDelegate();

    internal delegate void UnloadCrashHandlerDelegate();

    internal unsafe delegate void SetCaptureFilePathTemplateDelegate(byte* pathTemplate);

    internal unsafe delegate byte* GetCaptureFilePathTemplateDelegate();

    internal delegate uint GetNumCapturesDelegate();

    internal unsafe delegate uint GetCaptureDelegate(uint idx, char* filename, uint* pathLength, ulong* timestamp);

    internal unsafe delegate void SetCaptureFileCommentsDelegate(byte* filePath, byte* comments);

    internal delegate uint IsTargetControlConnectedDelegate();

    internal unsafe delegate uint LaunchReplayUIDelegate(uint connectTargetControl, byte* cmdline);

    internal unsafe delegate void SetActiveWindowDelegate(void* device, void* wndHandle);

    internal delegate void TriggerCaptureDelegate();

    internal delegate void TriggerMultiFrameCaptureDelegate(uint numFrames);

    internal unsafe delegate void StartFrameCaptureDelegate(void* device, void* wndHandle);

    internal delegate uint IsFrameCapturingDelegate();

    internal unsafe delegate uint EndFrameCaptureDelegate(void* device, void* wndHandle);

    internal unsafe delegate uint DiscardFrameCaptureDelegate(void* device, void* wndHandle);
}