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
                    return Color.Mix(m.Value, b, 0.1);
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

                            // Check if there is already a move to this spot.
                            _SelectionInfo.SelectAction sa;
                            if (acts.TryGetValue(pm.Destination, out sa))
                            {
                                _SelectionInfo.MultiSelectAction mtsa = sa as _SelectionInfo.MultiSelectAction;
                                if (mtsa == null)
                                {
                                    _SelectionInfo.MoveSelectAction msa = sa as _SelectionInfo.MoveSelectAction;
                                    acts[pm.Destination] = mtsa = new _SelectionInfo.MultiSelectAction();
                                    mtsa.Add(msa.Move as PieceMove, msa.Board);
                                    mtsa.Type = 2;
                                }
                                mtsa.Add(pm, m.Value);
                            }
                            else
                            {
                                acts.Add(pm.Destination, new _SelectionInfo.MoveSelectAction()
                                {
                                    Type = type,
                                    Move = pm,
                                    Board = m.Value
                                });
                            }
                        }

                        CastleMove cm = m.Key as CastleMove;
                        if (cm != null && cm.KingSource == Square)
                        {
                            acts.Add(cm.KingDestination, new _SelectionInfo.MoveSelectAction()
                            {
                                Type = 2,
                                Move = cm,
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

                        var mtsa = sa as _SelectionInfo.MultiSelectAction;
                        if (mtsa != null)
                        {
                            this._UpdateAction = mtsa.Display(this);
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

        public override void Update(GUIControlContext Context, double Time)
        {
            base.Update(Context, Time);
            if (this._UpdateAction != null)
            {
                this._UpdateAction(Context);
                this._UpdateAction = null;
            }
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
                Color.RGB(0.3, 0.8, 0.8),
                Color.RGB(0.4, 0.8, 0.4),
                Color.RGB(0.8, 0.4, 0.4),
                Color.RGB(0.8, 0.7, 0.3)
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

            /// <summary>
            /// Causes a popup to display a choice of possible new states for a moving piece.
            /// </summary>
            public class MultiSelectAction : SelectAction
            {
                public MultiSelectAction()
                {
                    this._Items = new List<_Possible>();
                }

                /// <summary>
                /// Adds an item to the possible selections.
                /// </summary>
                public void Add(PieceMove Move, Board Board)
                {
                    this._Items.Add(new _Possible()
                    {
                        Move = Move,
                        Board = Board
                    });
                }

                /// <summary>
                /// Creates a function that displays the selection to the user.
                /// </summary>
                public Action<GUIControlContext> Display(PlayerBoardView BoardView)
                {
                    return delegate(GUIControlContext Context)
                    {
                        LayerContainer lc;
                        Point offset;
                        if (Context.FindAncestor<LayerContainer>(out lc, out offset))
                        {
                            FlowContainer options = new FlowContainer(20.0, Axis.Horizontal);
                            Pane pane = new Pane(new SunkenContainer(options.WithMargin(20.0)).WithBorder(1.0));
                            ModalOptions mo = new ModalOptions()
                            {
                                Lightbox = false,
                                LowestModal = pane,
                                MouseFallthrough = false
                            };

                            foreach (_Possible p in this._Items)
                            {
                                _Possible ip = p;
                                Button button = new Button(ButtonStyle.CreateSolid(Skin.Default), new _PieceIcon(p.Move.NewState).CreateControl());
                                options.AddChild(button, 150.0);
                                button.Click += delegate
                                {
                                    lc.Modal = null;
                                    pane.Dismiss();
                                    BoardView.IssueMove(ip.Move, ip.Board);
                                };
                            }

                            pane.ClientSize = new Point(options.SuggestLength + 42.0, 190.0);
                            Rectangle merect = new Rectangle(offset, Context.Control.Size);

                            lc.AddControl(pane, merect.Location + merect.Size * 0.5 - pane.Size * 0.5);
                            lc.Modal = mo;
                        }
                    };
                }

                private class _PieceIcon : Render3DSurface
                {
                    public _PieceIcon(Piece Piece)
                    {
                        this._Piece = Piece;
                    }

                    public override void SetupProjection(Point Viewsize)
                    {
                        Matrix4d proj = Matrix4d.CreatePerspectiveFieldOfView(Math.Sin(Math.PI / 4.5), Viewsize.AspectRatio, 0.1, 100.0);
                        Matrix4d view = Matrix4d.LookAt(4.0f, 4.0f, 4.0f, 0.0f, 0.0f, 1.4f, 0.0f, 0.0f, 1.0f);
                        GL.MultMatrix(ref proj);
                        GL.MultMatrix(ref view);
                    }

                    public override void RenderScene()
                    {
                        GL.Disable(EnableCap.Texture2D);
                        GL.Enable(EnableCap.CullFace);
                        GL.Enable(EnableCap.DepthTest);
                        GL.Enable(EnableCap.Lighting);
                        GL.Enable(EnableCap.ColorMaterial);
                        GL.Enable(EnableCap.Normalize);
                        GL.Enable(EnableCap.Light0);
                        GL.Light(LightName.Light0, LightParameter.Position, new Vector4(0.7f, 1.1f, 0.9f, 0.0f));
                        GL.Light(LightName.Light0, LightParameter.Ambient, new Vector4(0.3f, 0.3f, 0.3f, 1.0f));
                        GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.Diffuse);
                        PieceVisual.Render(this._Piece.DisplayMesh, this._Piece.Player);
                        GL.Disable(EnableCap.Normalize);
                        GL.Disable(EnableCap.ColorMaterial);
                        GL.Disable(EnableCap.Lighting);
                        GL.Disable(EnableCap.DepthTest);
                        GL.Disable(EnableCap.CullFace);
                    }

                    private Piece _Piece;
                }

                private struct _Possible
                {
                    public PieceMove Move;
                    public Board Board;
                }

                private List<_Possible> _Items;
            }
        }

        private Action<GUIControlContext> _UpdateAction;

        private TextSample _ScoreSample;
        private _SelectionInfo _Selection;
        private List<KeyValuePair<Move, Board>> _Moves;
        private int _Player;
    }
}