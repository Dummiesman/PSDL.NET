using System;
using System.Collections.Generic;
using System.IO;

using System.Text;


namespace PSDL.Elements
{
    //CulledTriangleFan is a copy of TriangleFan, but the game won't
    //render it if it's first vertex is above the camera
    public class CulledTriangleFanElement : TriangleFanElement
    {
        public override int GetElementType()
        {
            return 5;
        }

        //Constructors
        public CulledTriangleFanElement(string texture, IEnumerable<Vertex> vertices)
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
