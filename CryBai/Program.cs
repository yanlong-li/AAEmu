// See https://aka.ms/new-console-template for more information

using System.Numerics;
using System.Text;
using AAEmu.Commons.Network;
// demo game\worlds\main_world\paths\010_010
var originX = 10;
var OriginY = 10;

// var a = AreasMission.ReadFromFile("areasmission0.bai");
// var result = NetMission.ReadFromFile("netmission0.bai");
var result = HideMission.ReadFromFile("hidemission0.bai");
// var result2 = VertsMission.ReadFromFile("vertsmission0.bai");

// Console.WriteLine(result);
// var hideMission = HideMission.ReadFromFile("hidemission0.bai");
// var vertsMission = VertsMission.ReadFromFile("vertsmission0.bai");

return;


public class NetMission
{
    public Vector3 Position { get; set; }

    public Dictionary<uint, ( NetMission, Vector3)> Neighbors { get; set; } = new();

    public static NetMission ReadFromFile(string filePath)
    {
        // 使用 FileStream 读取文件
        using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            // 创建一个字节数组来存储读取的内容
            byte[] buffer = new byte[fileStream.Length];

            // // 读取文件内容到字节数组
            _ = fileStream.Read(buffer, 0, buffer.Length);

            var ps = new PacketStream(buffer);

            var version = ps.ReadUInt32();
            ps.Pos += 24;
            var l1 = ps.ReadUInt32();

            Console.Write($"坐标点 {l1}");

            var l1List = new Dictionary<uint, NetMission>();
            if (l1 > 0)
            {
                for (uint i = 0; i < l1; i++)
                {
                    var net = new NetMission();
                    var id = ps.ReadUInt32();
                    ps.Pos += 24;
                    net.Position = ps.ReadVector3Single();
                    l1List.Add(id, net);
                    ps.Pos += 20;

                    // Console.WriteLine(net);
                }
            }

            // 区域连接点
            var l2 = ps.ReadUInt32();
            Console.WriteLine($"共计数 {l2} 个共同点位");

            for (int i = 0; i < l2; i++)
            {
                var localLayer = ps.ReadUInt32();
                var targetLayer = ps.ReadUInt32();
                //
                var vec1 = ps.ReadVector3Single();
                var vec2 = ps.ReadVector3Single();
                // unknown
                var vec3 = ps.ReadVector3Single();

                l1List.TryGetValue(localLayer, out var net);
                l1List.TryGetValue(targetLayer, out var net2);
                net?.Neighbors.Add(targetLayer, (net2, vec1)!);

                Console.WriteLine($"Local {localLayer}=>{net?.Position} Target {targetLayer}=>{net2?.Position} Position ? {vec1}");
            }

            // Console.WriteLine($"{l1List}");
        }

        return new NetMission();
    }
}

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
            
            var volumeVertices = new Dictionary<int, Dictionary<int, Vector3>>();
            var volumeAreaName = new List<string>();
            var usedVolumesCount = file.ReadUInt32();

            for (var idx = 0; idx < usedVolumesCount; ++idx)
            {
                // Loading boundary volumes, their ID's and names
                var volumeAreaNameSize = file.ReadUInt32();
                var areaName = Encoding.UTF8.GetString(file.ReadBytes((int)volumeAreaNameSize));
                volumeAreaName.Add(areaName);

                var unk1 = (SpecialArea.EType)file.ReadUInt64(); // 0 SpecialArea::EType
                var unk2 = file.ReadUInt64(); // 0 EWaypointConnections
                var unk3 = file.ReadByte(); // 2 altered
                var unk4 = file.ReadSingle(); // 0 fHeight
                var unk5 = file.ReadSingle(); // 0 fNodeAutoConnectDistance
                var unk6 = file.ReadSingle(); // 0 fMaxZ
                var unk7 = file.ReadSingle(); // 0 fMinZ
                var unk8 = file.ReadInt32(); // 0 nBuildingID
                var unk9 = (EAILightLevel)file.ReadByte(); // only nConfigurationVersion >= 18 read, lightLevel

                var verticesCount = file.ReadUInt32();
                var vtx = new Dictionary<int, Vector3>();
                for (var vtxIdx = 0; vtxIdx < verticesCount; ++vtxIdx)
                {
                    vtx.Add(vtxIdx, new Vector3(file.ReadSingle(), file.ReadSingle(), file.ReadSingle()));
                }

                volumeVertices.TryAdd(idx, vtx);
            }
        }

        return fileLoaded;
    }
}

public class HideMission: NetMission
{
    
}

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
            var count = ps.ReadUInt32();
            Console.WriteLine($"Count {count}");
            for (int i = 0; i < count; i++)
            {
                // var x1 = ps.ReadUInt32();
                // var x2 = ps.ReadUInt32();
                // var x3 = ps.ReadUInt32();
                var p1 = ps.ReadVector3Single();
                // var x4 = ps.ReadUInt32();
                // var x5 = ps.ReadUInt32();
                // var x6 = ps.ReadUInt32();
                var p2 = ps.ReadVector3Single();
                var x7 = ps.ReadUInt16();
                var x8 = ps.ReadUInt16();
                var x9 = ps.ReadUInt32();

                // Console.WriteLine(p1);
                Console.WriteLine(x7);
            }

            Console.WriteLine($"Data Len {ps.Count} Pos {ps.Pos} Down {ps.Count - ps.Pos}");
        }

        return new VertsMission();
    }
}
