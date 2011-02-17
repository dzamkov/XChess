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
                this.OnBoardChange(value);
            }
        }

        /// <summary>
        /// Issues a move to the board.
        /// </summary>
        public void IssueMove(Move Move, Board NewBoard)
        {
            this.Board = NewBoard;
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
                return this.LookAtPosition + new Vector3d(0.0, -15.0, 15.0);
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

        /// <summary>
        /// Gets the color of a square on the board.
        /// </summary>
        public virtual Color GetSquareColor(Square Square)
        {
            if ((Square.File + Square.Rank) % 2 == 0)
            {
                return Color.RGB(0.7, 0.5, 0.4);
            }
            else
            {
                return Color.RGB(0.7, 0.7, 0.7);
            }
        }

        /// <summary>
        /// Called when a square (or the piece on a square) of the board is clicked.
        /// </summary>
        /// <param name="Primary">Gets if the square was clicked with the primary mouse button.</param>
        protected virtual void OnSquareClick(Square Square, bool Primary)
        {

        }

        /// <summary>
        /// Called when the displayed board is changed, either by the Board property or by issuing a move.
        /// </summary>
        protected virtual void OnBoardChange(Board NewBoard)
        {

        }

        public sealed override void SetupProjection(Point Viewsize)
        {
            Matrix4d view = this.ProjectionMatrix;
            GL.MultMatrix(ref view);
        }

        public sealed override void RenderScene()
        {
            GL.LoadIdentity();

            // Draw board
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.ColorMaterial);


            GL.Enable(EnableCap.Light0);
            GL.Light(LightName.Light0, LightParameter.Position, new Vector4(0.4f, -1.0f, 0.9f, 0.0f));
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

            this._SquaresTexture.Bind2D();
            GL.Begin(BeginMode.Quads);
            for (int x = 0; x < files; x++)
            {
                for (int y = 0; y < ranks; y++)
                {
                    this.GetSquareColor(new Square(y, x)).Render();
                    double qx = x;
                    double qy = y;
                    double qxx = qx + 1.0;
                    double qyy = qy + 1.0;
                    GL.Normal3(new Vector3d(0.0, 0.0, 1.0));
                    GL.TexCoord2(0f, 0f); GL.Vertex2(qx, qy);
                    GL.TexCoord2(1f, 0f); GL.Vertex2(qxx, qy);
                    GL.TexCoord2(1f, 1f); GL.Vertex2(qxx, qyy);
                    GL.TexCoord2(0f, 1f); GL.Vertex2(qx, qyy);
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
            MouseState ms = Context.MouseState;
            if (ms != null)
            {
                // Test for click
                bool primary = false;
                bool click = false;
                if (ms.HasPushedButton(OpenTK.Input.MouseButton.Left))
                {
                    if (Context.SimpleKeyboardState.IsKeyDown(OpenTK.Input.Key.ControlLeft))
                    {
                        primary = false;
                    }
                    else
                    {
                        primary = true;
                    }
                    click = true;
                }
                if (ms.HasPushedButton(OpenTK.Input.MouseButton.Right))
                {
                    primary = false;
                    click = true;
                }

                // Click response
                if (click)
                {
                    Vector3d mouseraydir = this.UnprojectRay(ms.Position);
                    Vector3d mouseraystart = this.EyePosition;

                    

                    // Test if a piece has been clicked
                    foreach (PieceVisual pv in this._Visuals)
                    {
                        Matrix4d itrans = pv.Transform;
                        itrans.Invert();
                        Vector3d nmouseraydir = Vector3d.TransformNormal(mouseraydir, itrans);
                        Vector3d nmouseraystart = Vector3d.Transform(mouseraystart, itrans);
                        if (RayBoxIntersect(nmouseraystart, nmouseraydir, Mesh.FloatToDouble(pv.Mesh.BoundsMin) * 0.9f, Mesh.FloatToDouble(pv.Mesh.BoundsMax) * 0.9f))
                        {
                            // Mouse clicked on piece
                            this.OnSquareClick(pv.Square, primary);
                            click = false;
                        }
                    }

                    // Test if the board has been clicked
                    if (click)
                    {
                        Vector2d boardhit = RayPlaneIntersect(
                            new Vector3d(mouseraystart.Z, mouseraystart.X, mouseraystart.Y),
                            new Vector3d(mouseraydir.Z, mouseraydir.X, mouseraydir.Y));
                        if (boardhit.X >= 0.0 && boardhit.Y >= 0.0)
                        {
                            int ranks = this._CurrentBoard.Ranks;
                            int files = this._CurrentBoard.Files;
                            int tr = (int)boardhit.Y;
                            int tf = (int)boardhit.X;
                            if (tr < ranks && tf < files)
                            {
                                this.OnSquareClick(new Square(tr, tf), primary);
                            }
                        }
                    }
                }
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

        /// <summary>
        /// Gets where (Y, Z) a ray hits the plane at X = 0.
        /// </summary>
        public static Vector2d RayPlaneIntersect(Vector3d RayStart, Vector3d RayDir)
        {
            double dis = -RayStart.X / RayDir.X;
            return new Vector2d(RayStart.Y + RayDir.Y * dis, RayStart.Z + RayDir.Z * dis);
        }

        /// <summary>
        /// Gets if a ray intersects the given box.
        /// </summary>
        public static bool RayBoxIntersect(Vector3d RayStart, Vector3d RayDir, Vector3d BoxMin, Vector3d BoxMax)
        {
            Vector3d rs, rd; Vector2d rh;

            rs = new Vector3d(RayStart.X - BoxMax.X, RayStart.Y, RayStart.Z); rd = new Vector3d(RayDir.X, RayDir.Y, RayDir.Z);
            rh = RayPlaneIntersect(rs, rd);
            if (rh.X >= BoxMin.Y && rh.X <= BoxMax.Y && rh.Y >= BoxMin.Z && rh.Y <= BoxMax.Z) return true;

            rs = new Vector3d(RayStart.X - BoxMin.X, RayStart.Y, RayStart.Z); rd = new Vector3d(RayDir.X, RayDir.Y, RayDir.Z);
            rh = RayPlaneIntersect(rs, rd);
            if (rh.X >= BoxMin.Y && rh.X <= BoxMax.Y && rh.Y >= BoxMin.Z && rh.Y <= BoxMax.Z) return true;

            rs = new Vector3d(RayStart.Y - BoxMax.Y, RayStart.Z, RayStart.X); rd = new Vector3d(RayDir.Y, RayDir.Z, RayDir.X);
            rh = RayPlaneIntersect(rs, rd);
            if (rh.X >= BoxMin.Z && rh.X <= BoxMax.Z && rh.Y >= BoxMin.X && rh.Y <= BoxMax.X) return true;

            rs = new Vector3d(RayStart.Y - BoxMin.Y, RayStart.Z, RayStart.X); rd = new Vector3d(RayDir.Y, RayDir.Z, RayDir.X);
            rh = RayPlaneIntersect(rs, rd);
            if (rh.X >= BoxMin.Z && rh.X <= BoxMax.Z && rh.Y >= BoxMin.X && rh.Y <= BoxMax.X) return true;

            rs = new Vector3d(RayStart.Z - BoxMax.Z, RayStart.X, RayStart.Y); rd = new Vector3d(RayDir.Z, RayDir.X, RayDir.Y);
            rh = RayPlaneIntersect(rs, rd);
            if (rh.X >= BoxMin.X && rh.X <= BoxMax.X && rh.Y >= BoxMin.Y && rh.Y <= BoxMax.Y) return true;

            rs = new Vector3d(RayStart.Z - BoxMin.Z, RayStart.X, RayStart.Y); rd = new Vector3d(RayDir.Z, RayDir.X, RayDir.Y);
            rh = RayPlaneIntersect(rs, rd);
            if (rh.X >= BoxMin.X && rh.X <= BoxMax.X && rh.Y >= BoxMin.Y && rh.Y <= BoxMax.Y) return true;

            return false;
        }

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
        /// Gets the transform applied to the mesh to get its visible state.
        /// </summary>
        public Matrix4d Transform
        {
            get
            {
                return Matrix4d.Scale(0.4, 0.4, 0.4) * Matrix4d.CreateTranslation(this.Square.File + 0.5, this.Square.Rank + 0.5, 0.0);
            }
        }

        /// <summary>
        /// Renders the visual.
        /// </summary>
        public void Render()
        {
            if (this.Mesh != null)
            {
                Matrix4d trans = this.Transform;
                GL.MultMatrix(ref trans);
                Render(this.Mesh, this.State.Player);
            }
        }

        /// <summary>
        /// Renders a mesh for a piece.
        /// </summary>
        public static void Render(Mesh Mesh, int Player)
        {
            if (Player == 1)
            {
                GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Ambient, new Vector4(0.1f, 0.1f, 0.1f, 1.0f));
                GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Specular, new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
                GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Shininess, 32);
                GL.Color4(0.1, 0.1, 0.1, 1.0);
            }
            else
            {
                GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Ambient, new Vector4(0.7f, 0.7f, 0.7f, 1.0f));
                GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Specular, new Vector4(0.9f, 0.9f, 0.9f, 1.0f));
                GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Shininess, 127);
                GL.Color4(0.5, 0.5, 0.5, 1.0);
            }
            Mesh.Render();
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