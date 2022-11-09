using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;

/// <summary> Brute force generation of regional files. Used in pre-processing of terrain. </summary>
public class regionalMeshGenerator {
    #region VARIABLES
    private int multi, numSubMeshes, trueXSize, trueYSize;
    private double radius;
    private string name, basePath, pathToRegion;
    private List<double> lats, lons, heights, slopes;
    private double maxHeight = -1000, minHeight = 1000, maxSlope = -1000, minSlope = 1000, maxAzimuth = -1000, minAzimuth = 1000, maxEle = -1000, minEle = 1000;
    #endregion

    /// <summary> Generate a region's mesh. Used only in preprocessing, do not actually show to user. </summary>
    /// <param name="regionName">Name of the region. Must be all lowercase. </param>
    /// <param name="multi">Sample every xth point. </param>
    /// <param name="numSubMeshes">Generate numSubMeshes * numSubMeshes amount of meshes. </param>
    /// <param name="radius">Radius of the planet, in km. </param>
    public regionalMeshGenerator(string regionName, int multi, int numSubMeshes, double radius) {
        this.name = regionName;
        this.multi = multi;
        this.numSubMeshes = numSubMeshes;
        this.radius = radius;
        // Assumes windows
        // gets the user currently running the program (streamingAssetsPath is C:/Users/name/....)
        // we want 'name' as we use it as the identifier in regionalFileLocations.csv to store the path to the regional files

        string host = Application.streamingAssetsPath.Split('/')[2];
        string csvLine = general.regionalFileLocations.First(x => x.Contains(host)); // will throw error if 'name' does not exist
        basePath = csvLine.Split(',')[1].Trim();
        pathToRegion = Path.Combine(basePath, regionName);
        if (!Directory.Exists(pathToRegion)) throw new Exception($"Could not find region '{regionName}'. Please make sure it is in the correct format. See Resources/regionalFileLocationsInfo.txt for more info.");
    }

