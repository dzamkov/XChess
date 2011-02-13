using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace XChess
{
    /// <summary>
    /// A bunch of vertices and triangles.
    /// </summary>
    public class Mesh
    {
        private Mesh()
        {

        }

        /// <summary>
        /// Loads a mesh from a wavefront object file.
        /// </summary>
        public static Mesh LoadOBJ(Path File)
        {
            List<_VertexData> vertexdata = new List<_VertexData>();
            List<uint> tridata = new List<uint>();

            var parsestyle = System.Globalization.CultureInfo.InvariantCulture;

            string[] lines = System.IO.File.ReadAllLines(File.PathString);
            foreach (string line in lines)
            {
                string[] lineparts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);  

                // Vertex entry
                if (lineparts[0] == "v")
                {
                    vertexdata.Add(new _VertexData()
                    {
                        Pos = new Vector3(
                            float.Parse(lineparts[1], parsestyle),
                            float.Parse(lineparts[3], parsestyle),
                            float.Parse(lineparts[2], parsestyle))
                    });
                }

                // Face entry
                if (lineparts[0] == "f")
                {
                    uint[] verts = new uint[lineparts.Length - 1];
                    for (int t = 0; t < verts.Length; t++)
                    {
                        verts[t] = uint.Parse(lineparts[t + 1], parsestyle) - 1;
                    }
                    for (int t = 0; t < verts.Length - 2; t++)
                    {
                        tridata.Add(verts[0]);
                        tridata.Add(verts[t + 1]);
                        tridata.Add(verts[t + 2]);
                    }
                }
            }

            _ComputeNormals(vertexdata, tridata);
            return _Load(vertexdata, tridata);
        }

        /// <summary>
        /// Computes normals for a mesh.
        /// </summary>
        private static void _ComputeNormals(List<_VertexData> VertexData, List<uint> Tris)
        {
            for (int t = 0; t < Tris.Count / 3; t++)
            {
                uint a = Tris[t * 3 + 0];
                uint b = Tris[t * 3 + 1];
                uint c = Tris[t * 3 + 2];
                _VertexData ad = VertexData[(int)a];
                _VertexData bd = VertexData[(int)b];
                _VertexData cd = VertexData[(int)c];
                Vector3 norm = Vector3.Normalize(Vector3.Cross(cd.Pos - ad.Pos, bd.Pos - ad.Pos));
                ad.Norm += norm;
                bd.Norm += norm;
                cd.Norm += norm;
            }
            foreach (_VertexData vd in VertexData)
            {
                vd.Norm.Normalize();
            }
        }

        /// <summary>
        /// Loads a mesh from a set of vertex and triangle data.
        /// </summary>
        private unsafe static Mesh _Load(List<_VertexData> VertexData, List<uint> Tris)
        {
            uint eid;
            uint aid;
            GL.GenBuffers(1, out eid);
            GL.GenBuffers(1, out aid);
            GL.BindBuffer(BufferTarget.ArrayBuffer, aid);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, eid);
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(_VertexData.Size * VertexData.Count), IntPtr.Zero, BufferUsageHint.StaticDraw);
            GL.BufferData(BufferTarget.ElementArrayBuffer, new IntPtr(sizeof(uint) * Tris.Count), IntPtr.Zero, BufferUsageHint.StaticDraw);
            float* aptr = (float*)GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.WriteOnly);
            Vector3 boundsmin = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            Vector3 boundsmax = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
            foreach (_VertexData vd in VertexData)
            {
                aptr[0] = vd.Norm.X;
                aptr[1] = vd.Norm.Y;
                aptr[2] = vd.Norm.Z;
                aptr[3] = vd.Pos.X;
                aptr[4] = vd.Pos.Y;
                aptr[5] = vd.Pos.Z;
                boundsmin.X = Math.Min(boundsmin.X, vd.Pos.X);
                boundsmin.Y = Math.Min(boundsmin.Y, vd.Pos.Y);
                boundsmin.Z = Math.Min(boundsmin.Z, vd.Pos.Z);
                boundsmax.X = Math.Max(boundsmax.X, vd.Pos.X);
                boundsmax.Y = Math.Max(boundsmax.Y, vd.Pos.Y);
                boundsmax.Z = Math.Max(boundsmax.Z, vd.Pos.Z);
                aptr += 6;
            }
            GL.UnmapBuffer(BufferTarget.ArrayBuffer);
            uint* eptr = (uint*)GL.MapBuffer(BufferTarget.ElementArrayBuffer, BufferAccess.WriteOnly);
            foreach (uint ed in Tris)
            {
                *eptr = ed;
                eptr++;
            }
            GL.UnmapBuffer(BufferTarget.ElementArrayBuffer);
            return new Mesh()
            {
                _ElementCount = Tris.Count,
                _ArrayID = aid,
                _ElementID = eid,
                _BoundsMin = boundsmin,
                _BoundsMax = boundsmax
            };
        }

        private class _VertexData
        {
            public const int Size = sizeof(float) * (3 + 3);
            public Vector3 Pos;
            public Vector3 Norm;
        }

        /// <summary>
        /// Draws the mesh to the current GL context.
        /// </summary>
        public void Render()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, this._ArrayID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, this._ElementID);
            GL.InterleavedArrays(InterleavedArrayFormat.N3fV3f, 0, IntPtr.Zero);
            GL.DrawElements(BeginMode.Triangles, this._ElementCount, DrawElementsType.UnsignedInt, 0);
        }

        /// <summary>
        /// Gets the lower corner of the bounding box for this mesh.
        /// </summary>
        public Vector3 BoundsMin
        {
            get
            {
                return this._BoundsMin;
            }
        }

        /// <summary>
        /// Gets the upper corner of the bounding box for this mesh.
        /// </summary>
        public Vector3 BoundsMax
        {
            get
            {
                return this._BoundsMax;
            }
        }

        private Vector3 _BoundsMin;
        private Vector3 _BoundsMax;
        private int _ElementCount;
        private uint _ElementID;
        private uint _ArrayID;
    }
}