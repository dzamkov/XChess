using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using OpenTKGUI;

namespace XChess
{
    /// <summary>
    /// A view of a board.
    /// </summary>
    public class BoardView : Render3DControl
    {
        public BoardView(Board Board)
        {
            this._CurrentBoard = Board;

            Path resources = Path.Resources;
            this._SquaresTexture = Texture.Load(resources["Textures"]["Squares.png"]);
            this._BoardTexture = Texture.Load(resources["Textures"]["Board.png"]);

            this._SetupVisuals();
        }

        /// <summary>
        /// Gets or sets the board currently shown by the view.
        /// </summary>
        public Board Board
        {
            get
            {
                return this._CurrentBoard;
            }
            set
            {
                this._CurrentBoard = value;
                this._SetupVisuals();
            }
        }

        /// <summary>
        /// Sets up visuals for all the pieces.
        /// </summary>
        private void _SetupVisuals()
        {
            this._Visuals = new List<PieceVisual>();
            Board current = this._CurrentBoard;
            int files = current.Files;
            int ranks = current.Ranks;
            for (int x = 0; x < files; x++)
            {
                for (int y = 0; y < ranks; y++)
                {
                    Square sqr = new Square(y, x);
                    Piece piece = current.GetPiece(sqr);
                    if (piece != null)
                    {
                        this._Visuals.Add(new PieceVisual(sqr, piece));
                    }
                }
            }
        }

        /// <summary>
        /// Gets the current projection matrix for the view.
        /// </summary>
        public Matrix4d ProjectionMatrix
        {
            get
            {
                Vector3d up = new Vector3d(0.0, 0.0, 1.0);

                Matrix4d proj = Matrix4d.CreatePerspectiveFieldOfView(Math.Sin(Math.PI / 8.0), this.Size.AspectRatio, 0.1, 100.0);
                Matrix4d view = Matrix4d.LookAt(this.EyePosition, this.LookAtPosition, up);
                return view * proj;
            }
        }

        /// <summary>
        /// Gets the position the player is looking from.
        /// </summary>
        public Vector3d EyePosition
        {
            get
            {
                return this.LookAtPosition + new Vector3d(20.0 * Math.Sin(this._Time), 20.0 * Math.Cos(this._Time), 20.0);
            }
        }

        /// <summary>
        /// Gets where the player is looking to.
        /// </summary>
        public Vector3d LookAtPosition
        {
            get
            {
                int ranks = this._CurrentBoard.Ranks;
                int files = this._CurrentBoard.Files;
                return new Vector3d((double)files * 0.5, (double)ranks * 0.5, 0.0);
            }
        }

        public override void SetupProjection(Point Viewsize)
        {
            Matrix4d view = this.ProjectionMatrix;
            GL.MultMatrix(ref view);
        }

        public override void RenderScene()
        {
            GL.LoadIdentity();

            // Draw board
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.ColorMaterial);


            GL.Enable(EnableCap.Light0);
            GL.Light(LightName.Light0, LightParameter.Position, new Vector4(1.0f, 1.0f, 1.0f, 0.0f));
            GL.Light(LightName.Light0, LightParameter.Ambient, new Vector4(0.3f, 0.3f, 0.3f, 1.0f));
            this._DrawBoard();

            // Pieces
            GL.Enable(EnableCap.Normalize);

            GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.Diffuse);
            
            foreach (PieceVisual pv in this._Visuals)
            {
                GL.PushMatrix();
                pv.Render();
                GL.PopMatrix();
            }

