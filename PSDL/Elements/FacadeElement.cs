using System;
using System.Collections.Generic;
using System.IO;

using System.Text;


namespace PSDL.Elements
{
    public class FacadeElement : IPSDLElement
    {
        public Vertex[] Vertices = new Vertex[2];
        public float BottomHeight;
        public float TopHeight;
        public short UTiling;
        public short VTiling;

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

        public int GetElementType()
        {
            return 11;
        }

        public int GetElementSubType()
        {
            return 6;
        }

        public void Read(ref BinaryReader reader, int subtype, PSDLFile parent)
        {
            BottomHeight = parent.Floats[reader.ReadUInt16()];
            TopHeight = parent.Floats[reader.ReadUInt16()];
            UTiling = reader.ReadInt16();
            VTiling = reader.ReadInt16();
            Vertices[0] = parent.Vertices[reader.ReadUInt16()];
            Vertices[1] = parent.Vertices[reader.ReadUInt16()];
        }

        public void Save(ref BinaryWriter writer, PSDLFile parent)
        {
            writer.Write((ushort)parent.Floats.IndexOf(BottomHeight));
            writer.Write((ushort)parent.Floats.IndexOf(TopHeight));
            writer.Write(UTiling);
            writer.Write(VTiling);
            writer.Write((ushort)parent.Vertices.IndexOf(Vertices[0]));
            writer.Write((ushort)parent.Vertices.IndexOf(Vertices[1]));
        }

        //Constructors
        public FacadeElement(string texture, float bottomHeight, float topHeight, short uTiling, short vTiling, Vertex leftVertex, Vertex rightVertex)
        {
            Textures = new string[] { texture };
            this.BottomHeight = bottomHeight;
            TopHeight = topHeight;
            UTiling = uTiling;
            VTiling = vTiling;
            Vertices[0] = leftVertex;
            Vertices[1] = rightVertex;
        }

        public FacadeElement()
        {
            //But nobody came
        }
    }
}

