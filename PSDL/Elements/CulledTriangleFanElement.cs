using System.Collections.Generic;


namespace PSDL.Elements
{
    //CulledTriangleFan is a copy of TriangleFan, but the game won't
    //render it if it's first vertex is above the camera
    public class CulledTriangleFanElement : TriangleFanElement
    {
        public override int GetElementType()
        {
            return (int)ElementType.CulledTriangleFan;
        }

        //Constructors
        public CulledTriangleFanElement(string texture, IEnumerable<Vertex> vertices)
        {
            Textures = new [] { texture };
            Vertices.AddRange(vertices);
        }

        public CulledTriangleFanElement()
        {
            //But nobody came
        }
    }
}
