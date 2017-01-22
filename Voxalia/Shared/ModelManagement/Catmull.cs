using System;
using System.Collections.Generic;
using System.Linq;
using BEPUutilities;

namespace Voxalia.Shared.ModelManagement
{
    // This code found online at https://lotsacode.wordpress.com/2013/04/10/catmull-clark-surface-subdivider-in-c/ - and has a link to its own references below...
    
    public class Shape
    {
        private readonly Lazy<List<Edge>> _lazyAllEdges;
        private readonly Lazy<List<Point>> _lazyAllPoints;

        public Shape()
        {
            Faces = new List<Face>();

            _lazyAllEdges = new Lazy<List<Edge>>(GetAllEdges);
            _lazyAllPoints = new Lazy<List<Point>>(GetAllPoints);
        }

        public List<Face> Faces { get; set; }
        public List<Edge> AllEdges { get { return _lazyAllEdges.Value; } }
        public List<Point> AllPoints { get { return _lazyAllPoints.Value; } }

        public Face AddFace(Face face)
        {
            if (Faces.Any(f => f.IsMatchFor(face)))
            {
                throw new InvalidOperationException("There is already such a face in the shape!");
            }
            Faces.Add(face);
            return face;
        }

        public void RemoveFace(Face face)
        {
            Faces.Remove(face);
            face.Edges.ForEach(e => e.Faces.Remove(face));
        }

        private List<Edge> GetAllEdges()
        {
            return Faces.SelectMany(f => f.Edges).Distinct().ToList();
        }

        private List<Point> GetAllPoints()
        {
            return Faces.SelectMany(f => f.AllPoints).Distinct().ToList();
        }
    }

    public class Face
    {
        private readonly Lazy<Vector3> _lazyNormal;
        private readonly Lazy<List<Point>> _lazyAllPoints;

        public Face(params Edge[] edges)
        {
            Edges = new List<Edge>(edges);
            Edges.ForEach(e => e.AddFace(this));
            _lazyNormal = new Lazy<Vector3>(ComputeNormal);
            _lazyAllPoints = new Lazy<List<Point>>(GetAllPointsInWindingOrder);
        }

        public List<Edge> Edges { get; set; }
        public List<Point> AllPoints { get { return _lazyAllPoints.Value; } }
        public Vector3 Normal { get { return _lazyNormal.Value; } }

        public Point FacePoint { get; set; }

        public bool IsMatchFor(Face face)
        {
            return face.Edges.All(e => Edges.Contains(e));
        }

        private List<Point> GetAllPointsInWindingOrder()
        {
            //return Edges.SelectMany(e => e.Points).Distinct().ToList();
            List<Point> points = new List<Point>();
            // The edges were added in order, but we don't know if the points within the edges are in order. 
            // Therefore we look at the previous edge to figure out which point has already been visited

            Edge previous = Edges.First();
            foreach (Edge current in Edges.Skip(1).Take(Edges.Count - 2))
            {
                if (points.Count == 0)
                {
                    Point firstPoint = previous.PointOnlyInThis(current);
                    points.Add(firstPoint);
                    Point secondPoint = previous.PointInBoth(current);
                    points.Add(secondPoint);
                }

                Point nextPoint = current.PointOnlyInThis(previous);
                points.Add(nextPoint);

                previous = current;
            }

            return points;
        }

        private Vector3 ComputeNormal()
        {
            List<Point> points = GetAllPointsInWindingOrder();
            Vector3 v1 = points[0].Position;
            Vector3 v2 = points[1].Position;
            Vector3 v3 = points[2].Position;
            Vector3 t = -Vector3.Cross(v2 - v1, v3 - v1);
            t.Normalize();
            return t;
        }
    }

    public class Point
    {
        private readonly Lazy<Vector3> _lazyNormal;
        private readonly Lazy<List<Face>> _lazyAllFaces;

