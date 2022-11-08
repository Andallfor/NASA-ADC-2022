using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.IO;
using System;

public class serverConnectionPipe {
    

    public Task<globalMeshData> requestLunarTerrainPipe(int layer, Vector2Int fileCoord, Vector3Int range, bool flush) {
        string request = $"layer={layer}|area={fileCoord.x}_{fileCoord.y}|points={range.x}_{range.y}_{range.z}|flush={Convert.ToInt32(flush)}";

        return Task.Run<globalMeshData>(async () => {
            using (var server = new NamedPipeServerStream("one"))
            {
                server.WaitForConnection();

                using (var stream = new MemoryStream())
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(request);
                    server.Write(stream.ToArray(), 0, stream.ToArray().Length);

                    //byte[] bufferLength = new byte[64];
                    //await server.ReadAsync(bufferLength, 0, bufferLength.Length);
                    //string confirmation = System.Text.Encoding.ASCII.GetString(bufferLength);
                    //Debug.Log(confirmation);
                }

                server.Disconnect();
            }

            return new globalMeshData();
        });
    }
}
