namespace SpaceGame.Development.RenderDoc;

public partial class RenderDoc
{
    internal enum RenderDocVersion
    {
        API_Version_1_0_0 = 10000, // RENDERDOC_API_1_0_0 = 1 00 00
        API_Version_1_0_1 = 10001, // RENDERDOC_API_1_0_1 = 1 00 01
        API_Version_1_0_2 = 10002, // RENDERDOC_API_1_0_2 = 1 00 02
        API_Version_1_1_0 = 10100, // RENDERDOC_API_1_1_0 = 1 01 00
        API_Version_1_1_1 = 10101, // RENDERDOC_API_1_1_1 = 1 01 01
        API_Version_1_1_2 = 10102, // RENDERDOC_API_1_1_2 = 1 01 02
        API_Version_1_2_0 = 10200, // RENDERDOC_API_1_2_0 = 1 02 00
        API_Version_1_3_0 = 10300, // RENDERDOC_API_1_3_0 = 1 03 00
        API_Version_1_4_0 = 10400, // RENDERDOC_API_1_4_0 = 1 04 00
        API_Version_1_4_1 = 10401, // RENDERDOC_API_1_4_1 = 1 04 01
    }

    internal enum CaptureOption
    {
        // Allow the application to enable vsync
        //
        // Default - enabled
        //
        // 1 - The application can enable or disable vsync at will
        // 0 - vsync is force disabled
        AllowVSync = 0,

        // Allow the application to enable fullscreen
        //
        // Default - enabled
        //
        // 1 - The application can enable or disable fullscreen at will
        // 0 - fullscreen is force disabled
        AllowFullscreen = 1,

        // Record API debugging events and messages
        //
        // Default - disabled
        //
        // 1 - Enable built-in API debugging features and records the results into
        //     the capture, which is matched up with events on replay
        // 0 - no API debugging is forcibly enabled
        APIValidation = 2,
        DebugDeviceMode = 2, // deprecated name of this enum

        // Capture CPU callstacks for API events
        //
        // Default - disabled
        //
        // 1 - Enables capturing of callstacks
        // 0 - no callstacks are captured
        CaptureCallstacks = 3,

        // When capturing CPU callstacks, only capture them from drawcalls.
        // This option does nothing without the above option being enabled
        //
        // Default - disabled
        //
        // 1 - Only captures callstacks for drawcall type API events.
        //     Ignored if CaptureCallstacks is disabled
        // 0 - Callstacks, if enabled, are captured for every event.
        CaptureCallstacksOnlyDraws = 4,

        // Specify a delay in seconds to wait for a debugger to attach, after
        // creating or injecting into a process, before continuing to allow it to run.
        //
        // 0 indicates no delay, and the process will run immediately after injection
        //
        // Default - 0 seconds
        //
        DelayForDebugger = 5,

        // Verify buffer access. This includes checking the memory returned by a Map() call to
        // detect any out-of-bounds modification, as well as initialising buffers with undefined contents
        // to a marker value to catch use of uninitialised memory.
        //
        // NOTE: This option is only valid for OpenGL and D3D11. Explicit APIs such as D3D12 and Vulkan do
        // not do the same kind of interception & checking and undefined contents are really undefined.
        //
        // Default - disabled
        //
        // 1 - Verify buffer access
        // 0 - No verification is performed, and overwriting bounds may cause crashes or corruption in
        //     RenderDoc.
        VerifyBufferAccess = 6,

        // The old name for VerifyBufferAccess was VerifyMapWrites.
        // This option now controls the filling of uninitialised buffers with 0xdddddddd which was
        // previously always enabled
        VerifyMapWrites = VerifyBufferAccess,

        // Hooks any system API calls that create child processes, and injects
        // RenderDoc into them recursively with the same options.
        //
        // Default - disabled
        //
        // 1 - Hooks into spawned child processes
        // 0 - Child processes are not hooked by RenderDoc
        HookIntoChildren = 7,

        // By default RenderDoc only includes resources in the final capture necessary
        // for that frame, this allows you to override that behaviour.
        //
        // Default - disabled
        //
        // 1 - all live resources at the time of capture are included in the capture
        //     and available for inspection
        // 0 - only the resources referenced by the captured frame are included
        RefAllResources = 8,