        public Point(double x, double y, double z)
            : this(new Vector3(x, y, z))
        {
        }

        public Point(Vector3 position)
        {
            Position = position;
            Edges = new List<Edge>();
            _lazyNormal = new Lazy<Vector3>(ComputeNormal);
            _lazyAllFaces = new Lazy<List<Face>>(GetAllFaces);
        }

        public Vector3 Position { get; set; }
        public Point Successor { get; set; }
        public List<Edge> Edges { get; set; }

        public bool IsOnBorderOfHole
        {
            get
            {
                // a point is on the border of a hole if nfaces != nedges 
                //  with nfaces the number of faces the point belongs to, 
                //  and nedges the number of edges a point belongs to.  

                // A simpler method is if any of the edges the point belongs to is on the border of a hole, then the point is too
                return Edges.Any(e => e.IsOnBorderOfHole);
            }
        }

        public Vector3 Normal { get { return _lazyNormal.Value; } }

        public List<Face> AllFaces { get { return _lazyAllFaces.Value; } }

        public override string ToString()
        {
            return Position.ToString();
        }

        private Vector3 ComputeNormal()
        {
            List<Face> faces = Edges.SelectMany(e => e.Faces).Distinct().ToList();
            List<Vector3> normals = faces.Select(f => f.Normal).ToList();
            Vector3 avg = CatmullClarkSubdivider.Average(normals);
            avg.Normalize();
            return avg;
        }

        private List<Face> GetAllFaces()
        {
            return
                Edges.SelectMany(e => e.Faces).Distinct().ToList();
        }
    }

    public class Edge
    {
        public Edge(Point p1, Point p2)
        {
            if (p1 == p2)
            {
                throw new InvalidOperationException("Edge must be between two different points!");
            }
            Points = new List<Point> { p1, p2 };
            Faces = new List<Face>();
            p1.Edges.Add(this);
            p2.Edges.Add(this);
        }

        public List<Point> Points { get; set; }
        public List<Face> Faces { get; set; }
        public Point EdgePoint { get; set; }
        public Vector3 Middle { get { return (Points[0].Position + Points[1].Position) * 0.5; } }

        public bool IsOnBorderOfHole
        {
            get
            {
                //  an edge is the border of a hole if it belongs to only one face,
                return Faces.Count == 1;
            }
        }

        public bool IsMatchFor(Point p1, Point p2)
        {
            Point a1 = Points[0];
            Point a2 = Points[1];
            return
                (a1 == p1 && a2 == p2) ||
                (a1 == p2 && a2 == p1);
        }

        public override string ToString()
        {
            return string.Format("{0}-{1}", Points[0], Points[1]);
        }

        public void AddFace(Face face)
        {
            if (Faces.Contains(face))
            {
                throw new InvalidOperationException("Edge already contains face!");
            }
            Faces.Add(face);
        }

        public Point PointInBoth(Edge other)
        {
            Point p1 = Points[0];
            if (other.Points.Contains(p1))
            {
                return p1;
            }
            else
            {
                return Points[1];
            }
        }

        public Point PointOnlyInThis(Edge other)
        {
            Point p1 = Points[0];
            if (!other.Points.Contains(p1))
            {
                return p1;
            }
            else
            {
                return Points[1];
            }
        }
    }

