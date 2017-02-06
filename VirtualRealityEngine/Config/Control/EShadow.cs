using System;
using VirtualRealityEngine.Config.Control.Attributes;

namespace VirtualRealityEngine.Config.Control
{
    [Flags]
    public enum EShadow
    {
        NoShadow = 0,
        DropShadowSoftEdges = 1,
        Stroke = 2
    }
}
