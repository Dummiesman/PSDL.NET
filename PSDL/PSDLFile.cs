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
        private List<string> m_Textures;
        public List<float> Floats;
        public List<Vertex> Vertices;
        public List<Room> Rooms;
        public List<AIRoad> AIRoads;

        private string m_FilePath;   
        private bool m_ElementMapBuilt;
        private Dictionary<int, Type> _elementMap;

        public Vertex DimensionsMin    { get; set; }
        public Vertex DimensionsMax    { get; set; }
        public Vertex DimensionsCenter { get; set; }
        public float DimensionsRadius  { get; set; }

        //API for elements to use (maybe there's a better way?)
        public int GetTextureIndex(string tex)
        {
            return m_Textures.IndexOf(tex);
        }

        public string GetTextureFromCache(int id)
        {
            return m_Textures[id];
        }

        private void RebuildElementMap()
        {
            m_ElementMapBuilt = true;
            _elementMap = new Dictionary<int, Type>();

            foreach (var mytype in Assembly.GetExecutingAssembly().GetTypes().Where(mytype => mytype.GetInterfaces().Contains(typeof(ISDLElement))))
            {
                var element = (ISDLElement)Activator.CreateInstance(mytype);
                _elementMap[(int)element.Type] = element.GetType();
            }
        }

        private void Load()
        {
            if (!m_ElementMapBuilt)
                RebuildElementMap();

            //clear these, we'll be overwriting the data
            Vertices.Clear();
            Rooms.Clear();
            Floats.Clear();
            m_Textures.Clear();

            using (var r = new BinaryReader(File.OpenRead(m_FilePath)))
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
                        m_Textures.Add(new string(r.ReadChars(textureLen - 1)));
                        r.BaseStream.Seek(1, SeekOrigin.Current);
                    }
                    else
                    {
                        m_Textures.Add(string.Empty);
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
                    var roomElements = new List<ISDLElement>();

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

                    //attributes
                    var attributeStartAddress = r.BaseStream.Position;
                    while (r.BaseStream.Position < (attributeStartAddress + (numAttributes * 2)))
                    {
                        //extract attribute Type and subtype
                        var att = r.ReadUInt16();
                        int type = att >> 3 & 15;
                        int subtype = att & 7;

                        //new texture
                        if (type == 10)
                        {
                            //new texture
                            var textureId = (ushort)(r.ReadUInt16() + (256 * subtype) - 1);
                            currentTextureId = textureId;
                            continue;
                        }

                        //get the loader
                        ISDLElement loader = (ISDLElement)Activator.CreateInstance(_elementMap[type]);

                        //some Elements require a texture offset
                        var textureOffset = loader.TextureIndexOffset;

                        //read data, setup textures
                        loader.Read(r, subtype, this);

                        var requiredTextureCount = loader.RequiredTextureCount;
                        if (requiredTextureCount > 0)
                        {
                            if (currentTextureId != 65535)
                            {
                                loader.Textures = m_Textures.GetRange((int)currentTextureId + textureOffset, requiredTextureCount).ToArray();
                            }
                            else
                            {
                                //null texture
                                loader.Textures = new string[requiredTextureCount];
                                for (var j = 0; j < requiredTextureCount; j++)
                                    loader.Textures[j] = null;
                            }
                        }

                        //finally, add to room
                        roomElements.Add(loader);

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
                        pp.ConnectedRoom = (ppIndex > 0) ? Rooms[ppIndex - 1] :null;
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

                //read ai roads        
                var numAiRoads = r.ReadUInt32();
                for (var i = 0; i < numAiRoads; i++)
                {
                    //lots of unknown stuff
                    var flags = r.ReadUInt32();
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

                    AIRoads.Add(new AIRoad((AIRoad.PropulationFlags)flags, floatData1, floatData2, flags3, roadRooms));
                }

                var unconsumedBytes = r.BaseStream.Length - r.BaseStream.Position;
                if (unconsumedBytes > 0)
                    throw new Exception("Reader didn't consume all bytes. Something happened!!");
            }
        }

        //useful saving functions
        private static string TextureHashString(string[] textures)
        {
            if (textures?[0] == null)
                return null;

            var hash = string.Empty;
            foreach(var texture in textures)
            {
                hash += $"{texture}|";
            }
            return hash;
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
            m_Textures.Clear();

            var textureHashDictionary = new Dictionary<string, int>();

            foreach (var rm in Rooms)
            {
                foreach (var el in rm.Elements)
                {
                    //STEP 1 : CACHE DVIIDED ROAD DIVIDERS (We only have 1 byte)
                    if (el is DividedRoadElement)
                    {
                        var dr = (DividedRoadElement)el;
                        var hash = TextureHashString(dr.DividerTextures);

                        //weird invisble Textures
                        if (hash == null)
                            continue;

                        if (!textureHashDictionary.ContainsKey(hash))
                        {
                            textureHashDictionary[hash] = m_Textures.Count;
                            m_Textures.AddRange(dr.DividerTextures);
                        }
                        dr.DividerTexture = (byte)(textureHashDictionary[hash] + 1);
                    }

                    //STEP 2 : CACHE ROADS AND TUNNELS, FUCKING PROPULATOR (Propulator throws a fit if we don't do this)
                    if (el is DividedRoadElement || el is RoadElement || el is TunnelElement || el is WalkwayElement)
                    {
                        var hash = TextureHashString(el.Textures);

                        //weird invisble Textures
                        if (hash == null)
                            continue;

                        if (!textureHashDictionary.ContainsKey(hash))
                        {
                            textureHashDictionary[hash] = m_Textures.Count;
                            m_Textures.AddRange(el.Textures);
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
                    if (el.RequiredTextureCount == 0)
                        continue;

                    var hash = TextureHashString(el.Textures);

                    //weird invisble Textures
                    if (hash == null)
                        continue;

                    if (!textureHashDictionary.ContainsKey(hash))
                    {
                        textureHashDictionary[hash] = m_Textures.Count;
                        m_Textures.AddRange(el.Textures);
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
            if (m_FilePath == null)  
                throw new Exception("Can't save PSDL file without being constructed from a file path. Use SaveAs instead.");

            SaveAs(m_FilePath);
        }



        public void SaveAs(string saveAsFile)
        {
            if (!m_ElementMapBuilt)
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
                w.Write((uint)m_Textures.Count + 1);
                foreach (var texture in m_Textures)
                {
                    if (texture.Length > 0)
                    {
                        w.Write((byte)(texture.Length + 1)); //length incl. null terminator
                        for (var j = 0; j < texture.Length; j++)
                        {
                            w.Write(texture[j]);
                        }
                    }
                    w.Write((byte)0); //null terminator
                }

                //rooms
                w.Write((uint)Rooms.Count + 1);
                w.Write((uint)0); //unknown stuff :/
                foreach (var room in Rooms)
                {
                    w.Write((uint)room.Perimeter.Count);

                    var attributeLengthPtr = w.BaseStream.Position;
                    w.Write((uint)0);

                    //write Perimeter
                    foreach (var pp in room.Perimeter)
                    {
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

                    for (var j = 0; j < room.Elements.Count; j++)
                    {
                        var el = room.Elements[j];
                        var texHash = TextureHashString(el.Textures);

                        //need a new texture statement?
                        if (lastTextureHash != texHash && !(el is FacadeBoundElement))
                        {
                            if (texHash != null)
                            {
                                //generate a new texture
                                var textureIndex = textureHashDictionary[texHash] + 1 + -el.TextureIndexOffset;

                                int textureIndexByte = textureIndex % 256;
                                int textureIndexSubtype = (int)Math.Floor((float)textureIndex / 256);

                                w.Write(BuildAttributeHeader(false, 10, textureIndexSubtype));
                                w.Write((ushort)(textureIndexByte));
                            }
                            else
                            {
                                //null texture
                                w.Write(BuildAttributeHeader(false, 10, 0));
                                w.Write((ushort)(0));
                            }
                        }

                        //write the element
                        w.Write(BuildAttributeHeader((j == room.Elements.Count - 1), (int)el.Type, el.Subtype));

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
                    w.Write((int)road.Flags);
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

                    //calculate crossroads
                    Room startRoom = road.Rooms[0];
                    Room endRoom = road.Rooms[road.Rooms.Count - 1];

                    var startCrossroads = startRoom.GetCrossroadVertices(CrossroadEnd.First);
                    var endCrossroads = endRoom.GetCrossroadVertices(CrossroadEnd.Last);
                    foreach(var vertex in startCrossroads)
                    {
                        w.Write((ushort)Vertices.IndexOf(vertex));
                    }
                    foreach (var vertex in endCrossroads)
                    {
                        w.Write((ushort)Vertices.IndexOf(vertex));
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
            m_Textures = new List<string>();
            Floats = new List<float>();
            Vertices = new List<Vertex>();
            Rooms = new List<Room>();
            AIRoads = new List<AIRoad>();
        }

        public PSDLFile(string psdlPath) : this()
        {
            m_FilePath = psdlPath;
            Load();
        }
    }
}
