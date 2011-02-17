using System;
using System.Collections.Generic;

namespace XChess
{
    /// <summary>
    /// A game from a single players perspective.
    /// </summary>
    public abstract class Game
    {
        /// <summary>
        /// Gets the current state of the board.
        /// </summary>
        public abstract Board Board { get; }

        /// <summary>
        /// Gets the ID of the current player.
        /// </summary>
        public abstract int Player { get; }

        /// <summary>
        /// Makes a move.
        /// </summary>
        public abstract void Move(Move Move, Board NewBoard);

        /// <summary>
        /// Makes a move, and automatically computes the new board state.
        /// </summary>
        public void Move(Move Move)
        {
            this.Move(Move, this.Board.GetNext(Move));
        }

        /// <summary>
        /// Called when a external move is received.
        /// </summary>
        public event MoveReceivedHandler MoveReceived;
    }

    /// <summary>
    /// A game played on the same board, on the same computer, by two players.
    /// </summary>
    public class LocalGame : Game
    {
        public LocalGame(Board Initial)
        {
            this._Board = Initial;
        }

        public override Board Board
        {
            get
            {
                return this._Board;
            }
        }

        public override int Player
        {
            get
            {
                return this._Board.PlayerToMove;
            }
        }

        public override void Move(Move Move, Board NewBoard)
        {
            this._Board = NewBoard;
        }

        private Board _Board;
    }

    /// <summary>
    /// A handler for an external move being received in a game.
    /// </summary>
    public delegate void MoveReceivedHandler(Move Move, Board NewBoard);
}