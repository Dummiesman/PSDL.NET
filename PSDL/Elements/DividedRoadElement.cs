using System;
using System.Collections.Generic;
using System.IO;


namespace PSDL.Elements
{
    public class DividedRoadElement : IPSDLElement
    {
        public enum DividedRoadType : byte
        {
            Invisible = 0,
            Flat = 1,
            Elevated = 2,
            Wedged = 3
        }

        [Flags]
        public enum DividedRoadFlags : byte
        {
            ClosedEnd = 32,
            ClosedStart = 16,
        }

        public List<Vertex> Vertices = new List<Vertex>();
        public ushort Value;
        public byte Type;
        public byte Flags;
        public byte DividerTexture;

        public string[] Textures { get; set; }
        public string[] DividerTextures { get; set; }

        public int GetRequiredTextureCount()
        {
            return 3;
        }

        public int GetElementType()
        {
            return (int)ElementType.DividedRoad;
        }

        public int GetElementSubType()
        {
            var segmentCount = Vertices.Count / 6;
            return (segmentCount > Constants.MaxSubtype) ? 0 : segmentCount;
        }

        public void Read(ref BinaryReader reader, int subtype, PSDLFile parent)
        {
            var numSections = (ushort)subtype;
            if (numSections == 0)
                numSections = reader.ReadUInt16();

            //Console.WriteLine("DIVROAD FLAGTYPE @ " + reader.BaseStream.Position.ToString());
            var flagType = reader.ReadByte();
            Flags = (byte)(flagType >> 2);
            Type = (byte)(flagType & 3);

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

        public void Save(ref BinaryWriter writer, PSDLFile parent)
        {

            //write count if applicable
            var subtype = GetElementSubType();
            if (subtype == 0)
            {
                writer.Write((ushort)(Vertices.Count / 6));
            }

            writer.Write((byte)((byte)(Flags << 2) | Type));
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
