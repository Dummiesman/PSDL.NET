using System;
using System.Collections.Generic;
using System.IO;

using System.Text;


namespace PSDL.Elements
{
    public class TriangleFanElement : IPSDLElement
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

        public virtual int GetElementType()
        {
            return 6;
        }

        public int GetElementSubType()
        {
            var vcount = Vertices.Count - 2;
            if (vcount > Constants.MaxSubtype)
            {
                return 0;
            }
            else
            {
                return vcount;
            }
        }

        public void Read(ref BinaryReader reader, int subtype, PSDLFile parent)
        {
            var numSections = (ushort)subtype;
            if (numSections == 0)
                numSections = reader.ReadUInt16();

            for (var i = 0; i < numSections + 2; i++)
            {
                var vertexIndex = reader.ReadUInt16();
                Vertices.Add(parent.Vertices[vertexIndex]);
            }
        }

        public void Save(ref BinaryWriter writer, PSDLFile parent)
        {
            //write count if applicable
            int subtype = GetElementSubType();
            if (subtype == 0)
            {
                writer.Write((ushort)(Vertices.Count - 2));
            }

            //write indices
            for (var i = 0; i < Vertices.Count; i++)
            {
                writer.Write((ushort)parent.Vertices.IndexOf(Vertices[i]));
            }
        }

        //Constructors
        public TriangleFanElement(string texture, Vertex[] vertices)
        {
            Textures = new string[] { texture };
            this.Vertices.AddRange(vertices);
        }

        public TriangleFanElement()
        {
            //But nobody came
        }

    }
}
