// See https://aka.ms/new-console-template for more information

using System.Numerics;
using System.Text;
using AAEmu.Commons.Network;

// demo game\worlds\main_world\paths\010_010
var originX = 10;
var OriginY = 10;

// sprintf(fileName, "%s/net%s.bai",szLevel,szMission);
// sprintf(fileNameVerts, "%s/verts%s.bai",szLevel,szMission);
// sprintf(fileNameHide, "%s/hide%s.bai",szLevel,szMission);
// sprintf(fileNameVolume, "%s/v3d%s.bai",szLevel,szMission);
// sprintf(fileNameFlight, "%s/fnav%s.bai",szLevel,szMission);
// sprintf(fileNameRoads, "%s/roadnav%s.bai",szLevel,szMission);
// sprintf(fileNameAreas, "%s/areas%s.bai",szLevel,szMission);
// sprintf(fileNameWaypoint3DSurface, "%s/waypt3Dsfc%s.bai",szLevel,szMission);
// sprintf(fileNameFree2D, "%s/free2d%s.bai",szLevel,szMission);

// net  nav graph
// verts  vertex list
// areas  area list
// hide unknown


// var a = AreasMission.ReadFromFile("areasmission0.bai");
var result = NetMission.ReadFromFile("netmission0.bai");
// var result = HideMission.ReadFromFile("hidemission0.bai");
// var result2 = VertsMission.ReadFromFile("vertsmission0.bai");

// Console.WriteLine(result);
// var hideMission = HideMission.ReadFromFile("hidemission0.bai");
// var vertsMission = VertsMission.ReadFromFile("vertsmission0.bai");


return;

public class IAISystem
{
    enum ENavigationType
    {
        NAV_UNSET = 1 << 0,
        NAV_TRIANGULAR = 1 << 1,
        NAV_WAYPOINT_HUMAN = 1 << 2,
        NAV_WAYPOINT_3DSURFACE = 1 << 3,
        NAV_FLIGHT = 1 << 4,
        NAV_VOLUME = 1 << 5,
        NAV_ROAD = 1 << 6,
        NAV_SMARTOBJECT = 1 << 7,
        NAV_FREE_2D = 1 << 8,
        NAV_CUSTOM_NAVIGATION = 1 << 9,
        NAV_MAX_VALUE = NAV_CUSTOM_NAVIGATION
    }
}

public class NetMission
{
    public Vector3 Position { get; set; }

    public Dictionary<uint, ( NetMission, Vector3)> Neighbors { get; set; } = new();

    public static NetMission ReadFromFile(string filePath)
    {
        using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            byte[] buffer = new byte[fileStream.Length];

            _ = fileStream.Read(buffer, 0, buffer.Length);

            var ps = new PacketStream(buffer);

            Console.WriteLine("Verifying BAI file version");
            var version = ps.ReadUInt32();

            if (version != 54)
            {
                Console.Write(
                    "CGraph::ReadNodes Wrong triangulation BAI file version - found {0} expected {1}: Regenerate triangulation in the editor [Design bug]",
                    version, 54);
                // return null;
            }

            // Reading BBOX
            var min = ps.ReadVector3Single();
            var max = ps.ReadVector3Single();

            var l1 = ps.ReadUInt32();

            var l1List = new Dictionary<uint, NetMission>();
            if (l1 > 0)
            {
                for (uint i = 0; i < l1; i++)
                {
                    var net = new NetMission();
                    // the node ID used during quick load/save. Guaranteed to be unique (by resetting just before saving)
                    // and ID 1 is special = it signifies m_pSafeFirst
                    var id = ps.ReadUInt32();
                    var dir = ps.ReadVector3Single();
                    var up = ps.ReadVector3Single();
                    net.Position = ps.ReadVector3Single();
                    // building, span or volume
                    var index = ps.ReadInt32();
                    // int bostancle[3]
                    var obstacle1 = ps.ReadInt32();
                    var obstacle2 = ps.ReadInt32();
                    var obstacle3 = ps.ReadInt32();
                    //uint16 navType;	// cast from IAISystem::ENavigationType
                    var navType = ps.ReadUInt16();

                    //uint8 type:4; // 0-4 from EWaypointNodeType
                    //uint8 bForbidden:1; // bool - just for triangular and forbidden areas
                    //uint8 bForbiddenDesigner:1; // bool - just for triangular and designer forbidden
                    //uint8 bRemovable:1; // bool
                    var ctx = ps.ReadByte();
                    // var type = ps.ReadByte();
                    // var bForbiddenDesigner = ps.ReadByte();
                    // var bForbidden = ps.ReadByte();
                    // var bRemovable = ps.ReadByte();

                    // I Don't unknown remain 1 byte and there is data
                    var ctx2 = ps.ReadByte();


                    l1List.Add(id, net);

                    // Console.WriteLine(net);
                }
            }

            // Reading links
            var l2 = ps.ReadUInt32();
            var linkDescBuffer = new List<LinkDescriptor>();
            for (int i = 0; i < l2; i++)
            {
                var linkDescriptor = new LinkDescriptor();
                linkDescriptor.nSourceNode = ps.ReadUInt32();
                linkDescriptor.nTargetNode = ps.ReadUInt32();
                //
                linkDescriptor.vEdgeCenter = ps.ReadVector3Single();
                linkDescriptor.fMaxPassRadius = ps.ReadSingle();
                linkDescriptor.fExposure = ps.ReadSingle();
                linkDescriptor.fLength = ps.ReadSingle();
                linkDescriptor.fMaxWaterDepth = ps.ReadSingle();
                linkDescriptor.fMinWaterDepth = ps.ReadSingle();
                linkDescriptor.nStartIndex = ps.ReadByte();
                linkDescriptor.nEndIndex = ps.ReadByte();

                var ctx = ps.ReadByte(); // bIsPureTriangularLink & nEndIndex

                linkDescriptor.bIsPureTriangularLink = (byte)(ctx & 1);
                linkDescriptor.bSimplePassabilityCheck1 = (byte)(ctx & 2);

                linkDescBuffer.Add(linkDescriptor);
            }

            // Console.WriteLine($"{l1List}");
        }

