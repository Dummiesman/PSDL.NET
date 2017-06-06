using System;
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
    }

    public class SDLElementBase
    {
        public string[] Textures { get; set; }
    }

    public interface IGeometricSDLElement
    {
        Vertex[] GetVertices();
    }
}
