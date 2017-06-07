using System;
using System.Collections.Generic;
using System.IO;


namespace PSDL.Elements
{
    public class WalkwayElement : SDLElementBase, IGeometricSDLElement, ISDLElement
    {
        public List<Vertex> Vertices = new List<Vertex>();
        public Vertex[] GetVertices()
        {
            return Vertices.ToArray();
        }

        //interface
        public ElementType Type => ElementType.Walkway;
        public int Subtype
        {
            get
            {
                var segmentCount = Vertices.Count / 2;
                return (segmentCount > Constants.MaxSubtype) ? 0 : segmentCount;
            }
        }
        public int RequiredTextureCount => 1;

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
            var subtype = Subtype;
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
        public int RowCount => Vertices.Count / 2;
        public void AddRow(Vertex leftSide, Vertex rightSide)
        {
            Vertices.AddRange(new []{leftSide, rightSide});
        }

        public void AddRow(Vertex[] vertices)
        {
            if (vertices.Length != 2)
                throw new Exception("Wrong amount of vertices for a row.");
            AddRow(vertices[0], vertices[1]);
        }

        public void SetRow(int rowId, Vertex leftSide, Vertex rightSide)
        {
            int baseIndex = rowId * 2;
            Vertices[baseIndex] = leftSide;
            Vertices[baseIndex + 1] = rightSide;
        }


        public void SetRow(int rowId, Vertex[] vertices)
        {
            if (vertices.Length != 2)
                throw new Exception("Wrong amount of vertices for a row.");
            SetRow(rowId, vertices[0], vertices[1]);
        }

        public Vertex[] GetRow(int rowId)
        {
            int baseIndex = rowId * 2;
            return new[] { Vertices[baseIndex], Vertices[baseIndex + 1]};
        }

        public Vertex GetRowCenterPoint(int rowId)
        {
            var row = GetRow(rowId);
            return new Vertex((row[0].x + row[1].x) / 2, (row[0].y + row[1].y) / 2, (row[0].z + row[1].z) / 2);
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
