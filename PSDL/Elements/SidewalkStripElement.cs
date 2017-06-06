using System.Collections.Generic;
using System.IO;


namespace PSDL.Elements
{
    public class SidewalkStripElement : SDLElementBase, IGeometricSDLElement, ISDLElement
    {
        public List<Vertex> Vertices = new List<Vertex>();
        public Vertex[] GetVertices()
        {
            return Vertices.ToArray();
        }

        public bool IsStartCap;
        public bool IsEndCap;

        //interface
        public ElementType Type => ElementType.SidewalkStrip;
        public int Subtype
        {
            get
            {
                var bias = (IsStartCap || IsEndCap) ? 1 : 0;
                var calculatedSubtype = (Vertices.Count / 2) + bias;
                return (calculatedSubtype > Constants.MaxSubtype) ? 0 : calculatedSubtype;
            }
        }
        public int RequiredTextureCount => 1;

        public void Read(BinaryReader reader, int subtype, PSDLFile parent)
        {
            var refs = subtype;
            if (subtype == 0)
            {
                refs = reader.ReadUInt16();
            }

            var refList = new ushort[refs * 2];
            for (var i = 0; i < refs * 2; i++)
            {
                refList[i] = reader.ReadUInt16();
            }

            //cap handling
            if (refList[0] == 0 && refList[1] == 0)
            {
                IsStartCap = true;
            }
            else if (refList[0] == 1 && refList[1] == 1)
            {
                IsEndCap = true;
            }

            //read in Vertices
            var vertexStart = 0;
            if (IsEndCap || IsStartCap)
                vertexStart = 2;

            for (var i = vertexStart; i < refList.Length; i++)
            {
                Vertices.Add(parent.Vertices[refList[i]]);
            }

        }

        public void Save(BinaryWriter writer, PSDLFile parent)
        {
            var subType = Subtype;
            if (subType == 0)
            {
                var bias = 0;
                if (IsStartCap || IsEndCap)
                    bias += 1;

                bias += (Vertices.Count / 2);

                writer.Write((ushort)bias);
            }

            if (IsStartCap)
            {
                writer.Write((ushort)0);
                writer.Write((ushort)0);
            }
            else if (IsEndCap)
            {
                writer.Write((ushort)1);
                writer.Write((ushort)1);
            }

            for (var i = 0; i < Vertices.Count; i++)
            {
                writer.Write((ushort)parent.Vertices.IndexOf(Vertices[i]));
            }
        }

        //Constructurs
        public SidewalkStripElement(string texture, IEnumerable<Vertex> vertices)
        {
            Textures = new []{ texture };
            Vertices.AddRange(vertices);
        }

        public SidewalkStripElement()
        {
            //But nobody came
        }
    }
}

