using System.Collections.Generic;

//TODO : Figure out WTF all this shit is
namespace PSDL
{
    public class AIRoad
    {
        public ushort Unknown1;
        public ushort Unknown2;
        public byte Unknown3;
        public byte Unknown4;
        public List<float> Unknown5;
        public ushort Unknown6;
        public Vertex[] StartCrossroads;
        public Vertex[] EndCrossroads;
        public List<Room> PathRooms;

        public AIRoad(ushort u1, ushort u2, byte u3, byte u4, float[] u5, ushort u6, Vertex[] scr, Vertex[] ecr, Room[] crms)
        {
            Unknown1 = u1;
            Unknown2 = u2;
            Unknown3 = u3;
            Unknown4 = u4;
            Unknown6 = u6;

            Unknown5 = new List<float>();
            Unknown5.AddRange(u5);

            StartCrossroads = scr;
            EndCrossroads = ecr;

            PathRooms = new List<Room>();
            PathRooms.AddRange(crms);
        }
    }
}
