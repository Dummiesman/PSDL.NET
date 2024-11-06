using System;
using System.Collections.Generic;


namespace PSDL.Elements
{
    //CulledTriangleFan is a copy of TriangleFan, but the game won't
    //render it if it's first vertex is above the camera
    public class CulledTriangleFanElement : TriangleFanElement, ICloneable
    {
        public override ElementType Type => ElementType.CulledTriangleFan;

        //Clone interface
        public new object Clone()
        {
            return new CulledTriangleFanElement((TriangleFanElement) base.Clone());
        }

        //Constructors
        public CulledTriangleFanElement(TriangleFanElement source) : this(source.Textures[0], source.Vertices)
        {
            
        }

        public CulledTriangleFanElement(string texture, IEnumerable<Vertex> vertices)
        {
            Textures = new [] { texture };
            Vertices.AddRange(vertices);
        }

        public CulledTriangleFanElement()
        {
            //But nobody came
            Textures = new string[] { null };
        }
    }
}
