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

        public AIRoad StripInvalidRooms()
        {
            var road = new AIRoad(Unknown1, Unknown2, Unknown3, Unknown4, Unknown5, null);
            foreach (var room in Rooms)
            {
                if (room.VerifyForPropulation())
                    road.Rooms.Add(room);
            }
            return road;
        }

        public AIRoad(ushort u1, ushort u2, IEnumerable<float> u3, IEnumerable<float> u4, ushort u5, IEnumerable<Room> rooms = null)
        {
            Unknown1 = u1;
            Unknown2 = u2;
            Unknown5 = u5;

            Unknown3 = new List<float>();
            Unknown3.AddRange(u3);

            Unknown4 = new List<float>();
            Unknown3.AddRange(u4);

            Rooms = new List<Room>();

            //
            if (rooms != null)
                Rooms.AddRange(rooms);
        }
    }
}
