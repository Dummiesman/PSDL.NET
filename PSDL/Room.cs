using System.Collections.Generic;
using System.Linq;
using PSDL.Elements;


namespace PSDL
{
    public class Room
    {
        public List<PerimeterPoint> Perimeter;
        public List<IPSDLElement> Elements;
        public RoomFlags Flags;
        public byte PropRule;

        public IPSDLElement FindElementOfType<T>()
        {
            foreach (var element in Elements)
            {
                if (element is T)
                    return element;
            }
            return null;
        }

        public IEnumerable<Vertex> GatherPerimeterVertices()
        {
            var vertices = new Vertex[Perimeter.Count];

            for (var i = 0; i < Perimeter.Count; i++)
            {
                vertices[i] = Perimeter[i].Vertex;
            }

            return vertices;
        }

        public IEnumerable<Vertex> GatherVertices(bool includePerimeter = true)
        {
            var gatheredVertices = new HashSet<Vertex>();

            foreach (var el in Elements)
            {
                var type = (ElementType)el.GetElementType();
                switch (type)
                {
                    case ElementType.Road:
                        {
                            var cacheElement = (RoadElement)el;
                            gatheredVertices.UnionWith(cacheElement.Vertices);
                            break;
                        }
                    case ElementType.TriangleFan:
                    case ElementType.CulledTriangleFan:
                        {
                            var cacheElement = (TriangleFanElement)el;
                            gatheredVertices.UnionWith(cacheElement.Vertices);
                            break;
                        }
                    case ElementType.Crosswalk:
                        {
                            var cacheElement = (CrosswalkElement)el;
                            gatheredVertices.UnionWith(cacheElement.Vertices);
                            break;
                        }
                    case ElementType.DividedRoad:
                        {
                            var cacheElement = (DividedRoadElement)el;
                            gatheredVertices.UnionWith(cacheElement.Vertices);
                            break;
                        }
                    case ElementType.FacadeBound:
                        {
                            var cacheElement = (FacadeBoundElement)el;
                            gatheredVertices.UnionWith(cacheElement.Vertices);
                            break;
                        }
                    case ElementType.Facade:
                        {
                            var cacheElement = (FacadeElement)el;
                            gatheredVertices.UnionWith(cacheElement.Vertices);
                            break;
                        }
                    case ElementType.RoofTriangleFan:
                        {
                            var cacheElement = (RoofTriangleFanElement)el;
                            gatheredVertices.UnionWith(cacheElement.Vertices);
                            break;
                        }
                    case ElementType.Walkway:
                        {
                            var cacheElement = (WalkwayElement)el;
                            gatheredVertices.UnionWith(cacheElement.Vertices);
                            break;
                        }
                    case ElementType.Sliver:
                        {
                            var cacheElement = (SliverElement)el;
                            gatheredVertices.UnionWith(cacheElement.Vertices);
                            break;
                        }
                    case ElementType.SidewalkStrip:
                        {
                            var cacheElement = (SidewalkStripElement)el;
                            gatheredVertices.UnionWith(cacheElement.Vertices);
                            break;
                        }
                }
            }

            if (includePerimeter)
                gatheredVertices.UnionWith(GatherPerimeterVertices());

            var gatheredVertsArray = gatheredVertices.ToArray();
            gatheredVertices.Clear();
            return gatheredVertsArray;
        }

        public IEnumerable<float> GatherFloats()
        {
            HashSet<float> gatheredFloats = new HashSet<float>();
            
            foreach (var el in Elements)
            {
                var type = (ElementType)el.GetElementType();
                switch (type)
                {
                    case ElementType.FacadeBound:
                        {
                            var cacheElement = (FacadeBoundElement)el;
                            gatheredFloats.Add(cacheElement.Height);
                            break;
                        }
                    case ElementType.RoofTriangleFan:
                        {
                            var cacheElement = (RoofTriangleFanElement)el;
                            gatheredFloats.Add(cacheElement.Height);
                            break;
                        }
                    case ElementType.Facade:
                        {
                            var cacheElement = (FacadeElement)el;
                            gatheredFloats.Add(cacheElement.BottomHeight);
                            gatheredFloats.Add(cacheElement.TopHeight);
                            break;
                        }
                    case ElementType.Sliver:
                        {
                            var cacheElement = (SliverElement)el;
                            gatheredFloats.Add(cacheElement.TextureScale);
                            gatheredFloats.Add(cacheElement.Height);
                            break;
                        }
                }
            }

            var gatheredFloatsArray = gatheredFloats.ToArray();
            gatheredFloats.Clear();
            return gatheredFloatsArray;
        }

        //Constructors
        public Room()
        {
            Elements = new List<IPSDLElement>();
            Perimeter = new List<PerimeterPoint>();
        }

        public Room(IEnumerable<IPSDLElement> roomElements, IEnumerable<PerimeterPoint> perimeterPoints, byte propRule = 0, RoomFlags flags = 0) : this(){
            Elements.AddRange(roomElements);
            Perimeter.AddRange(perimeterPoints);
            Flags = (RoomFlags)flags;
            PropRule = propRule;
        }
    }
}
