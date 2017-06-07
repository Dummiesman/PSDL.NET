﻿using System;
using System.IO;

namespace PSDL.Elements
{
    public class FacadeBoundElement : SDLElementBase,  IGeometricSDLElement, ISDLElement
    {
        public Vertex[] Vertices = new Vertex[2];
        public Vertex[] GetVertices()
        {
            return Vertices;
        }

        public float LightAngle
        {
            get => (float) m_SunAngle * 5.625f;
            set => m_SunAngle = Math.Min((ushort)63, (ushort)((value % 360f) / 5.625));
        }
        private ushort m_SunAngle;
        public float Height;

        //interface
        public ElementType Type => ElementType.FacadeBound;
        public int Subtype => 4;
        public int RequiredTextureCount => 0;

        public void Read(BinaryReader reader, int subtype, PSDLFile parent)
        {
            m_SunAngle = reader.ReadUInt16();
            Height = parent.Floats[reader.ReadUInt16()];
            Vertices[0] = parent.Vertices[reader.ReadUInt16()];
            Vertices[1] = parent.Vertices[reader.ReadUInt16()];
        }

        public void Save(BinaryWriter writer, PSDLFile parent)
        {
            writer.Write(m_SunAngle);
            writer.Write((ushort)parent.Floats.IndexOf(Height));
            writer.Write((ushort)parent.Vertices.IndexOf(Vertices[0]));
            writer.Write((ushort)parent.Vertices.IndexOf(Vertices[1]));
        }

        //API
        public void RecalculateAngle()
        {
            float xDiff = Vertices[1].x - Vertices[0].x;
            float yDiff = Vertices[1].z - Vertices[0].z;
            double angle =  ((Math.Atan2(yDiff, xDiff) * (180 / Math.PI)) + 180);
            LightAngle = (float)angle;
        }

        //Constructors
        public FacadeBoundElement(float lightAngle, float height, Vertex leftVertex, Vertex rightVertex)
        {
            LightAngle = lightAngle;
            Height = height;
            Vertices[0] = leftVertex;
            Vertices[1] = rightVertex;
        }

        public FacadeBoundElement()
        {
            //But nobody came
        }
    }
}

