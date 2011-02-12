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
        public BoardView(Board Board)
        {
            this._CurrentBoard = Board;
        }

        /// <summary>
        /// Gets the board shown by the view.
        /// </summary>
        public Board Board
        {
            get
            {
                return this._CurrentBoard;
            }
        }

        public override void Render(GUIRenderContext Context)
        {
            Context.PushClip(new Rectangle(this.Size));
            GL.PushMatrix();
            GL.Scale(-this.Size.X, -this.Size.Y, 1.0);
            GL.Translate(-0.5, -0.5, 0.0);

            double aspect = this.Size.X / this.Size.Y;
            Matrix4d proj = Matrix4d.CreatePerspectiveFieldOfView(0.7, aspect, 0.1, 100.0);
            GL.MultMatrix(ref proj);
            Matrix4d view = Matrix4d.LookAt(Math.Sin(this._Time) * 10.0, Math.Cos(this._Time) * 10.0, 5.0, 0.0, 0.0, 0.0, 0.0, 0.0, 1.0);
            GL.MultMatrix(ref view);

            if (this._Test == null)
            {
                this._Test = Font.Default.CreateSample("Hello world");
            }

            GL.Scale(0.1, 0.1, 1.0);
            Context.DrawText(Color.RGB(0.0, 0.0, 0.0), this._Test, new Point(-1.0, -1.0));
            GL.PopMatrix();
            Context.Pop();
        }

        public override void Update(GUIControlContext Context, double Time)
        {
            this._Time += Time;
        }

        private TextSample _Test;
        private double _Time;
        private Board _CurrentBoard;
    }
}