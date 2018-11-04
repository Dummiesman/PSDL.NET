using System;
using System.Collections.Generic;
using System.IO;

namespace PSDL.Elements
{
    public class RoadElement : SDLElementBase, IGeometricSDLElement, ISDLElement
    {
        public List<Vertex> Vertices = new List<Vertex>();

        //IGeometricSDLElement
        public Vertex[] GetVertices() => Vertices.ToArray();
        public Vertex GetVertex(int index) => Vertices[index];
        public void SetVertex(int index, Vertex vertex) => Vertices[index] = vertex;
        public int GetVertexCount() => Vertices.Count;
        public void RemoveVertexAt(int idx) => throw new NotImplementedException();
        public void AddVertex() => throw new NotImplementedException();
        public void InsertVertex(int idx, Vertex vtx) => throw new NotImplementedException();
        public void InsertVertex(int idx) => throw new NotImplementedException();

        //interface
        public ElementType Type => ElementType.Road;
        public int Subtype
        {
            get
            {
                var segmentCount = Vertices.Count / 4;
                return (segmentCount > Constants.MaxSubtype) ? 0 : segmentCount;
            }
        }
        public int RequiredTextureCount => 3;

        public void Read(BinaryReader reader, int subtype, PSDLFile parent)
        {
            var numSections = (ushort)subtype;
            if (numSections == 0)
                numSections = reader.ReadUInt16();

            for (var i = 0; i < numSections * 4; i++)
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
                writer.Write((ushort)(Vertices.Count / 4));
            }

            //write indices
            for (var i = 0; i < Vertices.Count; i++)
            {
                writer.Write((ushort)parent.Vertices.IndexOf(Vertices[i]));
            }
        }

        //API
        public void SetTexture(RoadTextureType type, string texture)
        {
            Textures[(int) type] = texture;
        }

        public string GetTexture(RoadTextureType type)
        {
            return Textures[(int)type];
        }

        public int RowCount => Vertices.Count / 4;
        public void DeleteSidewalk(SidewalkRemovalMode mode)
        {
            //mm2 says if vert[0] == vert[1] then don't draw the sidewalk
            for (int i = 0; i < RowCount; i++)
            {
                var row = GetRow(i);
                if (mode == SidewalkRemovalMode.MoveRoadOutwards)
                {
                    row[1] = row[0];
                    row[2] = row[3];
                }
                else
                {
                    row[0] = row[1];
                    row[3] = row[2];
                }
                SetRow(i, row);
            }
        }

        public void AddRow(Vertex sidewalkOuterLeft, Vertex sidewalkInnerLeft, Vertex sidewalkInnerRight,
            Vertex sidewalkOuterRight)
        {
            Vertices.AddRange(new []{sidewalkOuterLeft, sidewalkInnerLeft, sidewalkInnerRight, sidewalkOuterRight});
        }

        public void AddRow(Vertex[] vertices)
        {
            if(vertices.Length != 4)
                throw new Exception("Wrong amount of vertices for a row.");
            AddRow(vertices[0], vertices[1], vertices[2], vertices[3]);
        }

        public void SetRow(int rowId, Vertex sidewalkOuterLeft, Vertex sidewalkInnerLeft, Vertex sidewalkInnerRight,
            Vertex sidewalkOuterRight)
        {
            int baseIndex = rowId * 4;
            Vertices[baseIndex] = sidewalkOuterLeft;
            Vertices[baseIndex + 1] = sidewalkInnerLeft;
            Vertices[baseIndex + 2] = sidewalkInnerRight;
            Vertices[baseIndex + 3] = sidewalkOuterRight;
        }


        public void SetRow(int rowId, Vertex[] vertices)
        {
            if (vertices.Length != 4)
                throw new Exception("Wrong amount of vertices for a row.");
            SetRow(rowId, vertices[0], vertices[1], vertices[2], vertices[3]);
        }

        public Vertex[] GetRow(int rowId)
        {
            int baseIndex = rowId * 4;
            return new []{Vertices[baseIndex], Vertices[baseIndex + 1], Vertices[baseIndex + 2], Vertices[baseIndex + 3]};
        }

        public Vertex GetRowCenterPoint(int rowId)
        {
            var row = GetRow(rowId);
            return (row[1] + row[2]) / 2;
        }

        //Constructors
        public RoadElement(string roadTexture, string sidewalkTexture, string LODTexture, IEnumerable<Vertex> vertices)
        {
            Textures = new []{roadTexture, sidewalkTexture, LODTexture};
            Vertices.AddRange(vertices);

            if((Vertices.Count % 4) != 0) 
                throw new Exception("Incorrect vertex count given to a road. A road must have a vertex count equal to a multiple of 4.");
        }

        public RoadElement(string roadTexture, string sidewalkTexture, string LODTexture)
        {
            Textures = new[] { roadTexture, sidewalkTexture, LODTexture };
        }

        public RoadElement()
        {
            //init texture array
            Textures = new string[3];
        }

    }
}
