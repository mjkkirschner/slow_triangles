using Autodesk.DesignScript.Interfaces;
using Dynamo.Wpf.Rendering;
using slow_triangles_DynamoNodes;
using System;
using System.Collections.Generic;
using System.Text;
using adsk = Autodesk.DesignScript.Geometry;

namespace slow_triangles.DynamoNodes
{
    public class Camera : IGraphicItem
    {
        public Autodesk.DesignScript.Geometry.Point Position { get; }
        public Autodesk.DesignScript.Geometry.Vector UpDirection { get; }
        public Autodesk.DesignScript.Geometry.Point LookTarget { get; }
        public int Width { get; }
        public int Height { get; }
        public int NearDistance { get; }
        public int FarDistance { get; }

        internal Camera(adsk.Point pos, adsk.Point lookTarget, int width = 1, int height = 1, int nearDist = 1, int farDist = 10)
        {
            Position = pos;
            LookTarget = lookTarget;
            Width = width;
            Height = height;
            NearDistance = nearDist;
            FarDistance = farDist;
            //hardcoded for now.
            UpDirection = adsk.Vector.YAxis();

        }
        public static Camera ByCameraInfo(adsk.Point pos, adsk.Vector lookDir, adsk.Point lookTarget, int width = 1, int height = 1, int nearDist = 1, int farDist = 10)
        {

            return new Camera(pos, lookTarget, width, height, nearDist, farDist);

        }

        public void Tessellate(IRenderPackage package, TessellationParameters parameters)
        {

            var sphere = adsk.Sphere.ByCenterPointRadius(this.Position, .05f);
            sphere.Tessellate(package, parameters);

            var line = adsk.Line.ByStartPointEndPoint(this.Position, this.LookTarget);
            line.Tessellate(package, parameters);

            
            var pos1 = line.CoordinateSystemAtDistance(NearDistance);
            var rect1 = adsk.Rectangle.ByWidthLength(pos1.ZXPlane,Width, Height);
            rect1.Tessellate(package, parameters);

            var pos2 = line.CoordinateSystemAtDistance(FarDistance);
            var rect2 = adsk.Rectangle.ByWidthLength(pos2.ZXPlane,Width * FarDistance, Height * FarDistance);
            rect2.Tessellate(package, parameters);

        }

    }
}