    /// <summary> Generate the region. </summary>
    /// <returns> A dictionary of the meshes. The key is (0, 0) to (numSubMeshes, numSubMeshes) and the mesh is the correlated mesh. </returns>
    public Dictionary<Vector2Int, Mesh> generate() {
        string[] files = Directory.GetFiles(pathToRegion);
        // good code trust
        lats = csvParse(files.First(x => x.ToLower().Contains("latitude")));
        lons = csvParse(files.First(x => x.ToLower().Contains("longitude")));
        heights = csvParse(files.First(x => x.ToLower().Contains("height")));
        slopes = csvParse(files.First(x => x.ToLower().Contains("slope")));
        /*
        
                     idealWidth                reminderWidth
            (i,j) (0,0)                      (2,0)
                    +===========+============+======|
        idealHeight |           |            |      |
                    |           |            |      |
                    |           |            |      |
                    +-----------+------------+------|
                    |           |            |      |
                    |           |            |      |
                    |           |            |      |
              (0,2) +-----------+------------+------|
     reminderHeight |           |            |      |
                    |           |            |      |
                    |===============================|
        
        + => start of a coordinate/box
        */

        double latAvg = lats.Average();
        double lonAvg = lons.Average();
        position offset = geographic.toCartesian(latAvg, lonAvg, radius + heights.Average() / 1000.0);
        Debug.Log($"{name}: {latAvg}, {lonAvg}");

        Dictionary<Vector2Int, Mesh> meshes = new Dictionary<Vector2Int, Mesh>();
        int idealWidth = (trueXSize / multi) / numSubMeshes;
        int idealHeight = (trueYSize / multi) / numSubMeshes;
        int reminderWidth = (trueXSize / multi) - (idealWidth * (numSubMeshes - 1));
        int reminderHeight = (trueYSize / multi) - (idealHeight * (numSubMeshes - 1));
        for (int i = 0; i < numSubMeshes; i++) {
            for (int j = 0; j < numSubMeshes; j++) {
                int width = (i == numSubMeshes - 1) ? reminderWidth : idealWidth + 1;
                int height = (i == numSubMeshes - 1) ? reminderHeight : idealHeight + 1;

                int xStart = i * idealWidth;
                int yStart = j * idealHeight;

                Vector3[] verts = new Vector3[width * height];
                Vector2[] uvs = new Vector2[width * height];
                
                int k = 0;
                for (int y = yStart; y < yStart + height; y++) {
                    for (int x = xStart; x < xStart + width; x++) {
                        int index = (y * multi) * trueXSize + (x * multi);
                        double lat = lats[index];
                        double lon = lons[index];
                        double h = heights[index];
                        position p = (geographic.toCartesian(lat, lon, h / 1000.0 + radius) - offset) / master.scale;
                        p.swapAxis(); // unity has y axis upwards, but calculations use z as up

                        verts[k] = (Vector3) p;
                        uvs[k] = new Vector2((float) x / width, (float) y / height);
                        k++;
                    }
                }

                meshes[new Vector2Int(i, j)] = generateMesh(verts, width, height);
            }
        }

        // retrieve min max data
        int c = lats.Count; // all lists have the same length
        for (int i = 0; i < c; i++) {
            double height = heights[i];
            double slope = slopes[i];
            double lat = lats[i];
            double lon = lons[i];

            // using geographic here to store values, not to actually use as a geographic coord (tech, in theory we use it as a coord)
            double elevation = elevationAngle(new geographic(lat * Mathf.Deg2Rad, lon * Mathf.Deg2Rad), height, 1737.4, new geographic(0, 0), 0, 6371);
            double azimuth = azimuthAngle(new geographic(lat * Mathf.Deg2Rad, lon * Mathf.Deg2Rad), new geographic(0, 0), new position(361000, 0, -42100));

            if (height > maxHeight) maxHeight = height;
            if (height < minHeight) minHeight = height;
            if (slope > maxSlope) maxSlope = slope;
            if (slope < minSlope) minSlope = slope;
            if (elevation > maxEle) maxEle = elevation;
            if (elevation < minEle) minEle = elevation;
            if (azimuth > maxAzimuth) maxAzimuth = azimuth;
            if (azimuth < minAzimuth) minAzimuth = azimuth;
        }

        Texture2D maps = new Texture2D(trueXSize, trueYSize);
        Color[] cs = new Color[trueXSize * trueYSize];

        // maybe combine with first loop? seems difficult to reconcile with submesh loop though
        for (int y = 0; y < trueYSize - 1; y++) {
            for (int x = 0; x < trueXSize - 1; x++) {
                // for now have gradients be relative (to the max value)
                int i = y * trueXSize + x;
                short height = percentToShort(heights[i], minHeight, maxHeight);
                short slope = percentToShort(slopes[i], minSlope, maxSlope);

                double lat = lats[i];
                double lon = lons[i];
                double azi = azimuthAngle(new geographic(lat * Mathf.Deg2Rad, lon * Mathf.Deg2Rad), new geographic(0, 0), new position(361000, 0, -42100));
                double ele = elevationAngle(new geographic(lat * Mathf.Deg2Rad, lon * Mathf.Deg2Rad), heights[i], 1737.4, new geographic(0, 0), 0, 6371);

                short azimuth = percentToShort(azi, minAzimuth, maxAzimuth);
                short elevation = percentToShort(ele, minEle, maxEle);

                //float r = combineShorts(height, slope);
                //float g = combineShorts(azimuth, elevation);
                //float b = combineShorts(0, 0); // TODO: add more maps!
                ////float a = combineShorts(0, 0);

                cs[i] = new Color(
                    percent(heights[i], minHeight, maxHeight),
                    percent(slopes[i], minSlope, maxSlope),
                    percent(ele, minEle, maxEle),
                    percent(azi, minAzimuth, maxAzimuth));
            }
        }

        maps.SetPixels(cs, 0);
        maps.Apply();

        File.WriteAllBytes("C:/Users/leozw/Desktop/ADC/out/" + name + ".png", maps.EncodeToPNG());

        return meshes;
    }

    private short percentToShort(double v, double min, double max) => (short) (((v - min) / (max - min)) * 65535 - 32767);
    private float percent(double v, double min, double max) => (float) ((v - min) / (max - min));
    private float combineShorts(short a, short b) => BitConverter.ToSingle(BitConverter.GetBytes(a).Concat(BitConverter.GetBytes(b)).ToArray(), 0);

