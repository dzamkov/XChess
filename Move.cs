using System;
using System.Collections.Generic;

namespace XChess
{
    /// <summary>
    /// A transition between one board state and another.
    /// </summary>
    public abstract class Move : IEquatable<Move>
    {
        /// <summary>
        /// Gets if this move is equivalent to another.
        /// </summary>
        public abstract bool Equals(Move Other);

        public override bool Equals(object obj)
        {
            Move m = obj as Move;
            if (m != null)
            {
                return this.Equals(m);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return 0x00000000;
        }
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

        public override bool Equals(Move Other)
        {
            PieceMove m = Other as PieceMove;
            if (m != null)
            {
                return m.Source == this.Source &&
                    m.Destination == this.Destination &&
                    m.NewState.Equals(this.NewState);
            }
            return false;
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

    /// <summary>
    /// A move of a pawn that causes another pawn to be taken.
    /// </summary>
    public class EnPassantMove : PieceMove
    {
        public override bool Equals(Move Other)
        {
            EnPassantMove m = Other as EnPassantMove;
            if (m != null)
            {
                return m.Source == this.Source &&
                    m.Destination == this.Destination &&
                    m.Captured == this.Captured &&
                    m.NewState.Equals(this.NewState);
            }
            return false;
        }

        /// <summary>
        /// The square of the pawn that was captured with this move.
        /// </summary>
        public Square Captured;
    }

    /// <summary>
    /// A move for castling.
    /// </summary>
    public class CastleMove : Move
    {
        public override bool Equals(Move Other)
        {
            CastleMove m = Other as CastleMove;
            if (m != null)
            {
                return m.KingSource == this.KingSource &&
                    m.KingDestination == this.KingDestination &&
                    m.RookSource == this.RookSource &&
                    m.RookDestination == this.RookDestination &&
                    m.NewKingState == this.NewKingState &&
                    m.NewRookState == this.NewRookState;
            }
            return false;
        }

        /// <summary>
        /// The starting square of the king.
        /// </summary>
        public Square KingSource;

        /// <summary>
        /// The starting square of the rook.
        /// </summary>
        public Square RookSource;

        /// <summary>
        /// The final square for the king.
        /// </summary>
        public Square KingDestination;

        /// <summary>
        /// The final square for the rook.
        /// </summary>
        public Square RookDestination;

        /// <summary>
        /// The new state for the king.
        /// </summary>
        public Piece NewKingState;

        /// <summary>
        /// The new state for the rook.
        /// </summary>
        public Piece NewRookState;
    }
}