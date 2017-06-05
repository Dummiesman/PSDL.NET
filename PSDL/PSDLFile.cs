using PSDL.Elements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;


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
        private bool _elementMapBuilt;
        private Dictionary<int, Type> _elementMap;

        public Vertex DimensionsMin    { get; set; }
        public Vertex DimensionsMax    { get; set; }
        public Vertex DimensionsCenter { get; set; }
        public float DimensionsRadius  { get; set; }

        //API for elements to use (maybe there's a better way?)
        public int GetTextureIndex(string tex)
        {
            return _textures.IndexOf(tex);
        }

        public string GetTextureFromCache(int id)
        {
            return _textures[id];
        }

        private void RebuildElementMap()
        {
            _elementMapBuilt = true;
            _elementMap = new Dictionary<int, Type>();

            foreach (var mytype in Assembly.GetExecutingAssembly().GetTypes().Where(mytype => mytype.GetInterfaces().Contains(typeof(IPSDLElement))))
            {
                var element = (IPSDLElement)Activator.CreateInstance(mytype);
                _elementMap[element.GetElementType()] = element.GetType();
            }
        }

        private void Load()
        {
            if (!_elementMapBuilt)
                RebuildElementMap();

            //clear these, we'll be overwriting the data
            Vertices.Clear();
            Rooms.Clear();
            Floats.Clear();
            _textures.Clear();

            using (var r = new BinaryReader(File.OpenRead(_filePath)))
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
                        int type = att >> 3 & 15;
                        int subtype = att & 7;

                        if (type != 10)
                        {
                            IPSDLElement loader = (IPSDLElement)Activator.CreateInstance(_elementMap[type]);

                            //some Elements require a texture offset
                            var textureOffset = 0;
                            var loaderType = (ElementType)loader.GetElementType();
                            switch (loaderType)
                            {
                                case ElementType.Crosswalk:
                                    textureOffset = 2;
                                    break;
                                case ElementType.SidewalkStrip:
                                    textureOffset = 1;
                                    break;
                            }

                            //read data, setup textures
                            loader.Read(r, subtype, this);

                            var requiredTextureCount = loader.GetRequiredTextureCount();
                            if (requiredTextureCount > 0)
                            {
                                if (currentTextureId != 65535)
                                {

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

                            //finally, add to room
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
                    Rooms.Add(new Room(roomElements, perimPoints));

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
                DimensionsMin = new Vertex(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                DimensionsMax = new Vertex(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                DimensionsCenter = new Vertex(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                DimensionsRadius = r.ReadSingle();

                //read prop paths         
                var numAiRoads = r.ReadUInt32();
                for (var i = 0; i < numAiRoads; i++)
                {
                    //lots of unknown stuff
                    var flags1 = r.ReadUInt16();
                    var flags2  = r.ReadUInt16();
                    var u3 = r.ReadByte();
                    var u4 = r.ReadByte();

                    //? some kind of distances
                    var floatData1 = new float[u3];
                    var floatData2 = new float[u4];
                    for (var j = 0; j < floatData1.Length; j++)
                    {
                        floatData1[j] = r.ReadSingle();
                    }
                    for (var j = 0; j < floatData2.Length; j++)
                    {
                        floatData2[j] = r.ReadSingle();
                    }


                    var flags3 = r.ReadUInt16();

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

                    AIRoads.Add(new AIRoad(flags1, flags2, floatData1, floatData2, flags3, roadRooms));
                }

                var unconsumedBytes = r.BaseStream.Length - r.BaseStream.Position;
                if (unconsumedBytes > 0)
                    throw new Exception("Reader didn't consume all bytes. Something happened!!");
            }
        }

        //useful saving functions
        private static string TextureHashString(string[] textures)
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


        //cache functions
        private void RefreshVertexCache()
        {
            Vertices.Clear();
            var hashVertexList = new HashSet<Vertex>();

            foreach (var rm in Rooms)
            {
                hashVertexList.UnionWith(rm.GatherVertices());
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
                    //STEP 1 : CACHE DVIIDED ROAD DIVIDERS
                    if (el is Elements.DividedRoadElement)
                    {
                        var dr = (DividedRoadElement)el;
                        var hash = TextureHashString(dr.DividerTextures);

                        //weird invisble Textures
                        if (hash == null)
                            continue;

                        if (!textureHashDictionary.ContainsKey(hash))
                        {
                            textureHashDictionary[hash] = _textures.Count;
                            _textures.AddRange(dr.DividerTextures);
                        }
                        dr.DividerTexture = (byte)(textureHashDictionary[hash] + 1);
                    }

                    //STEP 2 : CACHE ROADS AND TUNNELS, FUCKING PROPULATOR
                    if (el is DividedRoadElement || el is RoadElement || el is TunnelElement || el is WalkwayElement)
                    {
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
                hashFloatList.UnionWith(rm.GatherFloats());
            }

            Floats.AddRange(hashFloatList);
            hashFloatList.Clear();
        }

        public void RecalculateBounds()
        {
            var average = new Vertex(0, 0, 0);
            foreach (var vtx in Vertices)
            {
                DimensionsMin.x = Math.Min(vtx.x, DimensionsMin.x);
                DimensionsMin.y = Math.Min(vtx.y, DimensionsMin.y);
                DimensionsMin.z = Math.Min(vtx.z, DimensionsMin.z);
                DimensionsMax.x = Math.Max(vtx.x, DimensionsMax.x);
                DimensionsMax.y = Math.Max(vtx.y, DimensionsMax.y);
                DimensionsMax.z = Math.Max(vtx.z, DimensionsMax.z);
                average += vtx;
            }

            average /= Vertices.Count;
            DimensionsCenter = average;

            float radius = DimensionsMax.x - DimensionsMin.x;
            if (DimensionsMax.y - DimensionsMin.y > radius)
            {
                radius = DimensionsMax.y - DimensionsMin.y;
            }
            else
            {
                radius = DimensionsMax.z - DimensionsMin.z;
            }
            DimensionsRadius = radius/2;
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
            using (var w = new BinaryWriter(File.OpenWrite(saveAsFile)))
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

                        //need a new texture statement?
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

                        el.Save(w, this);    
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
                w.Write(DimensionsMin.x);
                w.Write(DimensionsMin.y);
                w.Write(DimensionsMin.z);

                w.Write(DimensionsMax.x);
                w.Write(DimensionsMax.y);
                w.Write(DimensionsMax.z);

                w.Write(DimensionsCenter.x);
                w.Write(DimensionsCenter.y);
                w.Write(DimensionsCenter.z);

                w.Write(DimensionsRadius);

                //verify ai roads
                var verifiedAiRoads = new List<AIRoad>();
                foreach (var aiRoad in AIRoads)
                {
                    var verified = aiRoad.StripInvalidRooms();
                    if (verified.Rooms.Count > 0)
                        verifiedAiRoads.Add(verified);
                }

                w.Write((uint) verifiedAiRoads.Count);
                for(var i=0; i < verifiedAiRoads.Count; i++)
                {
                    var road = verifiedAiRoads[i];
                    w.Write(road.Unknown1);
                    w.Write(road.Unknown2);
                    w.Write((byte)road.Unknown3.Count);
                    w.Write((byte)road.Unknown4.Count);
                    foreach (var f in road.Unknown3)
                    {
                        w.Write(f);
                    }
                    foreach (var f in road.Unknown4)
                    {
                        w.Write(f);
                    }
                    w.Write(road.Unknown5);

                    //CALCULATE crossroads, this code is crap, and only test code
                    //but left here since the AiRoads crash the game anyways
                    //still trying to figure out why :(
                    Room startRoom = road.Rooms[0];
                    Room endRoom = road.Rooms[road.Rooms.Count - 1];
                    var startRoad = startRoom.FindElementOfType<RoadElement>();
                    var endRoad = endRoom.FindElementOfType<RoadElement>();

                    if (startRoad == null)
                    {
                        startRoad = startRoom.FindElementOfType<DividedRoadElement>();
                        if (startRoad == null)
                        {
                            startRoad = startRoom.FindElementOfType<WalkwayElement>();
                            var re = (WalkwayElement) startRoad;
                            w.Write((ushort)Vertices.IndexOf(re.Vertices[0]));
                            w.Write((ushort)Vertices.IndexOf(re.Vertices[0]));
                            w.Write((ushort)Vertices.IndexOf(re.Vertices[1]));
                            w.Write((ushort)Vertices.IndexOf(re.Vertices[1]));
                        }
                        else
                        {
                            var re = (DividedRoadElement) startRoad;
                            w.Write((ushort) Vertices.IndexOf(re.Vertices[0]));
                            w.Write((ushort) Vertices.IndexOf(re.Vertices[1]));
                            w.Write((ushort) Vertices.IndexOf(re.Vertices[4]));
                            w.Write((ushort) Vertices.IndexOf(re.Vertices[5]));
                        }
                    }
                    else
                    {
                        var re = (RoadElement) startRoad;
                        w.Write((ushort)Vertices.IndexOf(re.Vertices[0]));
                        w.Write((ushort)Vertices.IndexOf(re.Vertices[1]));
                        w.Write((ushort)Vertices.IndexOf(re.Vertices[2]));
                        w.Write((ushort)Vertices.IndexOf(re.Vertices[3]));
                    }

                    if (endRoad == null)
                    {
                        endRoad = endRoom.FindElementOfType<DividedRoadElement>();
                        if (endRoad == null)
                        {
                            endRoad = endRoom.FindElementOfType<WalkwayElement>();
                            var re = (WalkwayElement) endRoad;
                            var vertCount = re.Vertices.Count;
                            w.Write((ushort)Vertices.IndexOf(re.Vertices[vertCount - 2]));
                            w.Write((ushort)Vertices.IndexOf(re.Vertices[vertCount - 2]));
                            w.Write((ushort)Vertices.IndexOf(re.Vertices[vertCount - 1]));
                            w.Write((ushort)Vertices.IndexOf(re.Vertices[vertCount - 1]));
                        }
                        else
                        {
                            var re = (DividedRoadElement) endRoad;
                            var vertCount = re.Vertices.Count;
                            w.Write((ushort) Vertices.IndexOf(re.Vertices[vertCount - 6]));
                            w.Write((ushort) Vertices.IndexOf(re.Vertices[vertCount - 5]));
                            w.Write((ushort) Vertices.IndexOf(re.Vertices[vertCount - 2]));
                            w.Write((ushort) Vertices.IndexOf(re.Vertices[vertCount - 1]));
                        }
                    }
                    else
                    {
                        var re = (RoadElement)endRoad;
                        var vertCount = re.Vertices.Count;
                        w.Write((ushort)Vertices.IndexOf(re.Vertices[vertCount - 4]));
                        w.Write((ushort)Vertices.IndexOf(re.Vertices[vertCount - 3]));
                        w.Write((ushort)Vertices.IndexOf(re.Vertices[vertCount - 2]));
                        w.Write((ushort)Vertices.IndexOf(re.Vertices[vertCount - 1]));

                    }
                    
                
                    w.Write((byte)road.Rooms.Count);
                    foreach (var room in road.Rooms)
                    {
                        w.Write((short)(Rooms.IndexOf(room) + 1));
                    }
                }
            }
        }

        //Constructors
        public PSDLFile()
        {
            _textures = new List<string>();
            Floats = new List<float>();
            Vertices = new List<Vertex>();
            Rooms = new List<Room>();
            AIRoads = new List<AIRoad>();
        }

        public PSDLFile(string psdlPath) : this()
        {
            _filePath = psdlPath;
            Load();
        }
    }
}