            GL.Disable(EnableCap.Normalize);
            GL.Disable(EnableCap.ColorMaterial);
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
        }

        private void _DrawBoard()
        {
            int ranks = this._CurrentBoard.Ranks;
            int files = this._CurrentBoard.Files;
            GL.Enable(EnableCap.Texture2D);
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Specular, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
            GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.AmbientAndDiffuse);

            GL.Translate(this.EyePosition + this._MouseRay * 20.0);

            this._SquaresTexture.Bind2D();
            GL.Begin(BeginMode.Quads);
            for (int x = 0; x < files; x++)
            {
                for (int y = 0; y < ranks; y++)
                {
                    if (x == 4)
                    {
                        GL.Color4(0.5, 2.0, 0.5, 1.0);
                    }
                    else
                    {
                        GL.Color4(0.8, 0.8, 0.8, 1.0);
                    }

                    const float texw = 0.5f;
                    const float texh = 1.0f;
                    float texadd = ((x + y) % 2 == 0) ? 0.0f : 0.5f;
                    double qx = x;
                    double qy = y;
                    double qxx = qx + 1.0;
                    double qyy = qy + 1.0;
                    GL.Normal3(new Vector3d(0.0, 0.0, 1.0));
                    GL.TexCoord2(texadd, 0f); GL.Vertex2(qx, qy);
                    GL.TexCoord2(texw + texadd, 0f); GL.Vertex2(qxx, qy);
                    GL.TexCoord2(texw + texadd, texh); GL.Vertex2(qxx, qyy);
                    GL.TexCoord2(texadd, texh); GL.Vertex2(qx, qyy);
                }
            }
            GL.End();

            this._BoardTexture.Bind2D();
            double edgedown = 0.4;
            double edgeout = 0.2;
            Vector3d[] boardcorners = new Vector3d[]
            {
                new Vector3d(0.0, 0.0, 0.0),
                new Vector3d(files, 0.0, 0.0),
                new Vector3d(files, ranks, 0.0),
                new Vector3d(0.0, ranks, 0.0),
                new Vector3d(-edgeout, -edgeout, -edgedown),
                new Vector3d(files + edgeout, -edgeout, -edgedown),
                new Vector3d(files + edgeout, ranks + edgeout, -edgedown),
                new Vector3d(-edgeout, ranks + edgeout, -edgedown),
            };
            GL.Begin(BeginMode.Quads);
            GL.Color4(1.0, 1.0, 1.0, 1.0);
            for (int t = 0; t < 4; t++)
            {
                int a = t;
                int b = (t + 1) % 4;

                Vector3d ca = boardcorners[a];
                Vector3d cb = boardcorners[b];
                Vector3d cc = boardcorners[b + 4];
                Vector3d cd = boardcorners[a + 4];
                GL.Normal3(Vector3d.Normalize(Vector3d.Cross(cc - ca, cb - ca)));

                float len = (float)((ca - cb).Length);
                GL.TexCoord2(1f, 0f); GL.Vertex3(ca);
                GL.TexCoord2(0f, 0f); GL.Vertex3(cd);
                GL.TexCoord2(0f, len); GL.Vertex3(cc);
                GL.TexCoord2(1f, len); GL.Vertex3(cb);
            }
            GL.End();
            GL.Disable(EnableCap.Texture2D);
        }

        public override void Update(GUIControlContext Context, double Time)
        {
            this._Time += Time * 0.2;

            MouseState ms = Context.MouseState;
            if (ms != null)
            {
                this._MouseRay = this.UnprojectRay(ms.Position);
            }
        }

        /// <summary>
        /// Gets the direction of the ray projected from the specified point on the control. Note that the ray starts at EyePosition.
        /// </summary>
        public Vector3d UnprojectRay(Point Pos)
        {
            Point size = this.Size;
            Matrix4d iproj = Matrix4d.Invert(this.ProjectionMatrix);
            Vector4d point = new Vector4d(-1.0 + 2.0 * (Pos.X / size.X), 1.0 - 2.0 *(Pos.Y / size.Y), 0.995, 1.0);
            Vector4d res;
            Vector4d.Transform(ref point, ref iproj, out res);
            return Vector3d.Normalize(new Vector3d(res.X / res.W, res.Y / res.W, res.Z / res.W) - this.EyePosition);
        }

        private Vector3d _MouseRay;
        private double _Time;
        private Texture _SquaresTexture;
        private Texture _BoardTexture;
        private List<PieceVisual> _Visuals;
        private Board _CurrentBoard;
    }

    /// <summary>
    /// A visual representation of a piece.
    /// </summary>
    public class PieceVisual
    {
        public PieceVisual(Square Square, Piece State)
        {
            this.Square = Square;
            this.State = State;
            this.Mesh = State.DisplayMesh;
        }



        /// <summary>
        /// Renders the visual.
        /// </summary>
        public void Render()
        {
            if (this.Mesh != null)
            {
                if (this.State.Player == 0)
                {
                    GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Ambient, new Vector4(0.1f, 0.1f, 0.1f, 1.0f));
                    GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Specular, new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                    GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Shininess, 32);
                    GL.Color4(0.0, 0.0, 0.0, 1.0);
                }
                else
                {
                    GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Ambient, new Vector4(0.7f, 0.7f, 0.7f, 1.0f));
                    GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Specular, new Vector4(0.9f, 0.9f, 0.9f, 1.0f));
                    GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Shininess, 127);
                    GL.Color4(0.5, 0.5, 0.5, 1.0);
                    
                }
                GL.Translate(this.Square.File + 0.5, this.Square.Rank + 0.5, 0.0);
                GL.Scale(0.4, 0.4, 0.4);
                this.Mesh.Render();
            }
        }

        /// <summary>
        /// The mesh displayed for the piece.
        /// </summary>
        public Mesh Mesh;

        /// <summary>
        /// The square the piece is on.
        /// </summary>
        public Square Square;

        /// <summary>
        /// The state of the piece.
        /// </summary>
        public Piece State;
    }
}