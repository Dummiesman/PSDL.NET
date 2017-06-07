using System;
using System.Collections.Generic;
using System.IO;


namespace PSDL.Elements
{
    public class DividedRoadElement : SDLElementBase, IGeometricSDLElement, ISDLElement
    {
        public enum DividerType : byte
        {
            Invisible = 0,
            Flat = 1,
            Elevated = 2,
            Wedged = 3
        }

        [Flags]
        public enum DividerFlags : byte
        {
            ClosedEnd = 32,
            ClosedStart = 16,
        }

        public List<Vertex> Vertices = new List<Vertex>();
        public Vertex[] GetVertices()
        {
            return Vertices.ToArray();
        }

        public ushort Value;
        public DividerType DivType;
        public DividerFlags DivFlags;
        public byte DividerTexture;

        public string[] DividerTextures { get; set; }

        //interface
        public ElementType Type => ElementType.DividedRoad;
        public int Subtype
        {
            get
            {
                var segmentCount = Vertices.Count / 6;
                return (segmentCount > Constants.MaxSubtype) ? 0 : segmentCount;
            }
        }
        public int RequiredTextureCount => 3;

        public void Read(BinaryReader reader, int subtype, PSDLFile parent)
        {
            var numSections = (ushort)subtype;
            if (numSections == 0)
                numSections = reader.ReadUInt16();

            var flagType = reader.ReadByte();
            DivFlags = (DividerFlags)(flagType >> 2);
            DivType = (DividerType)(flagType & 3);

            DividerTexture = reader.ReadByte(); //texture for divider what the fuck Angel!?
            DividerTextures = new[]
            {
                parent.GetTextureFromCache(DividerTexture - 1), parent.GetTextureFromCache(DividerTexture),
                parent.GetTextureFromCache(DividerTexture + 1), parent.GetTextureFromCache(DividerTexture + 2)
            };

            Value = reader.ReadUInt16();

            for (var i = 0; i < numSections * 6; i++)
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
                writer.Write((ushort)(Vertices.Count / 6));
            }

            writer.Write((byte)(((byte)DivFlags << 2) | (byte)DivType));
            writer.Write(DividerTexture);
            writer.Write(Value);

            //write indices
            for (var i = 0; i < Vertices.Count; i++)
            {
                writer.Write((ushort)parent.Vertices.IndexOf(Vertices[i]));
            }
        }

        //API
        public int RowCount => Vertices.Count / 6;
        public void DeleteSidewalk(SidewalkRemovalMode mode)
        {
            //mm2 says if vert[0] == vert[1] then don't draw the sidewalk
            for (int i = 0; i < RowCount; i++)
            {
                var row = GetRow(i);
                if (mode == SidewalkRemovalMode.MoveRoadOutwards)
                {
                    row[1] = row[0];
                    row[4] = row[5];
                }
                else
                {
                    row[0] = row[1];
                    row[5] = row[4];
                }
                SetRow(i, row);
            }
        }

        public void AddRow(Vertex sidewalkOuterLeft, Vertex sidewalkInnerLeft, Vertex sidewalkInnerRight, Vertex dividerLeft, Vertex dividerRight,
            Vertex sidewalkOuterRight)
        {
            Vertices.AddRange(new[] { sidewalkOuterLeft, sidewalkInnerLeft, dividerLeft, dividerRight, sidewalkInnerRight, sidewalkOuterRight });
        }

        public void AddRow(Vertex[] vertices)
        {
            if (vertices.Length != 6)
                throw new Exception("Wrong amount of vertices for a row.");
            AddRow(vertices[0], vertices[1], vertices[2], vertices[3], vertices[4], vertices[5]);
        }

        public void SetRow(int rowId, Vertex sidewalkOuterLeft, Vertex sidewalkInnerLeft, Vertex sidewalkInnerRight, Vertex dividerLeft, Vertex dividerRight,
            Vertex sidewalkOuterRight)
        {
            int baseIndex = rowId * 6;
            Vertices[baseIndex] = sidewalkOuterLeft;
            Vertices[baseIndex + 1] = sidewalkInnerLeft;
            Vertices[baseIndex + 2] = dividerLeft;
            Vertices[baseIndex + 3] = dividerRight;
            Vertices[baseIndex + 4] = sidewalkInnerRight;
            Vertices[baseIndex + 5] = sidewalkOuterRight;
        }


        public void SetRow(int rowId, Vertex[] vertices)
        {
            if (vertices.Length != 6)
                throw new Exception("Wrong amount of vertices for a row.");
            SetRow(rowId, vertices[0], vertices[1], vertices[2], vertices[3], vertices[4], vertices[5]);
        }

        public Vertex[] GetRow(int rowId)
        {
            int baseIndex = rowId * 6;
            return new[] { Vertices[baseIndex], Vertices[baseIndex + 1], Vertices[baseIndex + 2], Vertices[baseIndex + 3], Vertices[baseIndex + 4], Vertices[baseIndex + 5] };
        }

        public Vertex GetRowCenterPoint(int rowId)
        {
            var row = GetRow(rowId);
            return new Vertex((row[0].x + row[5].x) / 2, (row[0].y + row[5].y) / 2, (row[0].z + row[5].z) / 2);
        }


    }
}
