using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using NumSharp;
using System.Linq;
using System.IO.Pipes;
using System.Threading;

public class serverConnection {
    private string server;
    private int port;
    private int stride = 4;
    public serverConnection(string server, int port) {
        this.server = server;
        this.port = port;
    }

    /// <summary> Example: layer=10|area=15_30|points=50_75_4 </summary>
    public Task<globalMeshData> requestLunarTerrainSocket(int layer, Vector2Int fileCoord, Vector3Int range, bool flush) {
        string request = $"layer={layer}|area={fileCoord.x}_{fileCoord.y}|points={range.x}_{range.y}_{range.z}|flush={Convert.ToInt32(flush)}";

        return Task.Run<globalMeshData>(async () => {
            try {
                using (TcpClient client = new TcpClient()) {
                    bool validConnection = client.ConnectAsync(Dns.GetHostEntry(server).AddressList[1], port).Wait(10_000);
                    if (!validConnection) throw new TimeoutException();
                    NetworkStream stream = client.GetStream();

                    // send request
                    byte[] r = Encoding.ASCII.GetBytes(request);
                    await stream.WriteAsync(r, 0, r.Length);

                    // wait for reply, should be the length of the incoming data
                    byte[] bufferLength = new byte[64];
                    await stream.ReadAsync(bufferLength, 0, bufferLength.Length);
                    string confirmation = System.Text.Encoding.ASCII.GetString(bufferLength);

                    string[] lengths = confirmation.Split('|');
                    int dataLength = Convert.ToInt32(lengths[0]);
                    int sizeX = Convert.ToInt32(lengths[1]);
                    int sizeY = Convert.ToInt32(lengths[2]);

                    // send back the length to confirm
                    await stream.WriteAsync(BitConverter.GetBytes(dataLength), 0, 4);

                    // receive data
                    int[] heights = new int[sizeX * sizeY];
                    using (MemoryStream ms = new MemoryStream()) {                  
                        int count = 0;
                        do {
                            byte[] buf = new byte[1024];
                            count = await stream.ReadAsync(buf, 0, 1024);
                            ms.Write(buf, 0, count);
                        } while(stream.CanRead && count > 0);

                        Buffer.BlockCopy(ms.ToArray(), 0, heights, 0, sizeX * sizeY * stride);
                    }

                    stream.Close();

                    globalMeshData data = new globalMeshData() {
                        heights = heights,
                        size = new Vector2Int(sizeX, sizeY)};

                    return data;
                }
            } catch (Exception e) {
                Debug.Log(e);
                return new globalMeshData();
            }
        });
    }
}

public struct globalMeshData {
    public int[] heights;
    public Vector2Int size;
}
