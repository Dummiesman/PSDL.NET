using System.Collections.Generic;

//TODO : Figure out WTF all this shit is
namespace PSDL
{
    public class AIRoad
    {
        public ushort Unknown1;
        public ushort Unknown2;
        public List<float> Unknown3;
        public List<float> Unknown4;
        public ushort Unknown5;
        public List<Room> Rooms;

        public AIRoad(ushort u1, ushort u2, IEnumerable<float> u3, IEnumerable<float> u4, ushort u5, IEnumerable<Room> rooms)
        {
            Unknown1 = u1;
            Unknown2 = u2;
            Unknown5 = u5;

            Unknown3 = new List<float>();
            Unknown3.AddRange(u3);

            Unknown4 = new List<float>();
            Unknown3.AddRange(u4);

            Rooms = new List<Room>();
            Rooms.AddRange(rooms);
        }
    }
}