        // **NOTE**: As of RenderDoc v1.1 this option has been deprecated. Setting or
        // getting it will be ignored, to allow compatibility with older versions.
        // In v1.1 the option acts as if it's always enabled.
        //
        // By default RenderDoc skips saving initial states for resources where the
        // previous contents don't appear to be used, assuming that writes before
        // reads indicate previous contents aren't used.
        //
        // Default - disabled
        //
        // 1 - initial contents at the start of each captured frame are saved, even if
        //     they are later overwritten or cleared before being used.
        // 0 - unless a read is detected, initial contents will not be saved and will
        //     appear as black or empty data.
        SaveAllInitials = 9,

        // In APIs that allow for the recording of command lists to be replayed later,
        // RenderDoc may choose to not capture command lists before a frame capture is
        // triggered, to reduce overheads. This means any command lists recorded once
        // and replayed many times will not be available and may cause a failure to
        // capture.
        //
        // NOTE: This is only true for APIs where multithreading is difficult or
        // discouraged. Newer APIs like Vulkan and D3D12 will ignore this option
        // and always capture all command lists since the API is heavily oriented
        // around it and the overheads have been reduced by API design.
        //
        // 1 - All command lists are captured from the start of the application
        // 0 - Command lists are only captured if their recording begins during
        //     the period when a frame capture is in progress.
        CaptureAllCmdLists = 10,

        // Mute API debugging output when the API validation mode option is enabled
        //
        // Default - enabled
        //
        // 1 - Mute any API debug messages from being displayed or passed through
        // 0 - API debugging is displayed as normal
        DebugOutputMute = 11,

        // Option to allow vendor extensions to be used even when they may be
        // incompatible with RenderDoc and cause corrupted replays or crashes.
        //
        // Default - inactive
        //
        // No values are documented, this option should only be used when absolutely
        // necessary as directed by a RenderDoc developer.
        AllowUnsupportedVendorExtensions = 12,
    }

    internal enum InputButton
    {
        // '0' - '9' matches ASCII values
        Key_0 = 0x30,
        Key_1 = 0x31,
        Key_2 = 0x32,
        Key_3 = 0x33,
        Key_4 = 0x34,
        Key_5 = 0x35,
        Key_6 = 0x36,
        Key_7 = 0x37,
        Key_8 = 0x38,
        Key_9 = 0x39,

        // 'A' - 'Z' matches ASCII values
        Key_A = 0x41,
        Key_B = 0x42,
        Key_C = 0x43,
        Key_D = 0x44,
        Key_E = 0x45,
        Key_F = 0x46,
        Key_G = 0x47,
        Key_H = 0x48,
        Key_I = 0x49,
        Key_J = 0x4A,
        Key_K = 0x4B,
        Key_L = 0x4C,
        Key_M = 0x4D,
        Key_N = 0x4E,
        Key_O = 0x4F,
        Key_P = 0x50,
        Key_Q = 0x51,
        Key_R = 0x52,
        Key_S = 0x53,
        Key_T = 0x54,
        Key_U = 0x55,
        Key_V = 0x56,
        Key_W = 0x57,
        Key_X = 0x58,
        Key_Y = 0x59,
        Key_Z = 0x5A,

        // leave the rest of the ASCII range free
        // in case we want to use it later
        Key_NonPrintable = 0x100,

        Key_Divide,
        Key_Multiply,
        Key_Subtract,
        Key_Plus,

        Key_F1,
        Key_F2,
        Key_F3,
        Key_F4,
        Key_F5,
        Key_F6,
        Key_F7,
        Key_F8,
        Key_F9,
        Key_F10,
        Key_F11,
        Key_F12,

        Key_Home,
        Key_End,
        Key_Insert,
        Key_Delete,
        Key_PageUp,
        Key_PageDn,

        Key_Backspace,
        Key_Tab,
        Key_PrtScrn,
        Key_Pause,

        Key_Max,
    }

    internal enum OverlayBits : uint
    {
        // This single bit controls whether the overlay is enabled or disabled globally
        Enabled = 0x1,

        // Show the average framerate over several seconds as well as min/max
        FrameRate = 0x2,

        // Show the current frame number
        FrameNumber = 0x4,

        // Show a list of recent captures, and how many captures have been made
        CaptureList = 0x8,

        // Default values for the overlay mask
        Default = (Enabled | FrameRate | FrameNumber | CaptureList),

        // Enable all bits
        All = ~0U,

        // Disable all bits
        None = 0,
    }
}