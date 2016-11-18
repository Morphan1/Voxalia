using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BEPUutilities;

namespace Voxalia.Shared.ModelManagement
{
    // this class is to simplify some things from the original C++ code to work easier in C#
    public class ListHelper<T>
    {
        public List<T> Internal = new List<T>();

        public int Count
        {
            get
            {
                return Internal.Count;
            }
        }

        public void Clear()
        {
            Internal.Clear();
        }

        public void Add(T itm)
        {
            Internal.Add(itm);
        }

        public T this[int index]
        {
            get
            {
                return Internal[index];
            }
            set
            {
                while (index >= Internal.Count)
                {
                    Internal.Add(default(T));
                }
                Internal[index] = value;
            }
        }
    }

    // This code based on C++ code from http://voxels.blogspot.com/2014/05/quadric-mesh-simplification-with-source.html
    // Original license was MIT with copyright:
    // (C) by Sven Forstmann in 2014

    public class SymetricMatrix
    {
        // Constructor

        public SymetricMatrix(double c = 0)
        {
            for (int i = 0; i < 10; i++)
            {
                m[i] = c;
            }
        }

        public SymetricMatrix(double m11, double m12, double m13, double m14,
                            double m22, double m23, double m24,
                                        double m33, double m34,
                                                    double m44)
        {
            m[0] = m11; m[1] = m12; m[2] = m13; m[3] = m14;
            m[4] = m22; m[5] = m23; m[6] = m24;
            m[7] = m33; m[8] = m34;
            m[9] = m44;
        }

        // Make plane

        public SymetricMatrix(double a, double b, double c, double d)
        {
            m[0] = a * a; m[1] = a * b; m[2] = a * c; m[3] = a * d;
            m[4] = b * b; m[5] = b * c; m[6] = b * d;
            m[7] = c * c; m[8] = c * d;
            m[9] = d * d;
        }

        public double this[int c]
        {
            get
            {
                return m[c];
            }
            set
            {
                m[c] = value;
            }
        }

        // Determinant

        public double det(int a11, int a12, int a13,
                    int a21, int a22, int a23,
                    int a31, int a32, int a33)
        {
            double det = m[a11] * m[a22] * m[a33] + m[a13] * m[a21] * m[a32] + m[a12] * m[a23] * m[a31]
                        - m[a13] * m[a22] * m[a31] - m[a11] * m[a23] * m[a32] - m[a12] * m[a21] * m[a33];
            return det;
        }

        public static SymetricMatrix operator +(SymetricMatrix m, SymetricMatrix n)
        {
            return new SymetricMatrix(m[0] + n[0], m[1] + n[1], m[2] + n[2], m[3] + n[3],
                                                m[4] + n[4], m[5] + n[5], m[6] + n[6],
                                                             m[7] + n[7], m[8] + n[8],
                                                                          m[9] + n[9]);
        }

        public double[] m = new double[10];
    }
    ///////////////////////////////////////////


    // Global Variables & Strctures
    public struct Triangle
    {
        public Triangle(int a, int b, int c)
        {
            v = new int[3] { a, b, c };
            err = new double[4];
            deleted = 0;
            dirty = 0;
            n = Vector3.Zero;
        }

        public int[] v;
        public double[] err;
        public int deleted;
        public int dirty;
        public Vector3 n;
    };

    public struct Vertex
    {
        public Vector3 p;
        public int tstart;
        public int tcount;
        public SymetricMatrix q;
        public int border;
    }

    public struct Ref
    {
        public int tid;
        public int tvertex;
    }

    public class Simplify
    {
        public ListHelper<Triangle> triangles = new ListHelper<Triangle>();
        public ListHelper<Vertex> vertices = new ListHelper<Vertex>();
        public ListHelper<Ref> refs = new ListHelper<Ref>();

