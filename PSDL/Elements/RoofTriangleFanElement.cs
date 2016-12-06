using System;
using System.Collections.Generic;
using System.IO;

using System.Text;


namespace PSDL.Elements
{
    public class RoofTriangleFanElement : IPSDLElement
    {
        public List<Vertex> Vertices = new List<Vertex>();
        public float Height;

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
            return 12;
        }

        public int GetElementSubType()
        {
            var vcount = Vertices.Count - 1;
            if (vcount > Constants.MaxSubtype)
            {
                return 0;
            }

            return vcount;
        }

        public void Read(ref BinaryReader reader, int subtype, PSDLFile parent)
        {
            var numSections = (ushort)subtype;
            if (numSections == 0)
                numSections = reader.ReadUInt16();

            Height = parent.Floats[reader.ReadUInt16()];

            for (var i = 0; i < numSections + 1; i++)
            {
                ushort vertexIndex = reader.ReadUInt16();
                Vertices.Add(parent.Vertices[vertexIndex]);
            }
        }

        public void Save(ref BinaryWriter writer, PSDLFile parent)
        {
            //write count if applicable
            int subtype = GetElementSubType();
            if (subtype == 0)
            {
                writer.Write((ushort)(Vertices.Count - 1));
            }

            writer.Write((ushort)parent.Floats.IndexOf(Height));
            //write indices
            for (var i = 0; i < Vertices.Count; i++)
            {
                writer.Write((ushort)parent.Vertices.IndexOf(Vertices[i]));
            }
        }

        //Constructors
        public RoofTriangleFanElement(string texture, float height, IEnumerable<Vertex> vertices)
        {
            Textures = new []{ texture };
            Height = height;
            Vertices.AddRange(vertices);
        }

        public RoofTriangleFanElement()
        {
            //But nobody came
        }

    }
}