        return new NetMission();
    }
}

public class NodeDescriptor
{
    public uint ID;
    public Vector3 dir;
    public Vector3 up;
    public Vector3 pos;
    public int index;
    public int[] obstacle = new int[3];
    public ushort navType;

    private byte flags;

    public byte Type
    {
        get => (byte)(flags & 0b00001111); // 获取低4位
        set => flags = (byte)((flags & 0b11110000) | (value & 0b00001111)); // 设置低4位
    }

    public bool BForbidden
    {
        get => (flags & 0b00010000) != 0; // 获取第5位
        set
        {
            if (value)
                flags |= 0b00010000; // 设置第5位为1
            else
                flags &= 0b11101111; // 设置第5位为0
        }
    }

    public bool BForbiddenDesigner
    {
        get => (flags & 0b00100000) != 0; // 获取第6位
        set
        {
            if (value)
                flags |= 0b00100000; // 设置第6位为1
            else
                flags &= 0b11011111; // 设置第6位为0
        }
    }

    public bool BRemovable
    {
        get => (flags & 0b01000000) != 0; // 获取第7位
        set
        {
            if (value)
                flags |= 0b01000000; // 设置第7位为1
            else
                flags &= 0b10111111; // 设置第7位为0
        }
    }
}

public struct LinkDescriptor
{
    public uint nSourceNode;
    public uint nTargetNode;
    public Vector3 vEdgeCenter;
    public float fMaxPassRadius;
    public float fExposure;
    public float fLength;
    public float fMaxWaterDepth;
    public float fMinWaterDepth;

    public byte nStartIndex, nEndIndex;

    // byte :1,:1
    public byte bIsPureTriangularLink;
    public byte bSimplePassabilityCheck1;
};

public struct SpecialArea
{
    public enum EType
    {
        TYPE_WAYPOINT_HUMAN,
        TYPE_VOLUME,
        TYPE_FLIGHT,
        TYPE_WATER,
        TYPE_WAYPOINT_3DSURFACE,
        TYPE_FREE_2D,
        TYPE_TRIANGULATION,
        TYPE_LAYERED_NAV_MESH,
        TYPE_FLIGHT2,
    };

    // public bool bAltered = true;

    // void SetPolygon(const ListPositions& polygon)
    // {
    //     lstPolygon = polygon;
    //     CalcAABB();
    // }
    // const ListPositions& GetPolygon() const
    // {
    //     return lstPolygon;
    // }
    //
    // const AABB& GetAABB() const
    // {
    //     return aabb;
    // }

    // EType   type                : 6;
    // EWaypointConnections waypointConnections : 6;
    // EAILightLevel        lightLevel          : 4;
    // int16                nBuildingID;
    //
    // float                fMinZ;
    // float                fMaxZ;
    // float                fHeight;
    // float                fNodeAutoConnectDistance;
    //
    // bool                 bAltered     : 1;
    // bool                 bCritterOnly : 1;
    //
}

