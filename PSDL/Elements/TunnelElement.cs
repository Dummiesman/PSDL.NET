using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using System.Text;


namespace PSDL.Elements
{
    public class TunnelElement : IPSDLElement
    {
        [Flags]
        public enum TunnelFlags : ushort
        {
            LeftSide = 1,
            RightSide = 2,
            IsWall = 4,
            FlatCeiling = 8,
            ClosedStartLeft = 16,
            ClosedEndLeft = 32,
            ClosedStartRight = 64,
            ClosedEndRight = 128,
            CurvedCeiling = 256,
            OffsetStartLeft = 512,
            OffsetEndLeft = 1024,
            OffsetStartRight = 2048,
            OffsetEndRight = 4096,
            CurvedSides = 8192,
            Culled = 16384
        }

        public bool IsJunctionTunnel;
        public List<bool> JunctionWalls = new List<bool>();
        public ushort JunctionCeilingBits;

        public TunnelFlags Flags;

        public byte Height1;
        public byte Height2;
        public byte Unknown1;
        public byte Unknown2;

        public string[] Textures { get; set; }

        public int GetRequiredTextureCount()
        {
            return 6;
        }

        public int GetElementType()
        {
            return 9;
        }

        public int GetElementSubType()
        {
            return (IsJunctionTunnel) ? 0 : 3;
        }

        public void Read(ref BinaryReader reader, int subtype, PSDLFile parent)
        {
            ushort junctionShorts = 0;
            if (subtype == 0)
            {
                IsJunctionTunnel = true;
                junctionShorts = reader.ReadUInt16();
            }

            Flags = (TunnelFlags) reader.ReadUInt16();
            Unknown1 = reader.ReadByte();
            Height1 = reader.ReadByte();
            Unknown2 = reader.ReadByte();
            Height2 = reader.ReadByte();

            if (IsJunctionTunnel)
            {
                JunctionCeilingBits = reader.ReadUInt16();

                var ba = new BitArray(reader.ReadBytes((junctionShorts - 4) * 2));
                for (var i = 0; i < ba.Count; i++)
                {
                    JunctionWalls.Add(ba.Get(i));
                }
            }

        }

        public void Save(ref BinaryWriter writer, PSDLFile parent)
        {
            if (IsJunctionTunnel)
            {
                writer.Write((ushort)((JunctionWalls.Count >> 4) + 4));
            }

            writer.Write((ushort)Flags);
            writer.Write(Unknown1);
            writer.Write(Height1);
            writer.Write(Unknown2);
            writer.Write(Height2);

            if (IsJunctionTunnel)
            {
                writer.Write(JunctionCeilingBits);

                //hacky bit copying :(
                var ba = new BitArray(JunctionWalls.ToArray());
                var ret = new byte[(ba.Length - 1) / 8 + 1];
                ba.CopyTo(ret, 0);
                writer.Write(ret);

                //need padding?
                if (ret.Length % 2 > 0)
                {
                    writer.Write((byte)0);
                }
            }
        }

        //Constructors
        public TunnelElement(string leftWallTexture, string rightWallTexture, string ceilingTexture, string outerRightTexture, string outerLeftTexture, string undersideTexture, TunnelFlags flags, bool isJunction = false, params bool[] visibleWalls)
        {
            Textures = new string[] { leftWallTexture, rightWallTexture, ceilingTexture, outerRightTexture, outerLeftTexture, undersideTexture };
            IsJunctionTunnel = isJunction;
            Flags = flags;

            if (isJunction)
                JunctionWalls.AddRange(visibleWalls);
        }

        public TunnelElement()
        {
            //But nobody came
        }

    }
}