    public class CatmullClarkSubdivider
    {
        public Shape Subdivide(Shape shape)
        {
            // http://rosettacode.org/wiki/Catmull%E2%80%93Clark_subdivision_surface 
            Shape subdivided = new Shape();

            //for each face, a face point is created which is the average of all the points of the face.
            CreateFacePoints(shape);

            //for each edge, an edge point is created which is the average between the center of 
            //  the edge and the center of the segment made with the face points of the two adjacent faces.
            CreateEdgePoints(shape);

            //for each vertex point, its coordinates are updated from (new_coords):
            //    the old coordinates (old_coords),
            //    the average of the face points of the faces the point belongs to (avg_face_points),
            //    the average of the centers of edges the point belongs to (avg_mid_edges),
            //    how many faces a point belongs to (n), then use this formula: 
            //m1 = (n - 3) / n
            //m2 = 1 / n
            //m3 = 2 / n
            //new_coords = (m1 * old_coords)
            //           + (m2 * avg_face_points)
            //           + (m3 * avg_mid_edges)
            CreateVertexPoints(shape);

            //for a triangle face (a,b,c): 
            //   (a, edge_pointab, face_pointabc, edge_pointca)
            //   (b, edge_pointbc, face_pointabc, edge_pointab)
            //   (c, edge_pointca, face_pointabc, edge_pointbc)

            //for a quad face (a,b,c,d): 
            //   (a, edge_pointab, face_pointabcd, edge_pointda)
            //   (b, edge_pointbc, face_pointabcd, edge_pointab)
            //   (c, edge_pointcd, face_pointabcd, edge_pointbc)
            //   (d, edge_pointda, face_pointabcd, edge_pointcd)
            CreateFaces(shape, subdivided);

            return subdivided;
        }

        private void CreateFacePoints(Shape shape)
        {
            //for each face, a face point is created which is the average of all the points of the face.
            foreach (Face face in shape.Faces)
            {
                List<Point> points = face.AllPoints;
                face.FacePoint = new Point(Average(points));
            }
        }

        private void CreateEdgePoints(Shape shape)
        {
            //for each edge, an edge point is created which is the average between the center of 
            //  the edge and the center of the segment made with the face points of the two adjacent faces.
            List<Edge> edges = shape.AllEdges;
            foreach (Edge edge in edges)
            {
                if (edge.IsOnBorderOfHole)
                {
                    Vector3 position =
                        Average(
                            edge.Points[0],
                            edge.Points[1]);

                    edge.EdgePoint = new Point(position);
                }
                else
                {
                    Vector3 position =
                        Average(
                            edge.Points[0],
                            edge.Points[1],
                            edge.Faces[0].FacePoint,
                            edge.Faces[1].FacePoint);

                    edge.EdgePoint = new Point(position);
                }
            }
        }

        private void CreateVertexPoints(Shape shape)
        {
            //for each vertex point, its coordinates are updated from (new_coords):
            //    the old coordinates (old_coords),
            //    the average of the centers of edges the point belongs to (avg_mid_edges),
            //    the average of the face points of the faces the point belongs to (avg_face_points),
            //    how many faces a point belongs to (n), then use this formula: 
            //m1 = (n - 3) / n
            //m2 = 1 / n
            //m3 = 2 / n
            //new_coords = (m1 * old_coords)
            //           + (m2 * avg_face_points)
            //           + (m3 * avg_mid_edges) 
            List<Point> allPoints = shape.AllPoints;
            List<Edge> allEdges = shape.AllEdges;

            foreach (Point oldPoint in allPoints)
            {
                if (oldPoint.IsOnBorderOfHole)
                {
                    oldPoint.Successor = CreateVertexPointForBorderPoint(oldPoint);
                }
                else
                {
                    oldPoint.Successor = CreateVertexPoint(allEdges, oldPoint);
                }
            }
        }

        private Point CreateVertexPoint(List<Edge> allEdges, Point oldPoint)
        {
            //    the average of the face points of the faces the point belongs to (avg_face_points),
            //    how many faces a point belongs to (n), then use this formula: 

            //    the average of the centers of edges the point belongs to (avg_mid_edges),
            Vector3 avgMidEdges = Average(oldPoint.Edges.Select(e => e.Middle));

            //    the average of the face points of the faces the point belongs to (avg_face_points),
            List<Face> pointFaces = oldPoint.Edges.SelectMany(e => e.Faces).Distinct().ToList();
            Vector3 avgFacePoints = Average(pointFaces.Select(pf => pf.FacePoint));

            int faceCount = pointFaces.Count;

            double m1 = (faceCount - 3f) / faceCount;
            double m2 = 1f / faceCount;
            double m3 = 2f / faceCount;

            Point newPoint = new Point(m1 * oldPoint.Position + m2 * avgFacePoints + m3 * avgMidEdges);
            return newPoint;
        }

