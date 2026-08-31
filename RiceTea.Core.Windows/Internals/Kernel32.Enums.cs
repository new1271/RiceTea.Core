using System;

namespace RiceTea.Core.Windows.Internals;

[Flags]
internal enum ProcessCreationFlags : uint
{
    DebugProcess = 0x00000001,
    DebugOnlyThisProcess = 0x00000002,
    CreateSuspended = 0x00000004,
    DetachedProcess = 0x00000008,
    CreateNewConsole = 0x00000010,

    NormalPriorityClass = 0x00000020,
    IdlePriorityClass = 0x00000040,
    HighPriorityClass = 0x00000080,
    RealTimePriorityClass = 0x00000100,

    CreateNewProcessGroup = 0x00000200,
    CreateUnicodeEnvironment = 0x00000400,
    CreateSeparateWowVdm = 0x00000800,
    CreateSharedWowVdm = 0x00001000,
    CreateForceDos = 0x00002000,

    BelowNormalPriorityClass = 0x00004000,
    AboveNormalPriorityClass = 0x00008000,

    InheritParentAffinity = 0x00010000,
    [Obsolete("Deprecated, use InheritParentAffinity instead.")]
    InheritCallerPriority = 0x00020000,
    CreateProtectedProcess = 0x00040000,
    ExtendedStartupInfoPresent = 0x00080000,

    ProcessModeBackgroundBegin = 0x00100000,
    ProcessModeBackgroundEnd = 0x00200000,
    CreateSecureProcess = 0x00400000,

    CreateBreakawayFromJob = 0x01000000,
    CreatePreserveCodeAuthzLevel = 0x02000000,
    CreateDefaultErrorMode = 0x04000000,
    CreateNoWindow = 0x08000000,

    ProfileUser = 0x10000000,
    ProfileKernel = 0x20000000,
    ProfileServer = 0x40000000,
    CreateIgnoreSystemDefault = 0x80000000
}

[Flags]
internal enum StartupInfoFlags : uint
{
    UseShowWindow = 0x00000001,
    UseSize = 0x00000002,
    UsePosition = 0x00000004,
    UseCountChars = 0x00000008,
    UseFillAttribute = 0x00000010,
    RunFullscreen = 0x00000020, // ignored for non-x86 platforms
    ForceOnFeedback = 0x00000040,
    ForceOffFeedback = 0x00000080,
    UseStdHandles = 0x00000100,
    UseHotKey = 0x00000200,
    TitleIsLinkName = 0x00000800,
    TitleIsAppId = 0x00001000,
    PreventPinning = 0x00002000,
    UntrustedSource = 0x00008000
}

[Flags]
internal enum GenericAccessRights : uint
{
    Read = 0x80000000U,
    Write = 0x40000000U,
    Execute = 0x20000000U,
    All = 0x10000000U
}

