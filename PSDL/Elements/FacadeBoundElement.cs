using System;
using System.Collections.Generic;
using System.IO;

using System.Text;


namespace PSDL.Elements
{
    public class FacadeBoundElement : IPSDLElement
    {
        public Vertex[] Vertices = new Vertex[2];
        public ushort SunAngle;
        public float Height;
        public string[] Textures { get; set; }

        public int GetRequiredTextureCount()
        {
            return 0;
        }

        int IPSDLElement.GetElementType()
        {
            return 7;
        }

        public int GetElementSubType()
        {
            return 4;
        }

        public void Read(ref BinaryReader reader, int subtype, PSDLFile parent)
        {
            SunAngle = reader.ReadUInt16();
            Height = parent.Floats[reader.ReadUInt16()];
            Vertices[0] = parent.Vertices[reader.ReadUInt16()];
            Vertices[1] = parent.Vertices[reader.ReadUInt16()];
        }

        public void Save(ref BinaryWriter writer, PSDLFile parent)
        {
            writer.Write(SunAngle);
            writer.Write((ushort)parent.Floats.IndexOf(Height));
            writer.Write((ushort)parent.Vertices.IndexOf(Vertices[0]));
            writer.Write((ushort)parent.Vertices.IndexOf(Vertices[1]));
        }

        //Constructors
        public FacadeBoundElement(float sunAngle, float height, Vertex leftVertex, Vertex rightVertex)
        {
            //clamp angle
            if (sunAngle > 1.0f)
                sunAngle = 1.0f;
            
            SunAngle = (ushort)(sunAngle * 255);
            Height = height;
            Vertices[0] = leftVertex;
            Vertices[1] = rightVertex;
        }

        public FacadeBoundElement()
        {
            //But nobody came
        }
    }
}

