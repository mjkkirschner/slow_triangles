//load our obj data for our test model
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
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
                if (split.Length < 4 || split.First() == "f")
                {
                    continue;
                }
                var x = float.Parse(split[1]);
                var y = float.Parse(split[2]);
                var z = float.Parse(split[3]);

                output.Add(new Vector4(x, y, z, 1));

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
                if (split.Length < 4 || split.First() == "v")
                {
                    continue;
                }
                var a = int.Parse(split[1]);
                var b = int.Parse(split[2]);
                var c = int.Parse(split[3]);

                output.Add(new TriangleFace(new int[3] { a, b, c }));

            }
            return output;
        }

    }
}