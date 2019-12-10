//load our obj data for our test model
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using renderer.dataStructures;
using renderer.interfaces;

namespace renderer.core
{
    public static class ObjFileLoader
    {
        /// <summary>
        /// Loads verts from an wavefront obj file from a file path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<Vector4> LoadVertsFromObjAtPath(FileInfo path)
        {
            var text = File.ReadAllText(path.FullName);
            var lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            var output = new List<Vector4>();
            foreach (var line in lines)
            {
                var split = line.Split(' ');
                if (split.First() == "v")
                {

                    var x = float.Parse(split[1]);
                    var y = float.Parse(split[2]);
                    var z = float.Parse(split[3]);

                    output.Add(new Vector4(x, y, z, 1));
                }

            }
            return output;
        }

        /// <summary>
        /// Loads triangle faces from wavefront obj at filepath
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<TriangleFace> LoadTrisFromObjAtPath(FileInfo path)
        {
            var text = File.ReadAllText(path.FullName);
            var lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            var output = new List<TriangleFace>();
            foreach (var line in lines)
            {
                var split = line.Split(' ');

                if (split.First() == "f")
                {

                    var faceData = split.Skip(1).Select(x =>
                    {
                        //each entry in here is /vertI/uvI/normI
                        //ie f v1/vt1/vn1
                        var data = x.Split('/');
                        return data.Select(y => int.Parse(y));
                    });

                    var verts = faceData.Select(x => x.ElementAt(0)).ToArray();
                    var uvs = faceData.Select(x => x.ElementAtOrDefault(1)).ToArray();
                    var normals = faceData.Select(x => x.ElementAtOrDefault(2)).ToArray();
                    output.Add(new TriangleFace(verts, uvs, normals));
                }
            }
            return output;
        }

        public static Mesh LoadMeshFromObjAtPath(FileInfo path)
        {
            var triFaces = LoadTrisFromObjAtPath(path);

            var text = File.ReadAllText(path.FullName);
            var lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            var verts = new List<Vector4>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();


            foreach (var line in lines)
            {
                var split = line.Split(' ');

                if (split.First() == "vt")
                {
                    var u = float.Parse(split[1]);
                    var v = float.Parse(split[2]);
                    uvs.Add(new Vector2(u, v));

                }

                else if (split.First() == "vn")
                {
                    var x = float.Parse(split[1]);
                    var y = float.Parse(split[2]);
                    var z = float.Parse(split[3]);
                    normals.Add(Vector3.Normalize(new Vector3(x, y, z)));
                }

                else if (split.First() == "v")
                {
                    var x = float.Parse(split[1]);
                    var y = float.Parse(split[2]);
                    var z = float.Parse(split[3]);
                    verts.Add(new Vector4(x, y, z, 1));
                }
            }
            return new Mesh(triFaces, verts, normals, uvs);
        }

    }
}