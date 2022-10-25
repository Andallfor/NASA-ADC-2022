using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;

/// <summary> Brute force generation of regional files. Used in pre-processing of terrain. </summary>
public class regionalMeshGenerator {
    #region VARIABLES
    private int multi, numSubMeshes, trueXSize, trueYSize,res,counts;
    private double radius;
    private string name, basePath, pathToRegion;
    private List<double> lats, lons, heights,slopes;
    private Gradient gradient;
    #endregion

    /// <summary> Generate a region's mesh. Used only in preprocessing, do not actually show to user. </summary>
    /// <param name="regionName">Name of the region. Must be all lowercase. </param>
    /// <param name="multi">Sample every xth point. </param>
    /// <param name="numSubMeshes">Generate numSubMeshes * numSubMeshes amount of meshes. </param>
    /// <param name="radius">Radius of the planet, in km. </param>
    public regionalMeshGenerator(string regionName, int multi, int numSubMeshes, double radius,int res,Gradient gradient) {
        this.name = regionName;
        this.multi = multi;
        this.numSubMeshes = numSubMeshes;
        this.radius = radius;
        this.gradient=gradient;
        this.res = res;
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
        counts = lats.Count;
        generateHeightMap(gradient);
        generateTextureMap(slopes, "slopes",flipped:true);

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
                        k++;
                    }
                }

                meshes[new Vector2Int(i, j)] = generateMesh(verts, width, height);
            }
        }

        return meshes;
    }

    /// <summary>
    /// generateHeightMap
    /// </summary>
    private void generateHeightMap(Gradient gradient)
    {
        double maxY = 0;
        double minY = 0;
        List<double> vertice = new List<double>();
        
        for (int i = 0; i < counts; i++)
        {
            vertice.Add(geographic.toCartesian(lats[i], lons[i], heights[i] + radius).z);
            
        }
        generateTextureMap(vertice, "height",flipped:false);
    }
    private void generateTextureMap(List<double> data,string type,bool flipped)
    {
        
        double minY=0;
        double maxY=0;
        for(int i=0; i <counts; i++)
        {
            if (data[i] > maxY)
            {
                maxY = data[i];
            }
            else if (data[i] < minY)
            {
                minY = data[i];
            }
        }
        Color[] colors = new Color[counts];




        
        Texture2D tex = new Texture2D(trueXSize, trueYSize);
        for (int i = 0; i < counts; i++)
        {

            float height = Mathf.InverseLerp((float)minY, (float)maxY, (float)data[i]);

            colors[i] = gradient.Evaluate(height);
            //tex.SetPixel(i % vertice.Length, Mathf.FloorToInt(i / vertice.Length), gradient.Evaluate(height));


        }
        
        
        tex.SetPixels(colors);
        //tex.SetPixels(colors);
        Byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(general.regionalFileHostLocation.Split(',').Last().Trim() + name + "_"+type+"_TEXTURE.png", bytes);
    }

    /// <summary> Returns a 1D list (flattened) of all the values in the given regional CSV file. </summary>
    private List<double> csvParse(string path) {
        
        List<double> listA = new List<double>();
        using (var reader = new StreamReader(@path))
        {
            List<double> listB = new List<double>();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(';');
                for (int i = 0; i < values.Length; i += res)
                {
                    var values1 = values[i].Split(',');
                    trueYSize = (int)Math.Floor((double)((values1.Length) / res));

                    for (int n = 0; n < values1.Length; n += res)
                    {
                        listA.Add(double.Parse(values1[n]));
                    }
                }
            }
        }
        trueXSize = (int)Math.Floor((double)(listA.Count / (trueYSize)));
        //trueYSize -= 1;
        Debug.Log(trueXSize);
        Debug.Log(trueYSize);
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
