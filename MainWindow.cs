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
            this.Control = this._LayerContainer = new LayerContainer(new Blank(Color.RGB(0.0, 0.0, 0.0)));
            this.WindowState = WindowState.Maximized;
        }

        /// <summary>
        /// Program main entry-point.
        /// </summary>
        public static void Main(string[] Args)
        {
            new MainWindow().Run(60.0);
        }

        private LayerContainer _LayerContainer;
    }
}