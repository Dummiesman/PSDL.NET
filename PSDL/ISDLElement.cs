﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PSDL
{
    public interface ISDLElement
    {
        void Save(BinaryWriter writer, PSDLFile parent);
        void Read(BinaryReader reader, int subtype, PSDLFile parent);
        string[] Textures { get; set; }
        ElementType Type { get; }
        int Subtype { get; }
        int RequiredTextureCount { get; }
        int TextureIndexOffset { get; }
    }

    public class SDLElementBase
    {
        public string[] Textures { get; set; }
        public virtual int TextureIndexOffset => 0;
    }

    public interface IGeometricSDLElement
    {
        /// <summary>
        /// Returns vertices represented as an array. Not guaranteed to be direct access, may be a copy.
        /// </summary>
        /// <returns></returns>
        Vertex[] GetVertices();

        /// <summary>
        /// Grabs a direct reference to a vertex in the vertex array.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        Vertex GetVertex(int index);

        /// <summary>
        /// Sets a vertex directly in the array
        /// </summary>
        /// <param name="index"></param>
        /// <param name="vertex"></param>
        void SetVertex(int index, Vertex vertex);

        /// <summary>
        /// Gets the vertex count
        /// </summary>
        /// <returns></returns>
        int GetVertexCount();

        void RemoveVertexAt(int idx);
        void AddVertex();
        void InsertVertex(int idx, Vertex vtx);
        void InsertVertex(int idx);
    }
}
