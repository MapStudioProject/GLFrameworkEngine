using System;
using System.Linq;
using OpenTK;
using System.Collections.Generic;

namespace GLFrameworkEngine
{
    public class RevertableDelPointCollection : IRevertable
    {
        private List<PointInfo> Points = new List<PointInfo>();

        public RevertableDelPointCollection(List<RenderablePathPoint> points)
        {
            foreach (var point in points)
            {
                Points.Add(new PointInfo(point));

                //Revert all the parent/children values too
                foreach (var parent in point.Parents)
                    Points.Add(new PointInfo(parent));
                foreach (var child in point.Children)
                    Points.Add(new PointInfo(child));
            }
        }

        public IRevertable Revert()
        {
            var infos = new List<RenderablePathPoint>();
            foreach (var info in Points)
                infos.Add(info.Point);

            var revert = new RevertableAddPointCollection(infos);

            foreach (var info in Points.OrderBy(x => x.Index))
            {
                info.Point.Parents.Clear();
                info.Point.Children.Clear();

                foreach (var parent in info.Parents)
                    info.Point.Parents.Add(parent);
                foreach (var child in info.Children)
                    info.Point.Children.Add(child);

                if (!info.ParentPath.PathPoints.Contains(info.Point))
                {
                    info.ParentPath.AddPoint(info.Point, info.Index);
                    info.ParentPath.OnPointAdded(info.Point);
                }
                info.Point.IsSelected = true;
            }


            return revert;
        }

        public class PointInfo
        {
            public RenderablePathPoint Point;
            public RenderablePath ParentPath;
            public List<RenderablePathPoint> Children = new List<RenderablePathPoint>();
            public List<RenderablePathPoint> Parents = new List<RenderablePathPoint>();

            public int Index { get; set; }

            public PointInfo(RenderablePathPoint point)
            {
                Index = point.ParentPath.PathPoints.IndexOf(point);

                ParentPath = point.ParentPath;
                Point = point;
                foreach (var child in point.Children)
                    Children.Add(child);
                foreach (var parent in point.Parents)
                    Parents.Add(parent);
            }
        }
    }

    public class RevertableTransformPointCollection : IRevertable
    {
        private List<PointInfo> Points = new List<PointInfo>();

        public RevertableTransformPointCollection(List<RenderablePathPoint> points)
        {
            foreach (var point in points)
                Points.Add(new PointInfo(point));
        }

        public IRevertable Revert()
        {
            var infos = new List<RenderablePathPoint>();
            foreach (var info in Points)
            {
                info.Point.Transform.Position = info.position;
                info.Point.Transform.Rotation = info.rotation;
                info.Point.Transform.Scale = info.scale;

                infos.Add(info.Point);
            }

            return new RevertableDelPointCollection(infos);
        }

        public class PointInfo
        {
            public RenderablePathPoint Point;
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;

            public PointInfo(RenderablePathPoint point)
            {
                Point = point;
                position = point.Transform.Position;
                scale = point.Transform.Scale;
                rotation = point.Transform.Rotation;
            }
        }
    }

    public class RevertableParentChildCollection : IRevertable
    {
        private List<PointInfo> Points = new List<PointInfo>();

        public RevertableParentChildCollection(RenderablePathPoint point)
        {
            Points.Add(new PointInfo(point));
        }

        public RevertableParentChildCollection(List<PointInfo> points)
        {
            Points = points.ToList();
        }

        public RevertableParentChildCollection(List<RenderablePathPoint> points)
        {
            foreach (var point in points)
                Points.Add(new PointInfo(point));
        }

        public IRevertable Revert()
        {
            var infos = new List<PointInfo>();

            //Remove all the hovered points from the target's children
            foreach (var info in Points)
            {
                infos.Add(new PointInfo(info.Point));
                info.Revert();
            }

            return new RevertableParentChildCollection(infos);
        }

        public class PointInfo
        {
            public RenderablePath ParentPath;
            public RenderablePathPoint Point;
            public List<RenderablePathPoint> Children = new List<RenderablePathPoint>();
            public List<RenderablePathPoint> Parents = new List<RenderablePathPoint>();

            public PointInfo(RenderablePathPoint point)
            {
                ParentPath = point.ParentPath;
                Point = point;
                foreach (var child in point.Children)
                    Children.Add(child);
                foreach (var parent in point.Parents)
                    Parents.Add(parent);
            }

