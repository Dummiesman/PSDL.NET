using System;
using System.Collections.Generic;
using System.IO;

using System.Text;


namespace PSDL.Elements
{
    public class WalkwayElement : IPSDLElement
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
            return 1;
        }

        int IPSDLElement.GetElementType()
        {
            return 2;
        }

        public int GetElementSubType()
        {
            var segmentCount = Vertices.Count / 2;
            if (segmentCount > Constants.MaxSubtype)
            {
                return 0;
            }

            return segmentCount;

        }

        public void Read(ref BinaryReader reader, int subtype, PSDLFile parent)
        {
            var numSections = (ushort)subtype;
            if (numSections == 0)
                numSections = reader.ReadUInt16();

            for (var i = 0; i < numSections; i++)
            {
                for (var j = 0; j < 2; j++)
                {
                    var vertexIndex = reader.ReadUInt16();
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
                writer.Write((ushort)(Vertices.Count / 2));
            }

            //write indices
            for (var i = 0; i < Vertices.Count; i++)
            {
                writer.Write((ushort)parent.Vertices.IndexOf(Vertices[i]));
            }
        }

        //Constructors
        public WalkwayElement(string texture, Vertex[] vertices)
        {
            Textures = new string[] { texture };
            Vertices.AddRange(vertices);
        }

        public WalkwayElement()
        {
            //But nobody came
        }
    }
}
