using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;


namespace PSDL.Elements
{
    public class TunnelElement : SDLElementBase, ISDLElement, ICloneable
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

        /// <summary>
        /// Height of the tunnel. Maximum 255 meters.
        /// </summary>
        public float Height;
        public ushort Unknown;

        //properties
        /// <summary>
        /// Wall width is equivalent of Height * (1 / 3). It's automatically determined, and cannot be set.
        /// </summary>
        public float WallWidth => Height * (1f / 3f);

        /// <summary>
        /// Wall underside depth is equivalent of Height * (1 / 4). It's automatically determined
        /// </summary>
        public float WallUndersideDepth => Height * (1f / 4f);

        //interface
        public ElementType Type => ElementType.Tunnel;
        public int Subtype => (IsJunctionTunnel) ? 0 : 3;
        public int RequiredTextureCount => 6;

        public void Read(BinaryReader reader, int subtype, PSDLFile parent)
        {
            ushort junctionShorts = 0;
            if (subtype == 0)
            {
                IsJunctionTunnel = true;
                junctionShorts = reader.ReadUInt16();
            }

            Flags = (TunnelFlags) reader.ReadUInt16();
            Height = reader.ReadUInt16() / 256f;

            //case for really old (beta1 and below) psdls
            //which don't have the other 2 bytes
            if (subtype != 2)
            {
                Unknown = reader.ReadUInt16();
            }

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

        public void Save(BinaryWriter writer, PSDLFile parent)
        {
            if (IsJunctionTunnel)
            {
                writer.Write((ushort)((JunctionWalls.Count >> 4) + 4));
            }

            writer.Write((ushort)Flags);
            writer.Write((ushort)(Height * 256f));
            writer.Write(Unknown);


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

        //Clone interface
        public object Clone()
        {
            var cloneTunnel = new TunnelElement {Textures = new string[Textures.Length]};

            //clone texture array
            cloneTunnel.Textures = new string[this.Textures.Length];
            this.Textures.CopyTo(cloneTunnel.Textures, 0);
            
            //clone properties
            cloneTunnel.Flags = this.Flags;
            cloneTunnel.Height = this.Height;
            cloneTunnel.IsJunctionTunnel = this.IsJunctionTunnel;
            cloneTunnel.JunctionCeilingBits = this.JunctionCeilingBits;
            cloneTunnel.JunctionWalls.AddRange(this.JunctionWalls);
            cloneTunnel.Unknown = this.Unknown;

            return cloneTunnel;
        }

        //Constructors TODO: Create static constructors such as CreateWall CreateTunnel CreateJunctionWall CreateJunctionTunnel, because this is crap
        public TunnelElement(string leftWallTexture, string rightWallTexture, string ceilingTexture, string outerRightTexture, string outerLeftTexture, string undersideTexture, TunnelFlags flags, bool isJunction = false, params bool[] visibleWalls)
        {
            Textures = new []{ leftWallTexture, rightWallTexture, ceilingTexture, outerRightTexture, outerLeftTexture, undersideTexture };
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

