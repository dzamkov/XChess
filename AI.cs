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
            // Randomly
            if (this._Tree.Paths == null)
            {
                this._Tree.Compute();
            }

            var paths = this._Tree.Paths;
            if (paths.Count > 0)
            {
                int r = this._Random.Next(paths.Count);
                foreach (var kvp in paths)
                {
                    if (r == 0)
                    {
                        this._Tree = kvp.Value;
                        this.ReceiveMove(kvp.Key, this._Tree.Current);
                        break;
                    }
                    r--;
                }
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
        /// The current board at the begining of the board tree.
        /// </summary>
        public Board Current;

        /// <summary>
        /// A mapping of possible moves to their resulting board trees, or null if the progression has not yet been determined.
        /// </summary>
        public Dictionary<Move, BoardTree> Paths;
    }
}