        private Point CreateVertexPointForBorderPoint(Point oldPoint)
        {
            //for the vertex points that are on the border of a hole, the new coordinates are calculated as follows:
            // in all the edges the point belongs to, only take in account the middles of the edges that are on the border of the hole
            // calculate the average between these points (on the hole boundary) and the old coordinates (also on the hole boundary). 
            List<Vector3> positions = oldPoint.Edges.Where(e => e.IsOnBorderOfHole).Select(e => e.Middle).ToList();
            positions.Add(oldPoint.Position);

            return new Point(Average(positions));
        }

        private void CreateFaces(Shape shape, Shape subdivided)
        {
            List<Face> faces = shape.Faces;
            List<Edge> existingEdges = new List<Edge>();
            foreach (Face face in faces)
            {
                if (face.AllPoints.Count() == 3)
                {
                    CreateTriangleFace(existingEdges, subdivided, face);
                }
                else if (face.AllPoints.Count() == 4)
                {
                    CreateQuadFace(existingEdges, subdivided, face);
                }
                else
                {
                    throw new InvalidOperationException(string.Format("Unhandled facetype (point count={0})!", face.AllPoints.Count()));
                }
            }
            SubdivisionUtilities.VerifyThatThereAreNoEdgeDuplicates(existingEdges);
        }

        private void CreateTriangleFace(List<Edge> existingEdges, Shape subdivided, Face face)
        {
            List<Point> points = face.AllPoints;
            Point a = points[0];
            Point b = points[1];
            Point c = points[2];

            //for a triangle face (a,b,c): 
            //   (a, edge_pointab, face_pointabc, edge_pointca)
            //   (b, edge_pointbc, face_pointabc, edge_pointab)
            //   (c, edge_pointca, face_pointabc, edge_pointbc)
            Point facePoint = face.FacePoint;

            subdivided.Faces.Add(SubdivisionUtilities.CreateFaceF(existingEdges, a, face.Edges[0].EdgePoint, facePoint));
            subdivided.Faces.Add(SubdivisionUtilities.CreateFaceF(existingEdges, b, face.Edges[1].EdgePoint, facePoint));
            subdivided.Faces.Add(SubdivisionUtilities.CreateFaceF(existingEdges, c, face.Edges[2].EdgePoint, facePoint));
            subdivided.Faces.Add(SubdivisionUtilities.CreateFaceF(existingEdges, facePoint, face.Edges[2].EdgePoint, a));
            subdivided.Faces.Add(SubdivisionUtilities.CreateFaceF(existingEdges, facePoint, face.Edges[0].EdgePoint, b));
            subdivided.Faces.Add(SubdivisionUtilities.CreateFaceF(existingEdges, facePoint, face.Edges[1].EdgePoint, c));

            SubdivisionUtilities.VerifyThatThereAreNoEdgeDuplicates(existingEdges);
        }

