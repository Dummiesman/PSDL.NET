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
            ClosedEnd = 2,
            ClosedStart = 1,
        }

        public List<Vertex> Vertices = new List<Vertex>();
        public ushort Value;
        public byte Type;
        public byte Flags;
        public string DividerTexture;

        public string[] Textures { get; set; }

        public int GetRequiredTextureCount()
        {
            return 4;
        }

        public int GetElementType()
        {
            return 8;
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
            Flags = (byte)((byte)(flagType << 3) >> 3);
            Type = (byte)(flagType & 7);
            DividerTexture = parent.GetTextureFromCache(reader.ReadByte()); //texture for divider what the fuck Angel!?
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

            writer.Write((byte)0);
            writer.Write((byte)0);
            writer.Write(Value);

            //write indices
            for (var i = 0; i < Vertices.Count; i++)
            {
                writer.Write((ushort)parent.Vertices.IndexOf(Vertices[i]));
            }
        }

    }
}
