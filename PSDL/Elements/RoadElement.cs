using System;
using System.Collections.Generic;
using System.IO;

using System.Text;


namespace PSDL.Elements
{
    public class RoadElement : IPSDLElement
    {
        public List<Vertex> Vertices = new List<Vertex>();

        private string[] _textures;
        public string[] Textures
        {
            get
            {
                return _textures;
            }

            set
            {
                _textures = value;
            }
        }

        public int GetRequiredTextureCount()
        {
            return 3;
        }

        int IPSDLElement.GetElementType()
        {
            return 0;
        }

        public int GetElementSubType()
        {
            int segmentCount = Vertices.Count / 4;
            if (segmentCount > Constants.MaxSubtype)
            {
                return 0;
            }
            else
            {
                return segmentCount;
            }
        }

        public void Read(ref BinaryReader reader, int subtype, PSDLFile parent)
        {
            var numSections = (ushort)subtype;
            if (numSections == 0)
                numSections = reader.ReadUInt16();

            for (var i = 0; i < numSections; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    ushort vertexIndex = reader.ReadUInt16();
                    Vertices.Add(parent.Vertices[vertexIndex]);
                }
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
            for (int i = 0; i < Vertices.Count / 4; i += 4)
            {
                if (!moveRoadOutwards)
                {
                    Vertices[i] = Vertices[i + 1];
                    Vertices[i + 2] = Vertices[i + 3];
                }
                else
                {
                    Vertices[i + 1] = Vertices[i];
                    Vertices[i + 3] = Vertices[i + 2];
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
        public RoadElement(string roadTexture, string sidewalkTexture, string LODTexture, Vertex[] vertices)
        {
            Textures = new string[] { roadTexture, sidewalkTexture, LODTexture };
            Vertices.AddRange(vertices);
        }

        public RoadElement()
        {
            //But nobody came
        }

    }
}