    /// <summary>
    /// Generates Azimuth Angles
    /// </summary>
    /// <param name="moonDegrees">latitude and longitude of position on moon</param>
    /// <param name="onEarth">latitude and longitude of position on earth</param>
    /// <param name="earthPosition">position of earth in km assuming moon is [0,0,0]</param>
    /// <returns>double that is azimuth angle</returns>
    public static double azimuthAngle(geographic moonDegrees, geographic onEarth, position earthPosition)
    {
        geographic earthDegrees = new geographic(Math.Atan2(-42100_000, 361000_000), 0);
        geographic earth = new geographic(earthDegrees.lat, earthDegrees.lon);
        geographic moon = new geographic(moonDegrees.lat, moonDegrees.lon);
        return Mathf.Rad2Deg * Math.Atan2((Math.Sin(earth.lon - moon.lon) * Math.Cos(earth.lat)), ((Math.Cos(moon.lat) * Math.Sin(earth.lat)) - (Math.Sin(moon.lat) * Math.Cos(earth.lat) * Math.Cos(earth.lon - moon.lat))));
    }
    /// <summary>
    /// elevation angles
    /// </summary>
    /// <param name="moon">position of point on moon in latitudes and longitudes</param>
    /// <param name="moonTerrainHeight">height of terrain at position</param>
    /// <param name="moonRadius">radius of the moon</param>
    /// <param name="earth">position on earth in latitudes and longitudes</param>
    /// <param name="earthTerrainHeight">height of terrain on earth</param>
    /// <param name="earthRadius">radius of earth</param>
    /// <returns>double of elevation angle</returns>
    public static double elevationAngle(geographic moon, double moonTerrainHeight, double moonRadius, geographic earth, double earthTerrainHeight, double earthRadius)
    {
        position mpos = new position((moonRadius + moonTerrainHeight) * Math.Cos(moon.lat) * Math.Cos(moon.lon), (moonRadius + moonTerrainHeight) * Math.Cos(moon.lat) * Math.Sin(moon.lon), (moonRadius + moonTerrainHeight) * Math.Sin(moon.lat));
        position epos = new position((earthRadius + earthTerrainHeight) * Math.Cos(earth.lat) * Math.Cos(earth.lon), (earthRadius + earthTerrainHeight) * Math.Cos(earth.lat) * Math.Sin(earth.lon), (earthRadius + earthTerrainHeight) * Math.Sin(earth.lat));

        position vector = epos - mpos;
        double range = Math.Sqrt(Math.Pow(vector.x, 2) + Math.Pow(vector.y, 2) + Math.Pow(vector.z, 2));
        double rz = vector.x * Math.Cos(moon.lat) * Math.Cos(moon.lon) + vector.y * Math.Cos(moon.lat) * Math.Sin(moon.lon) + vector.z * Math.Sin(moon.lat);
        return Mathf.Rad2Deg*Math.Asin(rz / range);
    }

    /// <summary> Returns a 1D list (flattened) of all the values in the given regional CSV file. </summary>
    private List<double> csvParse(string path) {
        List<double> listA = new List<double>();
        using (var reader = new StreamReader(@path)) {
            List<double> listB = new List<double>();
            while (!reader.EndOfStream) {
                var line = reader.ReadLine();
                var values = line.Split(';');

                for (int i = 0; i < values.Length; i += 1) {
                    var values1 = values[i].Split(',');
                    trueYSize = values1.Length;

                    for (int n = 0; n < values1.Length; n += 1) listA.Add(double.Parse(values1[n]));
                }
            }
        }

        trueXSize = (int) Math.Floor((double) (listA.Count / trueYSize));
        return (listA);
    }

    /// <summary> Generates a mesh given a vector3 array of vertices. </summary>
    private Mesh generateMesh(Vector3[] vertice, int xSize, int ySize) {
        
        Mesh m = new Mesh();
        m.indexFormat= UnityEngine.Rendering.IndexFormat.UInt32;
        //m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; //NEEDS TO BE UINT32 APPARENTLY
        m.vertices = vertice;
        int tri = 0;
        int ver = 0;
        int[] triangles = new int[xSize * ySize * 6];
        for (int d = 0; d < ySize - 1; d++) {
            for (int i = 0; i < xSize - 1; i++) {
                triangles[tri + 5] = 0 + ver;
                triangles[tri + 4] = (xSize - 1) + 1 + ver;
                triangles[tri + 3] = ver + 1;
                triangles[tri + 2] = 0 + ver + 1;
                triangles[tri + 1] = (xSize - 1) + 1 + ver;
                triangles[tri + 0] = ver + (xSize - 1) + 2;
                ver++;
                tri += 6;
            }
            ver++;
        }
        m.triangles = triangles;
        m.RecalculateNormals();//Recalculates normals so that lighting works

        return m;
    }
}
