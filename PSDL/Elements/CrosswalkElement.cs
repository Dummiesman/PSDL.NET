using System;
using System.Collections.Generic;
using System.IO;

using System.Text;


namespace PSDL.Elements
{
    public class CrosswalkElement : IPSDLElement
    {
        public Vertex[] Vertices = new Vertex[4];

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
            return 4;
        }

        public int GetElementSubType()
        {
            return 4;
        }

        public void Read(ref BinaryReader reader, int subtype, PSDLFile parent)
        {
            for (var i = 0; i < 4; i++)
            {
                Vertices[i] = parent.Vertices[reader.ReadUInt16()];
            }
        }

        public void Save(ref BinaryWriter writer, PSDLFile parent)
        {
            for (var i = 0; i < 4; i++)
            {
                writer.Write((ushort)parent.Vertices.IndexOf(Vertices[i]));
            }
        }

        //Constructors
        public CrosswalkElement(string texture, Vertex[] vertices)
        {
            Textures = new string[] { texture };
            this.Vertices = vertices;
        }

        public CrosswalkElement()
        {
            //But nobody came
        }
    }
}

