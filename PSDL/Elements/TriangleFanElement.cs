using System.Collections.Generic;
using System.IO;


namespace PSDL.Elements
{
    public class TriangleFanElement : SDLElementBase, IGeometricSDLElement, ISDLElement
    {
        public List<Vertex> Vertices = new List<Vertex>();
        public Vertex[] GetVertices()
        {
            return Vertices.ToArray();
        }

        //interface
        public virtual ElementType Type => ElementType.TriangleFan;
        public int Subtype
        {
            get
            {
                var vcount = Vertices.Count - 2;
                return (vcount > Constants.MaxSubtype) ? 0 : vcount;
            }
        }
        public int RequiredTextureCount => 1;

        public void Read(BinaryReader reader, int subtype, PSDLFile parent)
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

        public void Save(BinaryWriter writer, PSDLFile parent)
        {
            //write count if applicable
            var subtype = Subtype;
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
        public TriangleFanElement(string texture, IEnumerable<Vertex> vertices)
        {
            Textures = new [] { texture };
            Vertices.AddRange(vertices);
        }

        public TriangleFanElement()
        {
            //But nobody came
        }

    }
}
