using System;
using System.Collections.Generic;
using System.IO;

using System.Text;

namespace PSDL
{
    public interface IPSDLElement
    {
        void Save(ref BinaryWriter writer, PSDLFile parent);
        void Read(ref BinaryReader reader, int subtype, PSDLFile parent);
        string[] Textures { get; set; }
        int GetElementType();
        int GetElementSubType();
        int GetRequiredTextureCount();
    }
}