        // Main simplification function 
        //
        // target_count  : target nr. of triangles
        // agressiveness : sharpness to increase the threashold.
        //                 5..8 are good numbers
        //                 more iterations yield higher quality
        //
        void simplify_mesh(int target_count, double agressiveness = 7)
        {
            // main iteration loop 

            int deleted_triangles = 0;
            ListHelper<int> deleted0 = new ListHelper<int>();
            ListHelper<int> deleted1 = new ListHelper<int>();
            int triangle_count = triangles.Count;

            for (int iteration = 0; iteration < 100; iteration++)
            {
                // target number of triangles reached ? Then break
                if (triangle_count - deleted_triangles <= target_count)
                {
                    break;
                }

                // update mesh once in a while
                if (iteration % 5 == 0)
                {
                    update_mesh(iteration);
                }

                // clear dirty flag
                for (int i = 0; i < triangles.Count; i++)
                {
                    Triangle t = triangles[i];
                    t.dirty = 0;
                    triangles[i] = t;
                }

                //
                // All triangles with edges below the threshold will be removed
                //
                // The following numbers works well for most models.
                // If it does not, try to adjust the 3 parameters
                //
                double threshold = 0.000000001 * Math.Pow((double)(iteration + 3), agressiveness);

                // remove vertices & mark deleted triangles
                for (int i = 0; i < triangles.Count; i++)
                {
                    Triangle t = triangles[i];
                    if (t.err[3] > threshold) continue;
                    if (t.deleted != 0) continue;
                    if (t.dirty != 0) continue;

                    for (int j = 0; j < 3; j++)
                    {
                        if (t.err[j] < threshold)
                        {
                            int i0 = t.v[j];
                            Vertex v0 = vertices[i0];
                            int i1 = t.v[(j + 1) % 3];
                            Vertex v1 = vertices[i1];

                            // Border check
                            if (v0.border != v1.border) continue;

                            // Compute vertex to collapse to
                            Vector3 p = new Vector3();
                            calculate_error(i0, i1, ref p);

                            // dont remove if flipped
                            if (flipped(p, i0, i1, ref v0, ref v1, ref deleted0)) continue;
                            if (flipped(p, i1, i0, ref v1, ref v0, ref deleted1)) continue;

                            // not flipped, so remove edge												
                            v0.p = p;
                            v0.q = v1.q + v0.q;
                            int tstart = refs.Count;

                            update_triangles(i0, ref v0, ref deleted0, ref deleted_triangles);
                            update_triangles(i0, ref v1, ref deleted1, ref deleted_triangles);

                            vertices[i0] = v0;
                            vertices[i1] = v1;

                            int tcount = refs.Count - tstart;

                            if (tcount <= v0.tcount)
                            {
                                // save ram
                                if (tcount != 0)
                                {
                                    for (int f = 0; f < tcount; f++)
                                    {
                                        refs[v0.tstart + f] = refs[tstart];
                                    }
                                }
                            }
                            else
                            {
                                // append
                                v0.tstart = tstart;
                            }

                            v0.tcount = tcount;
                            break;
                        }
                    }
                    triangles[i] = t;
                    // done?
                    if (triangle_count - deleted_triangles <= target_count) break;
                }
            }

            // clean up mesh
            compact_mesh();
        }

        // Check if a triangle flips when this edge is removed

        bool flipped(Vector3 p, int i0, int i1, ref Vertex v0, ref Vertex v1, ref ListHelper<int> deleted)
        {
            int bordercount = 0;
            for (int k = 0; k < v0.tcount; k++)
            {
                Triangle t = triangles[refs[v0.tstart + k].tid];
                if (t.deleted != 0) continue;

                int s = refs[v0.tstart + k].tvertex;
                int id1 = t.v[(s + 1) % 3];
                int id2 = t.v[(s + 2) % 3];

                if (id1 == i1 || id2 == i1) // delete ?
                {
                    bordercount++;
                    deleted[k] = 1;
                    continue;
                }
                Vector3 d1 = vertices[id1].p - p;
                d1.Normalize();
                Vector3 d2 = vertices[id2].p - p;
                d2.Normalize();
                if (Math.Abs(Vector3.Dot(d1, d2)) > 0.999) return true;
                Vector3 n;
                n = Vector3.Cross(d1, d2);
                n.Normalize();
                deleted[k] = 0;
                if (Vector3.Dot(n, t.n) < 0.2) return true;
            }
            return false;
        }

        // Update triangle connections and edge error after a edge is collapsed

