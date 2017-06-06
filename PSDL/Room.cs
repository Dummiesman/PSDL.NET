using System;
using System.Collections.Generic;
using System.Linq;
using PSDL.Elements;

namespace PSDL
{
    public class Room
    {
        public List<PerimeterPoint> Perimeter;
        public List<ISDLElement> Elements;
        public RoomFlags Flags;
        public byte PropRule;

        public Vertex[] GetCrossroadVertices(CrossroadEnd end)
        {
            var road = FindElementOfType<RoadElement>() as RoadElement;
            var divRoad = FindElementOfType<DividedRoadElement>() as DividedRoadElement;
            var walkway = FindElementOfType<WalkwayElement>() as WalkwayElement;

            if (road == null && divRoad == null && walkway == null)
                throw new Exception("Cannot get crossroad vertices for a room without a road");

            //return based on what we have
            if (road != null)
            {
                if (end == CrossroadEnd.First)
                {
                    return new[] {road.Vertices[0], road.Vertices[1], road.Vertices[2], road.Vertices[3]};
                }
                else
                {
                    int vertCount = road.Vertices.Count;
                    return new[] { road.Vertices[vertCount - 4], road.Vertices[vertCount - 3], road.Vertices[vertCount - 2], road.Vertices[vertCount - 1] };
                }
                
            }
            else if (divRoad != null)
            {
                if (end == CrossroadEnd.First)
                {
                    return new[] {divRoad.Vertices[0], divRoad.Vertices[1], divRoad.Vertices[4], divRoad.Vertices[5]};
                }
                else
                {
                    int vertCount = divRoad.Vertices.Count;
                    return new[] { divRoad.Vertices[vertCount - 6], divRoad.Vertices[vertCount - 5], divRoad.Vertices[vertCount - 2], divRoad.Vertices[vertCount - 1] };
                }
            }
            else
            {
                if (end == CrossroadEnd.First)
                {
                    return new[] {walkway.Vertices[0], walkway.Vertices[0], walkway.Vertices[1], walkway.Vertices[1]};
                }
                else
                {
                    int vertCount = walkway.Vertices.Count;
                    return new[] { walkway.Vertices[vertCount - 2], walkway.Vertices[vertCount - 2], walkway.Vertices[vertCount - 1], walkway.Vertices[vertCount - 1] };
                }
            }
        }


        public bool VerifyForPropulation()
        {
            foreach (var elements in Elements)
            {
                foreach (var texture in elements.Textures)
                {
                    if (string.IsNullOrEmpty((texture)))
                        return false;
                }
            }
            return true;
        }

        public ISDLElement FindElementOfType<T>()
        {
            foreach (var element in Elements)
            {
                if (element is T)
                    return element;
            }
            return null;
        }

        public ISDLElement[] FindElementsOfType<T>()
        {
            var elementCollection = new List<ISDLElement>();
            foreach (var element in Elements)
            {
                if (element is T)
                    elementCollection.Add(element);
            }
            return elementCollection.ToArray();
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
                if (el is IGeometricSDLElement)
                {
                    var GeometricElement = (IGeometricSDLElement) el;
                    gatheredVertices.UnionWith(GeometricElement.GetVertices());
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
                switch (el.Type)
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
            Elements = new List<ISDLElement>();
            Perimeter = new List<PerimeterPoint>();
        }

        public Room(IEnumerable<ISDLElement> roomElements, IEnumerable<PerimeterPoint> perimeterPoints, byte propRule = 0, RoomFlags flags = 0) : this(){
            Elements.AddRange(roomElements);
            Perimeter.AddRange(perimeterPoints);
            Flags = flags;
            PropRule = propRule;
        }
    }
}
