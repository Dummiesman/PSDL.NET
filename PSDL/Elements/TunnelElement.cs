using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using System.Text;


namespace PSDL.Elements
{
    public class TunnelElement : IPSDLElement
    {
        public bool LeftSide;
        public bool RightSide;
        public bool IsWall;
        public bool FlatCeiling;
        public bool ClosedStartLeft;
        public bool ClosedEndLeft;
        public bool ClosedStartRight;
        public bool ClosedEndRight;

        public bool CurvedCeiling;
        public bool OffsetStartLeft;
        public bool OffsetEndLeft;
        public bool OffsetStartRight;
        public bool OffsetEndRight;
        public bool CurvedSides;
        public bool Culled;

        public bool IsJunctionTunnel;
        public List<bool> JunctionWalls = new List<bool>();
        public ushort JunctionCeilingBits;

        public byte Height1;
        public byte Height2;
        public byte Unknown1;
        public byte Unknown2;



        private string[] _textures;
        public string[] Textures
        {
            get
            {
                return _textures;
            }

            set
            {
                _textures = value;
            }
        }

        public int GetRequiredTextureCount()
        {
            return 6;
        }

        int IPSDLElement.GetElementType()
        {
            return 9;
        }

        public int GetElementSubType()
        {
            if (IsJunctionTunnel)
                return 0;
            return 3;
        }

        public void Read(ref BinaryReader reader, int subtype, PSDLFile parent)
        {
            ushort junctionShorts = 0;
            if (subtype == 0)
            {
                IsJunctionTunnel = true;
                junctionShorts = reader.ReadUInt16();
            }

            var flagsA = reader.ReadByte();
            LeftSide = (flagsA & 1) >= 1;
            RightSide = (flagsA & 2) >= 1;
            IsWall = (flagsA & 4) >= 1;
            FlatCeiling = (flagsA & 8) >= 1;
            ClosedStartLeft = (flagsA & 16) >= 1;
            ClosedEndLeft = (flagsA & 32) >= 1;
            ClosedStartRight = (flagsA & 64) >= 1;
            ClosedEndRight = (flagsA & 128) >= 1;

            var flagsB = reader.ReadByte();
            CurvedCeiling = (flagsB & 1) >= 1;
            OffsetStartLeft = (flagsB & 2) >= 1;
            OffsetEndLeft = (flagsB & 4) >= 1;
            OffsetStartRight = (flagsB & 8) >= 1;
            OffsetEndRight = (flagsB & 16) >= 1;
            CurvedSides = (flagsB & 32) >= 1;
            Culled = (flagsB & 64) >= 1;

            Unknown1 = reader.ReadByte();
            Height1 = reader.ReadByte();
            Unknown2 = reader.ReadByte();
            Height2 = reader.ReadByte();

            if (IsJunctionTunnel)
            {
                JunctionCeilingBits = reader.ReadUInt16();

                var ba = new BitArray(reader.ReadBytes((junctionShorts - 4) * 2));
                for (int i = 0; i < ba.Count; i++)
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

            byte flagsA = 0;
            byte flagsB = 0;

            if (LeftSide) { flagsA |= 1; }
            if (RightSide) { flagsA |= 2; }
            if (IsWall) { flagsA |= 4; }
            if (FlatCeiling) { flagsA |= 8; }
            if (ClosedStartLeft) { flagsA |= 16; }
            if (ClosedEndLeft) { flagsA |= 32; }
            if (ClosedStartRight) { flagsA |= 64; }
            if (ClosedEndRight) { flagsA |= 128; }

            if (CurvedCeiling) { flagsB |= 1; }
            if (OffsetStartLeft) { flagsB |= 2; }
            if (OffsetEndLeft) { flagsB |= 4; }
            if (OffsetStartRight) { flagsB |= 8; }
            if (OffsetEndRight) { flagsB |= 16; }
            if (CurvedSides) { flagsB |= 32; }
            if (Culled) { flagsB |= 64; }

            writer.Write(flagsA);
            writer.Write(flagsB);
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

        //Constructors (TODO : Make some sort of TunnelFlags structure cause this is just crap)
        public TunnelElement(string leftWallTexture, string rightWallTexture, string ceilingTexture, string outerRightTexture, string outerLeftTexture, string undersideTexture, bool isJunction = false, params bool[] visibleWalls)
        {
            Textures = new string[] { leftWallTexture, rightWallTexture, ceilingTexture, outerRightTexture, outerLeftTexture, undersideTexture };
            IsJunctionTunnel = isJunction;

            if (isJunction)
                JunctionWalls.AddRange(visibleWalls);

        }

        public TunnelElement()
        {
            //But nobody came
        }

    }
}

