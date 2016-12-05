using System;
using System.Collections.Generic;
using System.IO;

using System.Text;


namespace PSDL.Elements
{
    //RoadTriangleFan is just a copy of TriangleFan with a different Type :/
    public class CulledTriangleFanElement : TriangleFanElement
    {
        public override int GetElementType()
        {
            return 5;
        }

        //Constructors
        public CulledTriangleFanElement(string texture, Vertex[] vertices)
        {
            Textures = new string[] { texture };
            this.Vertices.AddRange(vertices);
        }

        public CulledTriangleFanElement()
        {
            //But nobody came
        }
    }
}
