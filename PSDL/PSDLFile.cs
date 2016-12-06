using PSDL.Elements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;


namespace PSDL
{
    public class PSDLFile
    {
        private List<string> _textures;
        public List<float> Floats;
        public List<Vertex> Vertices;
        public List<Room> Rooms;
        public List<AIRoad> AIRoads;

        private string _filePath;
        private bool _elementMapBuilt = false;
        private Dictionary<int, Type> _elementMap;

        public Vertex dimensionsMin;
        public Vertex dimensionsMax;
        public Vertex dimensionsCenter;
        public float dimensionsRadius;

        public string GetTextureFromCache(int id)
        {
            return _textures[id];
        }

        public void RebuildElementMap()
        {
            _elementMapBuilt = true;
            _elementMap = new Dictionary<int, Type>();

            foreach (var mytype in Assembly.GetExecutingAssembly().GetTypes().Where(mytype => mytype.GetInterfaces().Contains(typeof(IPSDLElement))))
            {
                var element = (IPSDLElement)Activator.CreateInstance(mytype);
                _elementMap[element.GetElementType()] = element.GetType();
            }
        }

        public void Load()
        {
            if (!_elementMapBuilt)
                RebuildElementMap();

            //clear these, we'll be overwriting the data
            Vertices.Clear();
            Rooms.Clear();
            Floats.Clear();
            _textures.Clear();

            var r = new BinaryReader(File.OpenRead(_filePath));
            using (r)
            {
                //verify our magic is PSD0
                var magic = r.ReadUInt32();
                if (magic != 809784144)
                {
                    throw new Exception("Incorrect magic, this may not be a PSDL file");
                }

                var targetSize = r.ReadUInt32();
                if (targetSize != 2)
                {
                    throw new Exception("Incorrect target size. Expected 2, got " + targetSize.ToString() + ".");
                }

                //read Vertices
                var numVertices = r.ReadUInt32();
                for (var i = 0; i < numVertices; i++)
                {
                    Vertices.Add(new Vertex(r.ReadSingle(), r.ReadSingle(), r.ReadSingle()));
                }

                //read Floats
                var  numFloats = r.ReadUInt32();
                for (var i = 0; i < numFloats; i++)
                {
                    Floats.Add(r.ReadSingle());
                }

                //read Textures
                var numTextures = r.ReadUInt32() - 1;
                for (var i = 0; i < numTextures; i++)
                {
                    var textureLen = r.ReadByte();
                    if (textureLen > 0)
                    {
                        _textures.Add(new string(r.ReadChars(textureLen - 1)));
                        r.BaseStream.Seek(1, SeekOrigin.Current);
                    }
                    else
                    {
                        _textures.Add("");
                    }
                }

                //read rooms
                var numRooms = r.ReadUInt32() - 1;
                var somethingWeird = r.ReadUInt32();
                uint currentTextureId = 0;

                var perimeterLinks = new Dictionary<PerimeterPoint, ushort>();

                for (var i = 0; i < numRooms; i++)
                {
                    var perimPoints = new List<PerimeterPoint>();
                    var roomElements = new List<IPSDLElement>();

                    var numPerimeterPoints = r.ReadUInt32();
                    var numAttributes = r.ReadUInt32();

                    //Perimeter points
                    for (var j = 0; j < numPerimeterPoints; j++)
                    {
                        var perimVertIndex = r.ReadUInt16();
                        var perimBlockIndex = r.ReadUInt16();

                        perimPoints.Add(new PerimeterPoint(Vertices[perimVertIndex], null));

                        perimeterLinks[perimPoints[perimPoints.Count - 1]] = perimBlockIndex;
                    }

                    var attributeStartAddress = r.BaseStream.Position;
                    //attributes
                    while (r.BaseStream.Position < (attributeStartAddress + (numAttributes * 2)))
                    {
                        //extract attribute Type and subtype
                        var att = r.ReadUInt16();
                        var type = att >> 3 & 15;
                        var subtype = att & 7;

                        if (type != 10)
                        {
                            IPSDLElement loader = (IPSDLElement)Activator.CreateInstance(_elementMap[type]);

                            //some Elements require a texture offset
                            var textureOffset = 0;
                            if (loader is CrosswalkElement)
                            {
                                textureOffset = 2;
                            }
                            else if (loader is SidewalkStripElement)
                            {
                                textureOffset = 1;
                            }

                            //set Textures
                            var requiredTextureCount = loader.GetRequiredTextureCount();

                            if (requiredTextureCount > 0)
                            {
                                if (currentTextureId != 65535)
                                {
                                    //non null texture
                                    loader.Textures = _textures.GetRange((int)currentTextureId + textureOffset, requiredTextureCount).ToArray();
                                }
                                else
                                {
                                    //null texture
                                    loader.Textures = new string[requiredTextureCount];
                                    for (var j = 0; j < requiredTextureCount; j++)
                                    {
                                        loader.Textures[j] = null;
                                    }
                                }
                            }


                            //set data
                            //Console.WriteLine(loader.GetType().Name);
                            loader.Read(ref r, subtype, this);

                            //add to room
                            roomElements.Add(loader);
                        }
                        else
                        {
                            //new texture
                            var textureId = (ushort)(r.ReadUInt16() + (256 * subtype) - 1);
                            currentTextureId = textureId;
                        }
                    }

                    //finally, store this room
                    Rooms.Add(new Room(roomElements.ToArray(), perimPoints.ToArray()));

                }


                //set Perimeter links
                for (var i = 0; i < Rooms.Count; i++)
                {
                    var room = Rooms[i];
                    foreach (var pp in room.Perimeter)
                    {
                        var ppIndex = perimeterLinks[pp];
                        Room connection = null;

                        if (ppIndex > 0)
                        {
                            connection = Rooms[ppIndex - 1];
                        }

                        pp.ConnectedRoom = connection;
                    }
                }

                //skip padding? byte, read Flags
                r.BaseStream.Seek(1, SeekOrigin.Current);
                for (var i = 0; i < numRooms; i++)
                {
                    Rooms[i].Flags = (RoomFlags)r.ReadByte();
                }

                //skip padding? byte, read proprule index
                r.BaseStream.Seek(1, SeekOrigin.Current);
                for (var i = 0; i < numRooms; i++)
                {
                    Rooms[i].PropRule = r.ReadByte();
                }

                //dimensions
                dimensionsMin = new Vertex(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                dimensionsMax = new Vertex(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                dimensionsCenter = new Vertex(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                dimensionsRadius = r.ReadSingle();

                //read prop paths         
                var numAiRoads = r.ReadUInt32();
                for (var i = 0; i < numAiRoads; i++)
                {
                    //lots of unknown stuff
                    var u1 = r.ReadUInt16();
                    var u2 = r.ReadUInt16();
                    var u3 = r.ReadByte();
                    var u4 = r.ReadByte();

                    //? some kind of distances
                    var u5 = new float[u3 + u4];
                    for (var j = 0; j < u5.Length; j++)
                    {
                        u5[j] = r.ReadSingle();
                    }


                    var u6 = r.ReadUInt16();

                    //read road start Vertices
                    var scr = new Vertex[4];
                    for (var j = 0; j < 4; j++)
                    {
                        scr[j] = Vertices[r.ReadUInt16()];
                    }


                    //read road end Vertices
                    var ecr = new Vertex[4];
                    for (var j = 0; j < 4; j++)
                    {
                        ecr[j] = Vertices[r.ReadUInt16()];
                    }


                    var numRoomsInRoad = r.ReadByte();
                    var roadRooms = new Room[numRoomsInRoad];
                    for (var j = 0; j < numRoomsInRoad; j++)
                    {
                        //for some strange reason these are signed
                        var roomNumber = Math.Abs(r.ReadInt16());
                        roadRooms[j] = Rooms[roomNumber - 1];
                    }

                    AIRoads.Add(new AIRoad(u1, u2, u3, u4, u5, u6, scr, ecr, roadRooms));
                }

                var unconsumedBytes = r.BaseStream.Length - r.BaseStream.Position;
                if (unconsumedBytes > 0)
                    throw new Exception("Reader didn't consume all bytes. Something happened!!");
            }
        }

        //useful saving functions
        private bool ElementUsesNullTexture(IPSDLElement element)
        {
            return (element.Textures[0] == null || element.Textures == null);
        }

        private string TextureHashString(string[] textures)
        {
            if (textures == null || textures[0] == null)
                return null;

            var hash = "";
            foreach(var texture in textures)
            {
                hash += texture + "|";
            }
            return hash;
        }


        private void BuildVertexCache(IEnumerable<Vertex> vertices, ref HashSet<Vertex> hashList)
        {
            foreach (var v in vertices)
            {
                hashList.Add(v);
            }
        }

        private ushort BuildAttributeHeader(bool last, int type, int subtype)
        {
            if (subtype > Constants.MaxSubtype)
                throw new Exception($"Tried to create subtype > MAX_SUBTYPE for Type {type}, got subtype {subtype}.");

            if (type > Constants.MaxType)
                throw new Exception($"Tried to create attribute header with Type > MAX_TYPE, got Type {type}");

            var iLast = (last) ?  1 : 0;
            ushort ret = (byte)(iLast << 7 | type << 3 | subtype);
            return ret;
        }

        private void RefreshVertexCache()
        {
            Vertices.Clear();
            var hashVertexList = new HashSet<Vertex>();

            foreach (var rm in Rooms)
            {
                foreach (var pp in rm.Perimeter)
                {
                    hashVertexList.Add(pp.Vertex);
                }

                foreach (var el in rm.Elements)
                {
                    var type = (ElementType)el.GetElementType();
                    switch (type)
                    {
                        case ElementType.Road:
                        {
                            var cacheElement = (RoadElement) el;
                            BuildVertexCache(cacheElement.Vertices, ref hashVertexList);
                            break;
                        }
                        case ElementType.TriangleFan:
                        case ElementType.CulledTriangleFan:
                        {
                            var cacheElement = (TriangleFanElement)el;
                            BuildVertexCache(cacheElement.Vertices, ref hashVertexList);
                            break;
                        }
                        case ElementType.Crosswalk:
                        {
                            var cacheElement = (CrosswalkElement)el;
                            BuildVertexCache(cacheElement.Vertices, ref hashVertexList);
                            break;
                        }
                        case ElementType.DividedRoad:                            
                        {
                            var cacheElement = (DividedRoadElement)el;
                            BuildVertexCache(cacheElement.Vertices, ref hashVertexList);
                            break;
                        }
                        case ElementType.FacadeBound:
                        {
                            var cacheElement = (FacadeBoundElement)el;
                            BuildVertexCache(cacheElement.Vertices, ref hashVertexList);
                            break;
                        }
                        case ElementType.Facade:
                        {
                            var cacheElement = (FacadeElement)el;
                            BuildVertexCache(cacheElement.Vertices, ref hashVertexList);
                            break;
                        }
                        case ElementType.RoofTriangleFan:
                        {
                            var cacheElement = (RoofTriangleFanElement)el;
                            BuildVertexCache(cacheElement.Vertices, ref hashVertexList);
                            break;
                        }
                        case ElementType.Walkway:
                        {
                            var cacheElement = (WalkwayElement)el;
                            BuildVertexCache(cacheElement.Vertices, ref hashVertexList);
                            break;
                        }
                        case ElementType.Sliver:
                        {
                            var cacheElement = (SliverElement)el;
                            BuildVertexCache(cacheElement.Vertices, ref hashVertexList);
                            break;
                        }
                        case ElementType.SidewalkStrip:
                        {
                            var cacheElement = (SidewalkStripElement)el;
                            BuildVertexCache(cacheElement.Vertices, ref hashVertexList);
                            break;
                        }
                    }
                }
            }

            Vertices.AddRange(hashVertexList);
            hashVertexList.Clear();
        }

        private Dictionary<string, int> GenerateTextureHashDictionary()
        {
            //build Textures cache. start with divided roads
            //since they have a special index for Textures
            //which is only a wee little byte
            _textures.Clear();

            var textureHashDictionary = new Dictionary<string, int>();

            foreach (var rm in Rooms)
            {
                foreach (var el in rm.Elements)
                {
                    if (el is Elements.DividedRoadElement)
                    {
                        var dr = (DividedRoadElement)el;
                        if (!_textures.Contains(dr.DividerTexture))
                        {
                            textureHashDictionary[dr.DividerTexture + "|"] = _textures.Count;
                            _textures.Add(dr.DividerTexture);
                        }
                    }
                }
            }

            //next build everything else
            foreach (var rm in Rooms)
            {
                foreach (var el in rm.Elements)
                {
                    //no Textures, no hash
                    if (el.GetRequiredTextureCount() == 0)
                        continue;

                    var hash = TextureHashString(el.Textures);

                    //weird invisble Textures
                    if (hash == null)
                        continue;

                    if (!textureHashDictionary.ContainsKey(hash))
                    {
                        textureHashDictionary[hash] = _textures.Count;
                        _textures.AddRange(el.Textures);
                    }
                }
            }

            return textureHashDictionary;
        }

        private void RefreshFloatCache()
        {
            //build floats cache
            Floats.Clear();
            var hashFloatList = new HashSet<float>();

            foreach (var rm in Rooms)
            {
                foreach (var el in rm.Elements)
                {
                    var type = (ElementType) el.GetElementType();
                    switch (type)
                    {
                        case ElementType.FacadeBound:
                        {
                            var cacheElement = (FacadeBoundElement) el;
                            hashFloatList.Add(cacheElement.Height);
                            break;
                        }
                        case ElementType.RoofTriangleFan:
                        {
                            var cacheElement = (RoofTriangleFanElement)el;
                            hashFloatList.Add(cacheElement.Height);
                            break;
                        }
                        case ElementType.Facade:
                        {
                            var cacheElement = (FacadeElement)el;
                            hashFloatList.Add(cacheElement.BottomHeight);
                            hashFloatList.Add(cacheElement.TopHeight);
                            break;
                        }
                        case ElementType.Sliver:
                        {
                            var cacheElement = (SliverElement)el;
                            hashFloatList.Add(cacheElement.TextureScale);
                            hashFloatList.Add(cacheElement.Height);
                            break;
                        }
                    }
                }
            }

            Floats.AddRange(hashFloatList);
            hashFloatList.Clear();
        }

        public void RecalculateBounds()
        {
            var average = new Vertex(0, 0, 0);
            foreach (var vtx in Vertices)
            {
                dimensionsMin.x = Math.Min(vtx.x, dimensionsMin.x);
                dimensionsMin.y = Math.Min(vtx.y, dimensionsMin.y);
                dimensionsMin.z = Math.Min(vtx.z, dimensionsMin.z);
                dimensionsMax.x = Math.Max(vtx.x, dimensionsMax.x);
                dimensionsMax.y = Math.Max(vtx.y, dimensionsMax.y);
                dimensionsMax.z = Math.Max(vtx.z, dimensionsMax.z);
                average += vtx;
            }

            average /= Vertices.Count;
            dimensionsCenter = average;

            float radius = 0;
            radius = dimensionsMax.x - dimensionsMin.x;
            if (dimensionsMax.y - dimensionsMin.y > radius)
            {
                radius = dimensionsMax.y - dimensionsMin.y;
            }
            else
            {
                radius = dimensionsMax.z - dimensionsMin.z;
            }
            dimensionsRadius = radius/2;
        }

        public void ReSave()
        {
            if (_filePath == null)
                throw new Exception("Can't save PSDL file without being constructed from a file path. Use SaveAs instead.");

            SaveAs(_filePath);
        }



        public void SaveAs(string saveAsFile)
        {
            if (!_elementMapBuilt)
                RebuildElementMap();
            
            //initialize caches and get hash dict
            RefreshVertexCache();
            RefreshFloatCache();
            RecalculateBounds();
            var textureHashDictionary = GenerateTextureHashDictionary();

            //write file
            var w = new BinaryWriter(File.OpenWrite(saveAsFile));
            using (w)
            {
                //PSD0, and targetSize
                w.Write((uint)809784144);
                w.Write((uint)2);

                //Vertices
                w.Write((uint)Vertices.Count);
                foreach (var vtx in Vertices) 
                {
                    w.Write(vtx.x);
                    w.Write(vtx.y);
                    w.Write(vtx.z);
                }

                //floats
                w.Write((uint)Floats.Count);
                foreach (var flt in Floats)
                {
                    w.Write(flt);
                }

                //Textures
                w.Write((uint)_textures.Count + 1);
                for (var i = 0; i < _textures.Count; i++)
                {
                
                    if (_textures[i].Length > 0)
                    {
                        w.Write((byte)(_textures[i].Length + 1));
                        for (var j = 0; j < _textures[i].Length; j++)
                        {
                            w.Write(_textures[i][j]);
                        }
                        w.Write((byte)0);
                    }
                    else
                    {
                        w.Write((byte)0);
                    }
                }

                //rooms
                w.Write((uint)Rooms.Count + 1);
                w.Write((uint)0); //unknown stuff :/

                for (var i = 0; i < Rooms.Count; i++)
                {
                    var rm = Rooms[i];

                    w.Write((uint)rm.Perimeter.Count);

                    var attributeLengthPtr = w.BaseStream.Position;
                    w.Write((uint)0);

                    //write Perimeter
                    for (var j = 0; j < rm.Perimeter.Count; j++)
                    {
                        var pp = rm.Perimeter[j];
                        w.Write((ushort)Vertices.IndexOf(pp.Vertex));

                        if (pp.ConnectedRoom == null)
                        {
                            w.Write((ushort)0);
                        }
                        else
                        {
                            w.Write((ushort)(Rooms.IndexOf(pp.ConnectedRoom) + 1));
                        }

                    }

                    //write elements
                    var lastTextureHash = "$$";
                    var attributeStartPtr = w.BaseStream.Position;

                    for (var j = 0; j < rm.Elements.Count; j++)
                    {
                        var el = rm.Elements[j];
                        var texHash = TextureHashString(el.Textures);

                        if (lastTextureHash != texHash && !(el is FacadeBoundElement))
                        {
                            if (texHash != null)
                            {
                                //generate a new texture
                                var textureIndex = textureHashDictionary[texHash];

                                //some Elements require a texture offset
                                var textureOffset = 0;
                                if (el is CrosswalkElement)
                                {
                                    textureOffset = -2;
                                }
                                else if (el is SidewalkStripElement)
                                {
                                    textureOffset = -1;
                                }
                                textureIndex += textureOffset;

                                textureIndex += 1;
                                int textureIndexByte = textureIndex % 256;
                                int textureIndexSubtype = (int)Math.Floor((float)textureIndex / 256);

                                var textureHeader = BuildAttributeHeader(false, 10, textureIndexSubtype);

                                w.Write(textureHeader);

                                w.Write((ushort)(textureIndexByte));
                            }
                            else
                            {
                                //null texture
                                w.Write(BuildAttributeHeader(false, 10, 0));
                                w.Write((ushort)(0));
                            }
                        }

                        //last element?
                        var lastElement = (j == rm.Elements.Count - 1);

                        //write the element
                        w.Write(BuildAttributeHeader(lastElement, el.GetElementType(), el.GetElementSubType()));

                        el.Save(ref w, this);    
                    }

                    //finally, write attribute length
                    var attributeTotalSize = (uint)((w.BaseStream.Position - attributeStartPtr) / 2);

                    w.BaseStream.Seek(attributeLengthPtr, SeekOrigin.Begin);
                    w.Write(attributeTotalSize);
                    w.BaseStream.Seek(0, SeekOrigin.End);
                }

                //write padding(?) byte, and room Flags
                w.Write((byte)0);
                for (var i = 0; i < Rooms.Count; i++)
                {
                    w.Write((byte)Rooms[i].Flags);
                }

                //write padding(?) byte, and room proprules
                w.Write((byte)0xcd);
                for (var i = 0; i < Rooms.Count; i++)
                {
                    w.Write(Rooms[i].PropRule);
                }

                //write dimensions
                w.Write(dimensionsMin.x);
                w.Write(dimensionsMin.y);
                w.Write(dimensionsMin.z);

                w.Write(dimensionsMax.x);
                w.Write(dimensionsMax.y);
                w.Write(dimensionsMax.z);

                w.Write(dimensionsCenter.x);
                w.Write(dimensionsCenter.y);
                w.Write(dimensionsCenter.z);

                w.Write(dimensionsRadius);

                w.Write((uint)AIRoads.Count);
                for(var i=0; i < AIRoads.Count; i++)
                {
                    var road = AIRoads[i];
                    w.Write(road.Unknown1);
                    w.Write(road.Unknown2);
                    w.Write(road.Unknown3);
                    w.Write(road.Unknown4);
                    foreach (var f in road.Unknown5)
                    {
                        w.Write(f);
                    }
                    w.Write(road.Unknown6);
                    foreach (var scr in road.StartCrossroads)
                    {
                        w.Write((ushort)Vertices.IndexOf(scr));
                    }
                    foreach (var ecr in road.EndCrossroads)
                    {
                        w.Write((ushort)Vertices.IndexOf(ecr));
                    }

                    w.Write((byte)road.Rooms.Count);
                    foreach (var room in road.Rooms)
                    {
                        w.Write((ushort)(Rooms.IndexOf(room) + 1));
                    }
                }
            }
        }

        //Constructors
        private void BasicCTOR()
        {
            _textures = new List<string>();
            Floats = new List<float>();
            Vertices = new List<Vertex>();
            Rooms = new List<Room>();
            AIRoads = new List<AIRoad>();
        }

        public PSDLFile()
        {
            _filePath = null;
            BasicCTOR();
        }

        public PSDLFile(string psdlPath)
        {
            _filePath = psdlPath;
            BasicCTOR();
            Load();
        }
    }
}
