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
    public class BoardView : Control
    {
        public BoardView(Board Board, Path Resources)
        {
            this._CurrentBoard = Board;

            this._SquaresTexture = Texture.Load(Resources["Textures"]["Squares.png"]);
            this._BoardTexture = Texture.Load(Resources["Textures"]["Board.png"]);
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
            }
        }

        public override void Render(GUIRenderContext Context)
        {
            Context.PushClip(new Rectangle(this.Size));
            GL.PushMatrix();
            GL.Scale(-this.Size.X, -this.Size.Y, 1.0);
            GL.Translate(-0.5, -0.5, 0.0);

            int ranks = this._CurrentBoard.Ranks;
            int files = this._CurrentBoard.Files;

            // Set up camera
            Vector3d eyeoffset = new Vector3d(Math.Sin(this._Time) * 20.0, Math.Cos(this._Time) * 20.0, 10.0);
            Vector3d midboard = new Vector3d((double)files * 0.5, (double)ranks * 0.5, 0.0);
            Vector3d up = new Vector3d(0.0, 0.0, 1.0);

            double aspect = this.Size.X / this.Size.Y;
            Matrix4d proj = Matrix4d.CreatePerspectiveFieldOfView(0.7, aspect, 0.1, 100.0);
            GL.MultMatrix(ref proj);
            Matrix4d view = Matrix4d.LookAt(eyeoffset + midboard, midboard, up);
            GL.MultMatrix(ref view);

            // Draw board
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.ColorMaterial);

            GL.Enable(EnableCap.Light0);
            GL.Light(LightName.Light0, LightParameter.Ambient, new Vector4(0.2f, 0.2f, 0.2f, 1.0f));
            GL.Light(LightName.Light0, LightParameter.Diffuse, new Vector4(0.8f, 0.8f, 0.8f, 1.0f));
            GL.Light(LightName.Light0, LightParameter.Position, new Vector4(5.0f, 3.0f, 4.0f, 0.0f));


            GL.Color4(1.0, 1.0, 1.0, 1.0);
            this._SquaresTexture.Bind2D();
            GL.Begin(BeginMode.Quads);
            for (int x = 0; x < files; x++)
            {
                for (int y = 0; y < ranks; y++)
                {
                    const float texw = 0.5f;
                    const float texh = 1.0f;
                    float texadd = ((x + y) % 2 == 0) ? 0.0f : 0.5f;
                    double qx = x;
                    double qy = y;
                    double qxx = qx + 1.0;
                    double qyy = qy + 1.0;
                    GL.Normal3(new Vector3d(0.0, 0.0, 1.0));
                    GL.TexCoord2(texadd, 0f); GL.Vertex2(qx, qy);
                    GL.TexCoord2(texadd, texh); GL.Vertex2(qx, qyy);
                    GL.TexCoord2(texw + texadd, texh); GL.Vertex2(qxx, qyy);
                    GL.TexCoord2(texw + texadd, 0f); GL.Vertex2(qxx, qy);
                }
            }
            GL.End();
            this._BoardTexture.Bind2D();
            double edgedown = 0.3;
            double edgeout = 0.5;
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
            for (int t = 0; t < 4; t++)
            {
                int a = t;
                int b = (t + 1) % 4;
                
                Vector3d ca = boardcorners[a];
                Vector3d cb = boardcorners[b];
                Vector3d cc = boardcorners[b + 4];
                Vector3d cd = boardcorners[a + 4];
                GL.Normal3(Vector3d.Cross(cc - ca, cb - ca));

                float len = (float)((ca - cb).Length);
                GL.TexCoord2(1f, 0f); GL.Vertex3(ca);
                GL.TexCoord2(1f, len); GL.Vertex3(cb);
                GL.TexCoord2(0f, len); GL.Vertex3(cc);
                GL.TexCoord2(0f, 0f); GL.Vertex3(cd);
            }
            GL.End();

            GL.Disable(EnableCap.ColorMaterial);
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.Texture2D);


            GL.PopMatrix();
            Context.Pop();
        }

        public override void Update(GUIControlContext Context, double Time)
        {
            this._Time += Time * 0.2;
        }

        private Texture _SquaresTexture;
        private Texture _BoardTexture;
        private double _Time;
        private Board _CurrentBoard;
    }
}