        void update_triangles(int i0, ref Vertex v, ref ListHelper<int> deleted, ref int deleted_triangles)
        {
            Vector3 p = new Vector3();
            for (int k = 0; k < v.tcount; k++)
            {
                Ref r = refs[v.tstart + k];
                Triangle t = triangles[r.tid];
                if (t.deleted != 0) continue;
                if (deleted[k] != 0)
                {
                    t.deleted = 1;
                    deleted_triangles++;
                    continue;
                }
                t.v[r.tvertex] = i0;
                t.dirty = 1;
                t.err[0] = calculate_error(t.v[0], t.v[1], ref p);
                t.err[1] = calculate_error(t.v[1], t.v[2], ref p);
                t.err[2] = calculate_error(t.v[2], t.v[0], ref p);
                t.err[3] = Math.Min(t.err[0], Math.Min(t.err[1], t.err[2]));
                refs.Add(r);
                triangles[r.tid] = t;
            }
        }

        // compact triangles, compute edge error and build reference list

        void update_mesh(int iteration)
        {
            if (iteration > 0) // compact triangles
            {
                int dst = 0;
                for (int i = 0; i < triangles.Count; i++)
                {

                    if (triangles[i].deleted == 0)
                    {
                        triangles[dst++] = triangles[i];
                    }
                }
            }
            //
            // Init Quadrics by Plane & Edge Errors
            //
            // required at the beginning ( iteration == 0 )
            // recomputing during the simplification is not required,
            // but mostly improves the result for closed meshes
            //
            if (iteration == 0)
            {

                for (int i = 0; i < vertices.Count; i++)
                {
                    Vertex v = vertices[i];
                    v.q = new SymetricMatrix(0.0);
                    vertices[i] = v;
                }

                for (int i = 0; i < triangles.Count; i++)
                {
                    Triangle t = triangles[i];
                    Vector3 n = new Vector3();
                    Vector3[] p = new Vector3[3];
                    for (int j = 0; j < 3; j++)
                    {
                        p[j] = vertices[t.v[j]].p;
                    }
                    n = Vector3.Cross(p[1] - p[0], p[2] - p[0]);
                    n.Normalize();
                    t.n = n;
                    for (int j = 0; j < 3; j++)
                    {
                        Vertex v = vertices[t.v[j]];
                        v.q = vertices[t.v[j]].q + new SymetricMatrix(n.X, n.Y, n.Z, -Vector3.Dot(n, p[0]));
                        vertices[t.v[j]] = v;
                    }
                    triangles[i] = t;
                }
                for (int i = 0; i < triangles.Count; i++)
                {
                    // Calc Edge Error
                    Triangle t = triangles[i];
                    Vector3 p = new Vector3();
                    for (int j = 0; j < 3; j++)
                    {
                        t.err[j] = calculate_error(t.v[j], t.v[(j + 1) % 3], ref p);
                    }
                    t.err[3] = Math.Min(t.err[0], Math.Min(t.err[1], t.err[2]));
                    triangles[i] = t;
                }
            }

            // Init Reference ID list
            for (int i = 0; i < vertices.Count; i++)
            {
                Vertex v = vertices[i];
                v.tstart = 0;
                v.tcount = 0;
                vertices[i] = v;
            }
            for (int i = 0; i < triangles.Count; i++)
            {
                Triangle t = triangles[i];
                for (int j = 0; j < 3; i++)
                {
                    Vertex v = vertices[t.v[j]];
                    v.tcount++;
                    vertices[t.v[j]] = v;
                }
            }
            int tstart = 0;
            for (int i = 0; i < vertices.Count; i++)
            {
                Vertex v = vertices[i];
                v.tstart = tstart;
                tstart += v.tcount;
                v.tcount = 0;
                vertices[i] = v;
            }

            // Write References
            for (int i = 0; i < triangles.Count; i++)
            {
                Triangle t = triangles[i];
                for (int j = 0; j < 3; j++)
                {
                    Vertex v = vertices[t.v[j]];
                    Ref r = refs[v.tstart + v.tcount];
                    r.tid = i;
                    r.tvertex = j;
                    refs[v.tstart + v.tcount] = r;
                    v.tcount++;
                    vertices[t.v[j]] = v;
                }
                triangles[i] = t;
            }

            // Identify boundary : vertices[].border=0,1 
            if (iteration == 0)
            {
                ListHelper<int> vcount = new ListHelper<int>();
                ListHelper<int> vids = new ListHelper<int>();

                for (int i = 0; i < vertices.Count; i++)
                {
                    Vertex v = vertices[i];
                    v.border = 0;
                    vertices[i] = v;
                }

                for (int i = 0; i < vertices.Count; i++)
                {
                    Vertex v = vertices[i];
                    vcount.Clear();
                    vids.Clear();
                    for (int j = 0; j < v.tcount; j++)
                    {
                        int kb = refs[v.tstart + j].tid;
                        Triangle t = triangles[kb];
                        for (int k = 0; k < 3; k++)
                        {
                            int ofs = 0, id = t.v[k];
                            while (ofs < vcount.Count)
                            {
                                if (vids[ofs] == id) break;
                                ofs++;
                            }
                            if (ofs == vcount.Count)
                            {
                                vcount.Add(1);
                                vids.Add(id);
                            }
                            else
                                vcount[ofs]++;
                        }
                        triangles[kb] = t;
                    }
                    for (int j = 0; j < vcount.Count; j++)
                    {
                        if (vcount[j] == 1)
                        {
                            Vertex vt = vertices[vids[j]];
                            vt.border = 1;
                            vertices[vids[j]] = vt;
                        }
                    }
                }
            }
        }

