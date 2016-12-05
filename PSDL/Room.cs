using System;
using System.Collections.Generic;

using System.Text;


namespace PSDL
{
    public class Room
    {
        public List<PerimeterPoint> Perimeter;
        public List<IPSDLElement> Elements;
        public RoomFlags Flags;
        public byte PropRule;
        
        private void BasicCTOR()
        {
            Elements = new List<IPSDLElement>();
            Perimeter = new List<PerimeterPoint>();
        }

        public Room(IEnumerable<IPSDLElement> roomElements, IEnumerable<PerimeterPoint> perimeterPoints, byte propRule = 0, byte flags = 0){
            BasicCTOR();
            Elements.AddRange(roomElements);
            Perimeter.AddRange(perimeterPoints);
            Flags = (RoomFlags)flags;
            PropRule = propRule;
        }

        public Room()
        {
            BasicCTOR();
        }
    }
}