enum EAILightLevel
{
    AILL_NONE, //!< No effect.
    AILL_LIGHT, //!< Light.
    AILL_MEDIUM, //!< Medium.
    AILL_DARK, //!< Dark.
    AILL_SUPERDARK, //!< Super dark.
    AILL_LAST, //!< This always has to be the last one.
};

public class AreasMission
{
    //public bool ReadFromFile(string fileName, bool bAfterExporting)
    public static bool ReadFromFile(string path)
    {
        var fileLoaded = false;

        using (var file = new BinaryReader(File.Open(path, FileMode.Open)))
        {
            var nConfigurationVersion = file.ReadUInt32();

            // Read areas
            var numAreas = file.ReadUInt32();

            for (int i = 0; i < numAreas; i++)
            {
                var nameSize = file.ReadUInt32();
                var areaName = Encoding.UTF8.GetString(file.ReadBytes((int)nameSize));
                var type = (SpecialArea.EType)file.ReadUInt64(); // 0 SpecialArea::EType
                var waypointConnections = file.ReadUInt64(); // 0 EWaypointConnections
                var altered = file.ReadByte() != 0; // 2 altered
                var fHeight = file.ReadSingle(); // 0 fHeight
                if (nConfigurationVersion <= 16)
                {
                    var junk = file.ReadSingle();
                }

                var fNodeAutoConnectDistance = file.ReadSingle(); // 0 fNodeAutoConnectDistance
                var fMaxZ = file.ReadSingle(); // 0 fMaxZ
                var fMinZ = file.ReadSingle(); // 0 fMinZ
                var nBuildingID = file.ReadInt32(); // 0 nBuildingID
                if (nConfigurationVersion >= 18)
                {
                    var unk9 = (EAILightLevel)file.ReadByte(); // only nConfigurationVersion >= 18 read, lightLevel
                }

                if (nConfigurationVersion >= 23)
                {
                    var critterOnly = file.ReadByte() != 0;
                }

                // now the area itself
                var ptsSize = file.ReadUInt32();
                var polygon = new List<Vector3>();
                for (var vtxIdx = 0; vtxIdx < ptsSize; ++vtxIdx)
                {
                    polygon.Add(new Vector3(file.ReadSingle(), file.ReadSingle(), file.ReadSingle()));
                }
            }

            // Read forbidden areas
            // var numAreas = file.ReadUInt32();
            // for (var idx = 0; idx < numAreas; ++idx)
            // {
            //     ReadForbiddenArea(file);
            // }

            // Read navigation modifiers (special areas)
            // numAreas = file.ReadUInt32();
            // for (int i = 0; i < numAreas; i++)
            // {
            //     var nameSize = file.ReadUInt32();
            //     var areaName = Encoding.UTF8.GetString(file.ReadBytes((int)nameSize));
            //     var type = (SpecialArea.EType)file.ReadUInt64(); // 0 SpecialArea::EType
            //     var waypointConnections = file.ReadUInt64(); // 0 EWaypointConnections
            //     var altered = file.ReadByte() != 0; // 2 altered
            //     var fHeight = file.ReadSingle(); // 0 fHeight
            //     if (nConfigurationVersion <= 16)
            //     {
            //         var junk = file.ReadSingle();
            //     }
            //
            //     var fNodeAutoConnectDistance = file.ReadSingle(); // 0 fNodeAutoConnectDistance
            //     var fMaxZ = file.ReadSingle(); // 0 fMaxZ
            //     var fMinZ = file.ReadSingle(); // 0 fMinZ
            //     var nBuildingID = file.ReadInt32(); // 0 nBuildingID
            //     if (nConfigurationVersion >= 18)
            //     {
            //         var unk9 = (EAILightLevel)file.ReadByte(); // only nConfigurationVersion >= 18 read, lightLevel
            //     }
            //
            //     if (nConfigurationVersion >= 23)
            //     {
            //         var critterOnly = file.ReadByte() != 0;
            //     }
            //
            //     // now the area itself
            //     var ptsSize = file.ReadUInt32();
            //     var polygon = new List<Vector3>();
            //     for (var vtxIdx = 0; vtxIdx < ptsSize; ++vtxIdx)
            //     {
            //         polygon.Add(new Vector3(file.ReadSingle(), file.ReadSingle(), file.ReadSingle()));
            //     }
            // }


            // Read designer forbidden areas
            // numAreas = file.ReadUInt32();
            // for (int i = 0; i < numAreas; i++)
            // {
            //     ReadForbiddenArea(file);
            // }

            // Read forbidden boundaries
            // numAreas = file.ReadUInt32();
            // for (int i = 0; i < numAreas; i++)
            // {
            //     ReadForbiddenArea(file);
            // }

            // Read extra link costs
            // numAreas = file.ReadUInt32();

            // Read designer paths
            // [Language] C++
            // {
            //     file.ReadType(&numAreas);
            //     // vague sanity check
            //     AIAssert(numAreas < 1000000);
            //     for (unsigned iArea = 0 ; iArea < numAreas ; ++iArea)
            //     {
            //         ListPositions lp;
            //         string name;
            //         ReadPolygonArea(file, fileVersion, name, lp);
            //
            //         int navType(0), type(0);
            //         bool closed(false);
            //         file.ReadType(&navType);
            //         file.ReadType(&type);
            //
            //         if (fileVersion >= 22)
            //         {
            //             file.ReadType(&closed);
            //         }
            //
            //         if (m_mapDesignerPaths.find(name) != m_mapDesignerPaths.end())
            //             AIError("CAISystem::ReadAreasFromFile: Designer path '%s' already exists, please rename the path and reexport.", name.c_str());
            //         else
            //             m_mapDesignerPaths.insert(ShapeMap::iterator::value_type(name, SShape(lp, false, (IAISystem::ENavigationType)navType, type, closed)));
            //     }
            // }
        }

        return fileLoaded;
    }

