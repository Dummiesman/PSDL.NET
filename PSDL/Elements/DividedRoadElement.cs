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

    }
}