        // Finally compact mesh before exiting

        void compact_mesh()
        {
            int dst = 0;
            for (int i = 0; i < vertices.Count; i++)
            {
                Vertex v = vertices[i];
                v.tcount = 0;
                vertices[i] = v;
            }
            for (int i = 0; i < triangles.Count; i++)
            {
                if (triangles[i].deleted == 0)
                {
                    Triangle t = triangles[i];
                    triangles[dst++] = t;
                    for (int j = 0; j < 3; j++)
                    {
                        Vertex v = vertices[t.v[j]];
                        v.tcount = 1;
                        vertices[t.v[j]] = v;
                    }
                    triangles[i] = t;
                }
            }

            dst = 0;
            for (int i = 0; i < vertices.Count; i++)
            {
                Vertex v = vertices[i];
                if (v.tcount != 0)
                {
                    v.tstart = dst;
                    vertices[i] = v;
                    v = vertices[dst];
                    v.p = vertices[i].p;
                    vertices[dst] = v;
                    dst++;
                }
            }
            for (int i = 0; i < triangles.Count; i++)
            {
                Triangle t = triangles[i];
                for (int j = 0; j < 3; j++)
                {
                    t.v[j] = vertices[t.v[j]].tstart;
                }
                triangles[i] = t;
            }
        }

        // Error between vertex and Quadric

        double vertex_error(SymetricMatrix q, double x, double y, double z)
        {
            return q[0] * x * x + 2 * q[1] * x * y + 2 * q[2] * x * z + 2 * q[3] * x + q[4] * y * y
                 + 2 * q[5] * y * z + 2 * q[6] * y + q[7] * z * z + 2 * q[8] * z + q[9];
        }

        // Error for one edge

        double calculate_error(int id_v1, int id_v2, ref Vector3 p_result)
        {
            // compute interpolated vertex 

            SymetricMatrix q = vertices[id_v1].q + vertices[id_v2].q;
            bool border = vertices[id_v1].border != 0 && vertices[id_v2].border != 0;
            double error = 0;
            double det = q.det(0, 1, 2, 1, 4, 5, 2, 5, 7);

            if (det != 0 && !border)
            {
                // q_delta is invertible
                p_result.X = -1 / det * (q.det(1, 2, 3, 4, 5, 6, 5, 7, 8)); // vx = A41/det(q_delta) 
                p_result.Y = 1 / det * (q.det(0, 2, 3, 1, 5, 6, 2, 7, 8));  // vy = A42/det(q_delta) 
                p_result.Z = -1 / det * (q.det(0, 1, 3, 1, 4, 6, 2, 5, 8)); // vz = A43/det(q_delta) 
                error = vertex_error(q, p_result.X, p_result.Y, p_result.Z);
            }
            else
            {
                // det = 0 -> try to find best result
                Vector3 p1 = vertices[id_v1].p;
                Vector3 p2 = vertices[id_v2].p;
                Vector3 p3 = (p1 + p2) / 2;
                double error1 = vertex_error(q, p1.X, p1.Y, p1.Z);
                double error2 = vertex_error(q, p2.X, p2.Y, p2.Z);
                double error3 = vertex_error(q, p3.X, p3.Y, p3.Z);
                error = Math.Min(error1, Math.Min(error2, error3));
                if (error1 == error) p_result = p1;
                if (error2 == error) p_result = p2;
                if (error3 == error) p_result = p3;
            }
            return error;
        }
    }
///////////////////////////////////////////
}
