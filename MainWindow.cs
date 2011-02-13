using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using OpenTKGUI;

namespace XChess
{
    /// <summary>
    /// Main program window.
    /// </summary>
    public class MainWindow : HostWindow
    {
        public MainWindow() : base("XChess", 640, 480)
        {
            this.VSync = VSyncMode.Off;

            Board board = Board.Initial;
            this.Control = this._LayerContainer = new LayerContainer(new BoardView(board).WithBorder(3.0).WithCenterAlign(new Point(800.0, 800.0)));
            this.WindowState = WindowState.Maximized;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.DepthBufferBit);
            GL.MatrixMode(MatrixMode.Projection);
            base.OnRenderFrame(e);
        }

        /// <summary>
        /// Program main entry-point.
        /// </summary>
        public static void Main(string[] Args)
        {
            new MainWindow().Run(120.0);
        }

        private LayerContainer _LayerContainer;
    }
}