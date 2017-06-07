using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSDL
{
    public enum DividerTextureType
    {
        Top,
        Side,
        Cap,
        SideStrips
    }

    public enum RoadTextureType
    {
        Surface,
        Sidewalk,
        LOD
    }

    public enum CrossroadEnd
    {
        First,
        Last
    }

    public enum SidewalkRemovalMode
    {
        MoveSidewalkInwards,
        MoveRoadOutwards
    }
}