        private void CreateQuadFace(List<Edge> existingEdges, Shape subdivided, Face face)
        {
            //                  0 1 2 -> 3 
            //for a quad face (a,b,c,d): 
            //   (a, edge_pointab, face_pointabcd, edge_pointda)
            //   (b, edge_pointbc, face_pointabcd, edge_pointab)
            //   (c, edge_pointcd, face_pointabcd, edge_pointbc)
            //   (d, edge_pointda, face_pointabcd, edge_pointcd)
            List<Point> points = face.AllPoints;
            Point a = points[0].Successor;
            Point b = points[1].Successor;
            Point c = points[2].Successor;
            Point d = points[3].Successor;

            Point facePoint = face.FacePoint;

            subdivided.Faces.Add(SubdivisionUtilities.CreateFaceF(existingEdges, a, face.Edges[0].EdgePoint, facePoint));
            subdivided.Faces.Add(SubdivisionUtilities.CreateFaceF(existingEdges, b, face.Edges[1].EdgePoint, facePoint));
            subdivided.Faces.Add(SubdivisionUtilities.CreateFaceF(existingEdges, c, face.Edges[2].EdgePoint, facePoint));
            subdivided.Faces.Add(SubdivisionUtilities.CreateFaceF(existingEdges, d, face.Edges[3].EdgePoint, facePoint));
            subdivided.Faces.Add(SubdivisionUtilities.CreateFaceF(existingEdges, facePoint, face.Edges[3].EdgePoint, a));
            subdivided.Faces.Add(SubdivisionUtilities.CreateFaceF(existingEdges, facePoint, face.Edges[0].EdgePoint, b));
            subdivided.Faces.Add(SubdivisionUtilities.CreateFaceF(existingEdges, facePoint, face.Edges[1].EdgePoint, c));
            subdivided.Faces.Add(SubdivisionUtilities.CreateFaceF(existingEdges, facePoint, face.Edges[2].EdgePoint, d));

            SubdivisionUtilities.VerifyThatThereAreNoEdgeDuplicates(existingEdges);
        }

        public static Vector3 Average(IEnumerable<Vector3> points)
        {
            int count = 0;
            Vector3 avg = Vector3.Zero;
            foreach (Vector3 p in points)
            {
                count++;
                avg += p;
            }
            return avg / count;
        }

        public static Vector3 Average(IEnumerable<Point> points)
        {
            int count = 0;
            Vector3 avg = Vector3.Zero;
            foreach (Point p in points)
            {
                count++;
                avg += p.Position;
            }
            return avg / count;
        }

        private Vector3 Average(params Point[] points)
        {
            return Average(new List<Point>(points));
        }
    }
    public static class SubdivisionUtilities
    {
        public static Edge[] GetOrCreateEdges(List<Edge> existingEdges, params Point[] points)
        {
            List<Edge> edges = new List<Edge>();

            Point first = points.First();
            Point previous = first;
            foreach (Point point in points.Skip(1))
            {
                Edge edge = GetOrCreateEdge(existingEdges, previous, point);
                edges.Add(edge);
                previous = point;
            }

            edges.Add(GetOrCreateEdge(existingEdges, previous, first));

            return edges.ToArray();
        }

        public static Edge GetOrCreateEdge(List<Edge> existingEdges, Point p1, Point p2)
        {
            Edge edge = existingEdges.SingleOrDefault(e => e.IsMatchFor(p1, p2));

            if (edge == null)
            {
                edge = new Edge(p1, p2);
                existingEdges.Add(edge);
            }
            return edge;
        }

        public static Face CreateFaceF(List<Edge> edges, params Point[] points)
        {
            return new Face(GetOrCreateEdges(edges, points));
        }

        public static Face CreateFaceR(List<Edge> edges, params Point[] points)
        {
            return new Face(GetOrCreateEdges(edges, points.Reverse().ToArray()));
        }

        public static void VerifyThatThereAreNoEdgeDuplicates(List<Edge> edges)
        {
            // Debug code
            //foreach (Edge edge in edges)
            //{
            //    Point p1 = edge.Points[0];
            //    Point p2 = edge.Points[1];
            //    int c1 = edges.Count(e => e.Points[0].Position.Equals(p1.Position) && e.Points[1].Position.Equals(p2.Position));
            //    int c2 = edges.Count(e => e.Points[0].Position.Equals(p2.Position) && e.Points[1].Position.Equals(p1.Position));

            //    if (c1 + c2 != 1)
            //    {
            //        throw new InvalidOperationException("There are edge duplicates!");
            //    }
            //}
        }
    }
}
