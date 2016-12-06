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
        public string[] Textures { get; set; }

        public int GetRequiredTextureCount()
        {
            return 1;
        }

        public int GetElementType()
        {
            return 12;
        }

        public int GetElementSubType()
        {
            var vcount = Vertices.Count - 1;
            return (vcount > Constants.MaxSubtype) ? 0 : vcount;
        }

        public void Read(ref BinaryReader reader, int subtype, PSDLFile parent)
        {
            var numSections = (ushort)subtype;
            if (numSections == 0)
                numSections = reader.ReadUInt16();

            Height = parent.Floats[reader.ReadUInt16()];

            for (var i = 0; i < numSections + 1; i++)
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
