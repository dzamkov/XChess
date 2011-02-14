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
            this.Control = this._LayerContainer = new LayerContainer((this._View = new BoardView(board)));
            this.WindowState = WindowState.Maximized;
        }

        /// <summary>
        /// Program main entry-point.
        /// </summary>
        public static void Main(string[] Args)
        {
            new MainWindow().Run(120.0);
        }

        private BoardView _View;
        private LayerContainer _LayerContainer;
    }
}