            public void Revert()
            {
                foreach (var point in ParentPath.PathPoints)
                {
                    if (point.Children.Contains(Point))
                        point.Children.Remove(Point);
                    if (point.Parents.Contains(Point))
                        point.Parents.Remove(Point);
                }
                Point.Children.Clear();
                Point.Parents.Clear();

                //Re connect the point's children and points
                foreach (var parent in Parents)
                    parent.AddChild(Point);

                foreach (var child in Children)
                    Point.AddChild(child);

            }
        }
    }

    public class RevertableConnectPointCollection : IRevertable
    {
        private List<PointInfo> Points = new List<PointInfo>();
        private PointInfo TargetPoint;

        public RevertableConnectPointCollection(RenderablePathPoint point, RenderablePathPoint target)
        {
            TargetPoint = new PointInfo(target);
            Points.Add(new PointInfo(point));
        }

        public RevertableConnectPointCollection(List<RenderablePathPoint> points, RenderablePathPoint target)
        {
            TargetPoint = new PointInfo(target);
            foreach (var point in points)
                Points.Add(new PointInfo(point));
        }

        public IRevertable Revert()
        {
            var infos = new List<RenderablePathPoint>();
            var point = TargetPoint.Point;

            //Re add the hovered target point
            TargetPoint.ParentPath.AddPoint(point);
            TargetPoint.ParentPath.OnPointAdded(point);

            TargetPoint.Revert();

            //Remove all the hovered points from the target's children
            foreach (var info in Points)
            {
                info.Revert();
                infos.Add(info.Point);
            }

            return new RevertableConnectPointCollection(infos, point);
        }

        public class PointInfo
        {
            public RenderablePath ParentPath;
            public RenderablePathPoint Point;
            public List<RenderablePathPoint> Children = new List<RenderablePathPoint>();
            public List<RenderablePathPoint> Parents = new List<RenderablePathPoint>();

            public PointInfo(RenderablePathPoint point)
            {
                ParentPath = point.ParentPath;
                Point = point;
                foreach (var child in point.Children)
                    Children.Add(child);
                foreach (var parent in point.Parents)
                    Parents.Add(parent);
            }

            public void Revert()
            {
                foreach (var point in ParentPath.PathPoints)
                {
                    if (point.Children.Contains(Point))
                        point.Children.Remove(Point);
                    if (point.Parents.Contains(Point))
                        point.Parents.Remove(Point);
                }
                Point.Children.Clear();
                Point.Parents.Clear();

                //Re connect the point's children and points
                foreach (var parent in Parents)
                    parent.AddChild(Point);

                foreach (var child in Children)
                    Point.AddChild(child);

            }
        }
    }

    public class RevertableAddPointCollection : IRevertable
    {
        private List<PointInfo> Points = new List<PointInfo>();

        public RevertableAddPointCollection(RenderablePathPoint point)
        {
            Points.Add(new PointInfo(point));
        }

        public RevertableAddPointCollection(List<RenderablePathPoint> points)
        {
            foreach (var point in points)
                Points.Add(new PointInfo(point));
        }

        public IRevertable Revert()
        {
            var infos = new List<RenderablePathPoint>();
            foreach (var info in Points)
                infos.Add(info.Point);

            var revert = new RevertableDelPointCollection(infos);
            foreach (var info in Points) 
                info.ParentPath.RemovePoint(info.Point);

            if (Points.Count > 0)
            {
                var path = Points[0].ParentPath;
                path.RemovedSelectedCallback?.Invoke(path, EventArgs.Empty);
            }

            GLContext.ActiveContext.TransformTools.Clear();

            return revert;
        }

        public class PointInfo
        {
            public RenderablePathPoint Point;
            public RenderablePath ParentPath;
            public List<RenderablePathPoint> Children = new List<RenderablePathPoint>();
            public List<RenderablePathPoint> Parents = new List<RenderablePathPoint>();

            public PointInfo(RenderablePathPoint point)
            {
                ParentPath = point.ParentPath;
                Point = point;
                foreach (var child in point.Children)
                    Children.Add(child);
                foreach (var parent in point.Parents)
                    Parents.Add(parent);
            }
        }
    }
}
