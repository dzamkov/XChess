using System;
using System.Collections.Generic;

namespace XChess
{
    /// <summary>
    /// A transition between one board state and another.
    /// </summary>
    public class Move
    {

    }

    /// <summary>
    /// A move of a single piece.
    /// </summary>
    public class PieceMove : Move
    {
        /// <summary>
        /// Creates a piece move.
        /// </summary>
        public static PieceMove Create(Square Source, Square Destination, Piece NewState)
        {
            return new PieceMove()
            {
                Source = Source,
                Destination = Destination,
                NewState = NewState
            };
        }

        /// <summary>
        /// The square the piece moved from.
        /// </summary>
        public Square Source;

        /// <summary>
        /// The square the piece moved to.
        /// </summary>
        public Square Destination;

        /// <summary>
        /// The new state at the destination square.
        /// </summary>
        public Piece NewState;
    }
}