using System.IO;


namespace PSDL.Elements
{
    public class FacadeElement : SDLElementBase, IGeometricSDLElement, ISDLElement
    {
        public Vertex[] Vertices = new Vertex[2];
        public Vertex[] GetVertices()
        {
            return Vertices;
        }

        public float BottomHeight;
        public float TopHeight;
        public short UTiling;
        public short VTiling;

        //interface
        public ElementType Type => ElementType.Facade;
        public int Subtype => 6;
        public int RequiredTextureCount => 1;


        public void Read(BinaryReader reader, int subtype, PSDLFile parent)
        {
            BottomHeight = parent.Floats[reader.ReadUInt16()];
            TopHeight = parent.Floats[reader.ReadUInt16()];
            UTiling = reader.ReadInt16();
            VTiling = reader.ReadInt16();
            Vertices[0] = parent.Vertices[reader.ReadUInt16()];
            Vertices[1] = parent.Vertices[reader.ReadUInt16()];
        }

        public void Save(BinaryWriter writer, PSDLFile parent)
        {
            writer.Write((ushort)parent.Floats.IndexOf(BottomHeight));
            writer.Write((ushort)parent.Floats.IndexOf(TopHeight));
            writer.Write(UTiling);
            writer.Write(VTiling);
            writer.Write((ushort)parent.Vertices.IndexOf(Vertices[0]));
            writer.Write((ushort)parent.Vertices.IndexOf(Vertices[1]));
        }

        //API
        public FacadeBoundElement CreateBound()
        {
            var bound = new FacadeBoundElement(0, this.TopHeight, Vertices[0], Vertices[1]);
            bound.RecalculateAngle();
            return bound;
        }

        //Constructors
        public FacadeElement(string texture, float bottomHeight, float topHeight, short uTiling, short vTiling, Vertex leftVertex, Vertex rightVertex)
        {
            Textures = new [] { texture };
            BottomHeight = bottomHeight;
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

