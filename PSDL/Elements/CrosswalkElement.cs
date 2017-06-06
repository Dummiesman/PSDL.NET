using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PSDL.Elements
{
    public class CrosswalkElement : SDLElementBase, IGeometricSDLElement, ISDLElement
    {
        public Vertex[] Vertices = new Vertex[4];
        public Vertex[] GetVertices()
        {
            return Vertices;
        }

        //interface
        public ElementType Type => ElementType.Crosswalk;
        public int Subtype => 4;
        public int RequiredTextureCount => 1;


        public void Read(BinaryReader reader, int subtype, PSDLFile parent)
        {
            for (var i = 0; i < 4; i++)
            {
                Vertices[i] = parent.Vertices[reader.ReadUInt16()];
            }
        }

        public void Save(BinaryWriter writer, PSDLFile parent)
        {
            for (var i = 0; i < 4; i++)
            {
                writer.Write((ushort)parent.Vertices.IndexOf(Vertices[i]));
            }
        }

        //Constructors
        public CrosswalkElement(string texture, IEnumerable<Vertex> vertices)
        {
            Textures = new [] { texture };
            Vertices = vertices.ToArray();
        }

        public CrosswalkElement()
        {
            //But nobody came
        }
    }
}

