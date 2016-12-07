using System.Collections.Generic;
using System.IO;


namespace PSDL.Elements
{
    public class WalkwayElement : IPSDLElement
    {
        public List<Vertex> Vertices = new List<Vertex>();
        public string[] Textures { get; set; }

        public int GetRequiredTextureCount()
        {
            return 1;
        }

        public int GetElementType()
        {
            return (int)ElementType.Walkway;
        }

        public int GetElementSubType()
        {
            var segmentCount = Vertices.Count / 2;
            return (segmentCount > Constants.MaxSubtype) ? 0 : segmentCount;
        }

        public void Read(BinaryReader reader, int subtype, PSDLFile parent)
        {
            var numSections = (ushort)subtype;
            if (numSections == 0)
                numSections = reader.ReadUInt16();

            for (var i = 0; i < numSections; i++)
            {
                for (var j = 0; j < 2; j++)
                {
                    var vertexIndex = reader.ReadUInt16();
                    Vertices.Add(parent.Vertices[vertexIndex]);
                }
            }
        }

        public void Save(BinaryWriter writer, PSDLFile parent)
        {
            //write count if applicable
            var subtype = GetElementSubType();
            if (subtype == 0)
            {
                writer.Write((ushort)(Vertices.Count / 2));
            }

            //write indices
            for (var i = 0; i < Vertices.Count; i++)
            {
                writer.Write((ushort)parent.Vertices.IndexOf(Vertices[i]));
            }
        }

        //API
        public void AddRow(Vertex leftSide, Vertex rightSide)
        {
            Vertices.AddRange(new []{leftSide, rightSide});
        }

        //Constructors
        public WalkwayElement(string texture, IEnumerable<Vertex> vertices)
        {
            Textures = new []{ texture };
            Vertices.AddRange(vertices);
        }

        public WalkwayElement()
        {
            //But nobody came
        }
    }
}
