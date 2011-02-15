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
            if(this._Selection != null)
            {
                Color? m = null;
                if(this._Selection.Selected == Square)
                {
                    m = _SelectionInfo.Colors[0];
                }
                _SelectionInfo.SelectAction sa;
                if (this._Selection.Actions.TryGetValue(Square, out sa))
                {
                    m = _SelectionInfo.Colors[sa.Type + 1];
                }
                if (m != null)
                {
                    return Color.Mix(m.Value, b, 0.2);
                }
            }
            return b;
        }

        protected override void OnSquareClick(Square Square, bool Primary)
        {
            if (Primary)
            {
                // Selection
                Piece piece = this.Board.GetPiece(Square);
                if (piece != null)
                {
                    _SelectionInfo si = new _SelectionInfo();
                    si.Selected = Square;
                    var acts = si.Actions = new Dictionary<Square, _SelectionInfo.SelectAction>();
                    foreach (KeyValuePair<Move, Board> m in this._Moves)
                    {
                        PieceMove pm = m.Key as PieceMove;
                        if (pm != null && pm.Source == Square)
                        {
                            int type = 0;
                            if (this.Board.GetPiece(pm.Destination) != null || pm is EnPassantMove)
                            {
                                type = 1;
                            }
                            acts.Add(pm.Destination, new _SelectionInfo.MoveSelectAction()
                            {
                                Type = type,
                                Move = m.Key,
                                Board = m.Value
                            });
                        }
                    }
                    this._Selection = si;
                }
            }
            else
            {
                // Move
                if (this._Selection != null)
                {
                    _SelectionInfo.SelectAction sa;
                    if (this._Selection.Actions.TryGetValue(Square, out sa))
                    {
                        var msa = sa as _SelectionInfo.MoveSelectAction;
                        if (msa != null)
                        {
                            this.IssueMove(msa.Move, msa.Board);
                        }
                    }
                }
            }
        }

        protected override void OnBoardChange(Board NewBoard)
        {
            this._Moves = new List<KeyValuePair<Move, Board>>(NewBoard.Moves);
            this._Selection = null;

            if (this._ScoreSample != null)
            {
                this._ScoreSample.Dispose();
                this._ScoreSample = null;
            }
        }

        public override void OverRender(GUIRenderContext Context)
        {
            if (this._ScoreSample == null)
            {
                this._ScoreSample = Font.Default.CreateSample(this.Board.GetScore(0).ToString());
            }
            Context.DrawText(Color.RGB(0.0, 0.0, 0.0), this._ScoreSample, new Point(10.0, 10.0));
        }

        /// <summary>
        /// Information about a piece selection a player made.
        /// </summary>
        private class _SelectionInfo
        {
            public Square Selected;
            public Dictionary<Square, SelectAction> Actions;

            public static readonly Color[] Colors = new Color[]
            {
                Color.RGB(0.4, 0.8, 0.8),
                Color.RGB(0.5, 0.8, 0.5),
                Color.RGB(0.8, 0.5, 0.5)
            };

            /// <summary>
            /// An action that happens when the player clicks a square (moving)
            /// while this piece is selected.
            /// </summary>
            public class SelectAction
            {
                public int Type;
            }

            /// <summary>
            /// Causes a move to be made.
            /// </summary>
            public class MoveSelectAction : SelectAction
            {
                public Move Move;
                public Board Board;
            }
        }

        private TextSample _ScoreSample;
        private _SelectionInfo _Selection;
        private List<KeyValuePair<Move, Board>> _Moves;
        private int _Player;
    }
}