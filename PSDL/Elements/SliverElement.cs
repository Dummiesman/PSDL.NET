﻿using System;
using System.Collections.Generic;
using System.IO;

using System.Text;


namespace PSDL.Elements
{
    public class SliverElement : IPSDLElement
    {
        public Vertex[] Vertices = new Vertex[2];
        public float Height;
        public float TextureScale;

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
            return 1;
        }

        int IPSDLElement.GetElementType()
        {
            return 3;
        }

        public int GetElementSubType()
        {
            return 4;
        }

        public void Read(ref BinaryReader reader, int subtype, PSDLFile parent)
        {
            Height = parent.Floats[reader.ReadUInt16()];
            TextureScale = parent.Floats[reader.ReadUInt16()];
            Vertices[0] = parent.Vertices[reader.ReadUInt16()];
            Vertices[1] = parent.Vertices[reader.ReadUInt16()];
        }

        public void Save(ref BinaryWriter writer, PSDLFile parent)
        {
            writer.Write((ushort)parent.Floats.IndexOf(Height));
            writer.Write((ushort)parent.Floats.IndexOf(TextureScale));
            writer.Write((ushort)parent.Vertices.IndexOf(Vertices[0]));
            writer.Write((ushort)parent.Vertices.IndexOf(Vertices[1]));
        }

        //Constructors
        public SliverElement(string texture, float height, float textureScale, Vertex leftVertex, Vertex rightVertex)
        {
            Textures = new string[] { texture };
            Height = height;
            TextureScale = textureScale;
            Vertices[0] = leftVertex;
            Vertices[1] = rightVertex;
        }

        public SliverElement()
        {
            //But nobody came
        }
    }
}

