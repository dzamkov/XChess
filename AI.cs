using System;
using System.Collections.Generic;

namespace XChess
{
    /// <summary>
    /// A game against an AI.
    /// </summary>
    public class AIGame : Game
    {
        public AIGame(int Player, Board Initial)
        {
            this._Player = Player;
            this._Tree = new BoardTree(Initial);
            this._Random = new Random();

            if (Player == 1)
            {
                this._MakeMove();
            }
        }

        public override Board Board
        {
            get
            {
                return this._Tree.Current;
            }
        }

        public override int Player
        {
            get
            {
                return this._Player;
            }
        }

        /// <summary>
        /// Causes the AI to make its move.
        /// </summary>
        private void _MakeMove()
        {
            this._Tree.Compute(400);

            BoardTree.ScoredMove best = this._Tree.BestMove;
            if (best != null)
            {
                this._Tree = best.NewBoard;
                this.ReceiveMove(best.Move, this._Tree.Current);
            }
        }

        public override void Move(Move Move, Board NewBoard)
        {
            if (this._Tree.Paths == null)
            {
                this._Tree.Compute();
            }
            this._Tree = this._Tree.Paths[Move];
            this._MakeMove();
        }

        private int _Player;
        private BoardTree _Tree;
        private Random _Random;
    }

    /// <summary>
    /// A recursive data structure containing the current board, and all possible board trees that can be reached with a single move.
    /// </summary>
    public class BoardTree
    {
        public BoardTree(Board Current)
        {
            this.Current = Current;
        }

        /// <summary>
        /// Computes the possible paths this board can result in.
        /// </summary>
        public void Compute()
        {
            this.Paths = new Dictionary<Move, BoardTree>(_MoveComparer.Singleton);
            foreach (KeyValuePair<Move, Board> kvp in this.Current.Moves)
            {
                this.Paths.Add(kvp.Key, new BoardTree(kvp.Value));
            }
        }

        /// <summary>
        /// Computes a certain amount of possible boards in the board tree (breadth first). 
        /// </summary>
        public void Compute(int Amount)
        {
            LinkedList<BoardTree> tocompute = new LinkedList<BoardTree>();
            tocompute.AddFirst(this);

            while (tocompute.Count > 0 && Amount > 0)
            {
                BoardTree bt = tocompute.First.Value;
                tocompute.Remove(tocompute.First);

                if (bt.Paths == null)
                {
                    bt.Compute();
                    Amount--;
                }
                foreach (var kvp in bt.Paths)
                {
                    tocompute.AddLast(kvp.Value);
                }
            }
        }

        private class _MoveComparer : EqualityComparer<Move>
        {
            public override bool Equals(Move x, Move y)
            {
                return x.Equals(y);    
            }

            public override int GetHashCode(Move obj)
            {
                return obj.GetHashCode();
            }

            public static readonly _MoveComparer Singleton = new _MoveComparer();
        }

        /// <summary>
        /// Gets the optimal move for this board tree using the minimax algorithim. Requires this tree, and possibly others,
        /// to be computed beforehand. Note that the scores returned are for the player to move.
        /// </summary>
        public ScoredMove BestMove
        {
            get
            {
                int ply = this.Current.PlayerToMove;

                ScoredMove res = null;
                foreach (var kvp in this.Paths)
                {
                    ScoredMove possible;
                    if (kvp.Value.Paths == null || kvp.Value.Paths.Count == 0)
                    {
                        double score = kvp.Value.Current.GetScore(ply);
                        possible = new ScoredMove()
                        {
                            Score = score,
                            Move = kvp.Key,
                            NewBoard = kvp.Value
                        };
                    }
                    else
                    {
                        possible = new ScoredMove()
                        {
                            Score = -kvp.Value.BestMove.Score,
                            Move = kvp.Key,
                            NewBoard = kvp.Value
                        };
                    }
                    if (res == null || possible.Score > res.Score)
                    {
                        res = possible;
                    }
                }
                return res;
            }
        }

        /// <summary>
        /// A move with a known score.
        /// </summary>
        public class ScoredMove
        {
            public double Score;
            public Move Move;
            public BoardTree NewBoard;
        }

        /// <summary>
        /// The current board at the begining of the board tree.
        /// </summary>
        public Board Current;

        /// <summary>
        /// A mapping of possible moves to their resulting board trees, or null if the progression has not yet been determined.
        /// </summary>
        public Dictionary<Move, BoardTree> Paths;
    }
}