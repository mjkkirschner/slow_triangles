using Autodesk.DesignScript.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace slow_triangles.DynamoNodes
{
    [SupressImportIntoVM]
    public static class meshHelpers
    {
        public static List<List<T>> Split<T>(List<T> source, int subListLength)
        {
            return source.
               Select((x, i) => new { Index = i, Value = x })
               .GroupBy(x => x.Index / subListLength)
               .Select(x => x.Select(v => v.Value).ToList())
               .ToList();
        }
    }

    [SupressImportIntoVM]
    public static class GeometryExtensions
    {
        public static Vector3 ToVector3(this Autodesk.DesignScript.Geometry.Point pt)
        {
            return new Vector3((float)pt.X, (float)pt.Y, (float)pt.Z);
        }
        public static Vector3 ToVector3(this Autodesk.DesignScript.Geometry.Vector vec)
        {
            return new Vector3((float)vec.X, (float)vec.Y, (float)vec.Z);
        }
    }
}