    public static void ReadForbiddenArea(BinaryReader file)
    {
        var volumeAreaNameSize = file.ReadUInt32();
        var areaName = Encoding.UTF8.GetString(file.ReadBytes((int)volumeAreaNameSize));

        var ptsSize = file.ReadUInt32();
        var pts = new List<Vector3>();
        for (int i = 0; i < ptsSize; i++)
        {
            var pt = new Vector3(file.ReadSingle(), file.ReadSingle(), file.ReadSingle());

            pts.Add(pt);
        }

        return;
    }

    public static void ReadExtraLinkCostArea(BinaryReader file)
    {
        var areaNameSize = file.ReadUInt32();
        var areaName = Encoding.UTF8.GetString(file.ReadBytes((int)areaNameSize));


        var origCostFactor = file.ReadSingle();
        // file.ReadType(&shape.aabb.min);
        var aabbMin = new Vector3(file.ReadSingle(), file.ReadSingle(), file.ReadSingle());
        // file.ReadType(&shape.aabb.max);

        // unsigned ptsSize;
        // file.ReadType(&ptsSize);

        var ptsSize = file.ReadUInt32();
        for (int i = 0; i < ptsSize; i++)
        {
            var pt = new Vector3(file.ReadSingle(), file.ReadSingle(), file.ReadSingle());
        }
    }
}

public class HideMission : NetMission
{
    // not found source code
}

public class ObstacleDataDesc
{
    public Vector3 vPos;
    public Vector3 vDir;

    /// this radius is approximate - it is estimated during link generation. if -ve it means
    /// that it shouldn't be used (i.e. object is significantly non-circular)
    public float fApproxRadius;

    public byte flags;
    public byte approxHeight;
};

public class VertsMission
{
    public static object ReadFromFile(string filePath)
    {
        using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            byte[] buffer = new byte[fileStream.Length];
            _ = fileStream.Read(buffer, 0, buffer.Length);
            var ps = new PacketStream(buffer);

            var version = ps.ReadUInt32();

            if (version != 1)
            {
                Console.WriteLine(
                    "CVertexList::ReadFromFile Wrong vertex list BAI file version - found {0} expected {1}: Regenerate triangulation in the editor [Design bug]",
                    version, 1);
            }

            // Read number of descriptors.
            var count = ps.ReadUInt32();
            Console.WriteLine($"Count {count}");
            for (int i = 0; i < count; i++)
            {
                var obdd = new ObstacleDataDesc();
                obdd.vPos = ps.ReadVector3Single();
                obdd.vDir = ps.ReadVector3Single();
                obdd.fApproxRadius = ps.ReadSingle();
                obdd.flags = ps.ReadByte();
                obdd.approxHeight = ps.ReadByte();

                // unk, 32bit Padding ?
                ps.Pos += 2;
            }

            Console.WriteLine($"Data Len {ps.Count} Pos {ps.Pos} Down {ps.Count - ps.Pos}");
        }

        return new VertsMission();
    }
}
