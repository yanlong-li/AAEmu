// See https://aka.ms/new-console-template for more information

using System.Numerics;
using System.Text;
using AAEmu.Commons.Network;

var originX = 10;
var OriginY = 10;

// var a = AreasMission.ReadFromFile("areasmission0.bai");
var netMission = NetMission.ReadFromFile("netmission0.bai");


Console.WriteLine(netMission);
// var hideMission = HideMission.ReadFromFile("hidemission0.bai");
// var vertsMission = VertsMission.ReadFromFile("vertsmission0.bai");

return;


public class NetMission
{
    public static NetMission ReadFromFile(string filePath)
    {
        // 使用 FileStream 读取文件
        using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            // 创建一个字节数组来存储读取的内容
            byte[] buffer = new byte[fileStream.Length];

            // // 读取文件内容到字节数组
            _ = fileStream.Read(buffer, 0, buffer.Length);
            //
            // // 将字节数组转换为字符串
            // string fileContent = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            //
            // // 输出文件内容
            // Console.WriteLine(fileContent);

            var ps = new PacketStream(buffer);

            ps.Pos += 28;
            var l1 = ps.ReadUInt32();

            Console.Write($"图层数量 {l1}");

            var l1List = new Dictionary<uint, Vector3>();
            if (l1 > 0)
            {
                for (uint i = 0; i < l1; i++)
                {
                    var id = ps.ReadUInt32();
                    ps.Pos += 24;
                    var vec = ps.ReadVector3Single();
                    l1List.Add(id, vec);
                    ps.Pos += 20;
                }
            }

            // 区域连接点
            var l2 = ps.ReadUInt32();
            Console.WriteLine($"共计数 {l2} 个连接点位");

            for (int i = 0; i < l2; i++)
            {
                var localLayer = ps.ReadUInt32();
                var targetLayer = ps.ReadUInt32();
                //
                var vec1 = ps.ReadVector3Single();
                var vec2 = ps.ReadVector3Single();
                // unknown
                var vec3 = ps.ReadVector3Single();

                Console.WriteLine($"{vec1} {vec2} {vec3}");
            }

            Console.WriteLine($"{l1List}");
        }

        return new NetMission();
    }
}

public class AreasMission
{
    //public bool ReadFromFile(string fileName, bool bAfterExporting)
    public static bool ReadFromFile(string path)
    {
        var fileLoaded = false;
        //                                        volumeId,       idx,            vtxIdx
        var volumeVerticesBuffer = new Dictionary<int, Dictionary<int, Dictionary<int, Vector3>>>();
        var volumeAreaNameBuffer = new Dictionary<int, List<string>>();

        using (var file = new BinaryReader(File.Open(path, FileMode.Open)))
        {
            string AreaName;
            var volumeId = 0;
            var usedVolumesCount = 0u;
            var volumeAreaNameSize = 0u;
            var verticesCount = 0u;
            var nConfigurationVersion = file.ReadUInt32();
            //volumeHeight = file.ReadUInt32();
            do
            {
                //                                  idx
                var volumeVertices = new Dictionary<int, Dictionary<int, Vector3>>();
                var volumeAreaName = new List<string>();
                usedVolumesCount = file.ReadUInt32();
                for (var idx = 0; idx < usedVolumesCount; ++idx)
                {
                    // Loading boundary volumes, their ID's and names
                    volumeAreaNameSize = file.ReadUInt32();
                    AreaName = Encoding.UTF8.GetString(file.ReadBytes((int)volumeAreaNameSize));
                    volumeAreaName.Add(AreaName);

                    //if (gap)
                    if (volumeAreaNameSize == 0x13)
                    {
                        var unk1 = file.ReadUInt32(); // 0
                        var unk2 = file.ReadUInt32(); // 0
                        var unk3 = file.ReadByte(); // 2
                        var unk4 = file.ReadUInt32(); // 0
                        var unk5 = file.ReadUInt32(); // 0
                        var unk6 = file.ReadSingle(); // 0
                        var unk7 = file.ReadSingle(); // 0
                        var unk8 = file.ReadSingle(); // 0
                        var unk9 = file.ReadSingle(); // 0
                        var unk10 = file.ReadUInt32(); // 0
                        var unk11 = file.ReadByte(); // 0
                    }

                    verticesCount = file.ReadUInt32();
                    var vtx = new Dictionary<int, Vector3>();
                    for (var vtxIdx = 0; vtxIdx < verticesCount; ++vtxIdx)
                    {
                        vtx.Add(vtxIdx, new Vector3(file.ReadSingle(), file.ReadSingle(), file.ReadSingle()));
                    }

                    volumeVertices.TryAdd(idx, vtx);
                }

                volumeVerticesBuffer.TryAdd(volumeId, volumeVertices);
                volumeAreaNameBuffer.TryAdd(volumeId, volumeAreaName);
                volumeId++;
                // CreateVolume(volumeVerticesBuffer[0], verticesCount, volumeHeight, volumeId);
                // m_volumesManager.RegisterAreaFromLoadedData(volumeAreaName, volumeId);

                fileLoaded = true;
                nConfigurationVersion--;
            } while (nConfigurationVersion > 0);
        }

        return fileLoaded;
    }
}

public class HideMission
{
    public static object ReadFromFile(string hidemission0Bai)
    {
        throw new NotImplementedException();
    }
}

public class VertsMission
{
    public static object ReadFromFile(string vertsmission0Bai)
    {
        throw new NotImplementedException();
    }
}
