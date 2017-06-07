using System;
using System.IO;


namespace PSDL.Elements
{
    public class SliverElement : SDLElementBase, IGeometricSDLElement, ISDLElement
    {
        public Vertex[] Vertices = new Vertex[2];
        public Vertex[] GetVertices()
        {
            return Vertices;
        }

        public float Height;
        public float TextureScale;

        //interface
        public ElementType Type => ElementType.Sliver;
        public int Subtype => 4;
        public int RequiredTextureCount => 1;

        public void Read(BinaryReader reader, int subtype, PSDLFile parent)
        {
            Height = parent.Floats[reader.ReadUInt16()];
            TextureScale = parent.Floats[reader.ReadUInt16()];
            Vertices[0] = parent.Vertices[reader.ReadUInt16()];
            Vertices[1] = parent.Vertices[reader.ReadUInt16()];
        }

        public void Save(BinaryWriter writer, PSDLFile parent)
        {
            writer.Write((ushort)parent.Floats.IndexOf(Height));
            writer.Write((ushort)parent.Floats.IndexOf(TextureScale));
            writer.Write((ushort)parent.Vertices.IndexOf(Vertices[0]));
            writer.Write((ushort)parent.Vertices.IndexOf(Vertices[1]));
        }

        //API
        public void SetTextureTiling(int numTiles)
        {
            var v1 = new Vertex(Vertices[0].x, Height, Vertices[0].z);
            var v2 = new Vertex(Vertices[1].x, Height, Vertices[1].z);
            TextureScale = (float) numTiles / v1.Distance(v2);
        }

        //Constructors
        public SliverElement(string texture, float height, float textureScale, Vertex leftVertex, Vertex rightVertex)
        {
            Textures = new [] { texture };
            Height = height;
            TextureScale = textureScale;
            Vertices[0] = leftVertex;
            Vertices[1] = rightVertex;
        }

        public SliverElement()
        {
            //But nobody came
        }
    }
}

