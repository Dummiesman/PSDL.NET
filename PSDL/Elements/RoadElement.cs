using System.Collections.Generic;
using System.IO;

namespace PSDL.Elements
{
    public class RoadElement : IPSDLElement
    {
        public List<Vertex> Vertices = new List<Vertex>();
        public string[] Textures { get; set; }

        public int GetRequiredTextureCount()
        {
            return 3;
        }

        public int GetElementType()
        {
            return 0;
        }

        public int GetElementSubType()
        {
            var segmentCount = Vertices.Count / 4;
            return (segmentCount > Constants.MaxSubtype) ? 0 : segmentCount;
        }

        public void Read(ref BinaryReader reader, int subtype, PSDLFile parent)
        {
            var numSections = (ushort)subtype;
            if (numSections == 0)
                numSections = reader.ReadUInt16();

            for (var i = 0; i < numSections * 4; i++)
            {
                var vertexIndex = reader.ReadUInt16();
                Vertices.Add(parent.Vertices[vertexIndex]);
            }
        }

        public void Save(ref BinaryWriter writer, PSDLFile parent)
        {
            //write count if applicable
            var subtype = GetElementSubType();
            if (subtype == 0)
            {
                writer.Write((ushort)(Vertices.Count / 4));
            }

            //write indices
            for (var i = 0; i < Vertices.Count; i++)
            {
                writer.Write((ushort)parent.Vertices.IndexOf(Vertices[i]));
            }
        }

        //API
        public void DeleteSidewalk(bool moveRoadOutwards = false)
        {
            //mm2 says if vert[0] == vert[1] then don't draw the sidewalk
            for (var i = 0; i < Vertices.Count; i += 4)
            {
                if (!moveRoadOutwards)
                {
                    Vertices[i] = Vertices[i + 1];
                    Vertices[i + 3] = Vertices[i + 2];
                }
                else
                {
                    Vertices[i + 1] = Vertices[i];
                    Vertices[i + 2] = Vertices[i + 3];
                }
            }
        }

        //API
        public void AddRow(Vertex sidewalkOuterLeft, Vertex sidewalkInnerLeft, Vertex sidewalkInnerRight,
            Vertex sidewalkOuterRight)
        {
            Vertices.AddRange(new Vertex[] {sidewalkOuterLeft, sidewalkInnerLeft, sidewalkInnerRight, sidewalkOuterRight});
        }

        //Constructors
        public RoadElement(string roadTexture, string sidewalkTexture, string LODTexture, IEnumerable<Vertex> vertices)
        {
            Textures = new []{roadTexture, sidewalkTexture, LODTexture};
            Vertices.AddRange(vertices);
        }

        public RoadElement(string roadTexture, string sidewalkTexture, string LODTexture)
        {
            Textures = new[] { roadTexture, sidewalkTexture, LODTexture };
        }

        public RoadElement()
        {
            //But nobody came
        }

    }
}
