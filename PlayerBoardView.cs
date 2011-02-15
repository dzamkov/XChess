using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using OpenTKGUI;

namespace XChess
{
    /// <summary>
    /// A view of the board from the perspective of a player.
    /// </summary>
    public class PlayerBoardView : BoardView
    {
        public PlayerBoardView(Board Board, int Player)
            : base(Board)
        {
            this._Player = Player;
        }

        public override Color GetSquareColor(Square Square)
        {
            Color b = base.GetSquareColor(Square);
            if (this._Selected.Rank == Square.Rank || this._Selected.File == Square.File)
            {
                return Color.Mix(Color.RGB(0.4, 0.9, 0.9), b, 0.4);
            }
            else
            {
                return b;
            }
        }

        protected override void OnSquareClick(Square Square, bool Primary)
        {
            this._Selected = Square;
        }

        private Square _Selected;
        private int _Player;
    }
}