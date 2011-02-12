using System;
using System.Collections.Generic;

namespace XChess
{
    /// <summary>
    /// Represents the state of one square on the board. Can be replaced with null to indicate the square
    /// is empty.
    /// </summary>
    public class Piece
    {
        /// <summary>
        /// Gets the default state for a pawn.
        /// </summary>
        public static PawnPiece Pawn
        {
            get
            {
                return new PawnPiece() { EnPassantThreat = false, CanJump = true };
            }
        }

        /// <summary>
        /// Gets the default state for a rook.
        /// </summary>
        public static RookPiece Rook
        {
            get
            {
                return new RookPiece() { CanCastle = true };
            }
        }

        /// <summary>
        /// Gets the default state for a king.
        /// </summary>
        public static KingPiece King
        {
            get
            {
                return new KingPiece() { CanCastle = true };
            }
        }

        /// <summary>
        /// Gets the default state for a queen.
        /// </summary>
        public static QueenPiece Queen
        {
            get
            {
                return new QueenPiece();
            }
        }

        /// <summary>
        /// Gets the default state for a knight.
        /// </summary>
        public static KnightPiece Knight
        {
            get
            {
                return new KnightPiece();
            }
        }

        /// <summary>
        /// Gets the default state for a bishop.
        /// </summary>
        public static BishopPiece Bishop
        {
            get
            {
                return new BishopPiece();
            }
        }

        /// <summary>
        /// Gets the player that owns this piece.
        /// </summary>
        public int Player;
    }

    /// <summary>
    /// A pawn.
    /// </summary>
    public class PawnPiece : Piece
    {
        /// <summary>
        /// Can this pawn be taken with en passant the next turn?
        /// </summary>
        public bool EnPassantThreat;

        /// <summary>
        /// Can this pawn jump two squares the next turn?
        /// </summary>
        public bool CanJump;
    }

    /// <summary>
    /// A rook.
    /// </summary>
    public class RookPiece : Piece
    {

        /// <summary>
        /// Can this rook castle eventually?
        /// </summary>
        public bool CanCastle;
    }

    /// <summary>
    /// A queen.
    /// </summary>
    public class QueenPiece : Piece
    {

    }

    /// <summary>
    /// A king.
    /// </summary>
    public class KingPiece : Piece
    {

        /// <summary>
        /// Can this king castle eventually?
        /// </summary>
        public bool CanCastle;
    }

    /// <summary>
    /// A knight.
    /// </summary>
    public class KnightPiece : Piece
    {

    }

    /// <summary>
    /// A bishop.
    /// </summary>
    public class BishopPiece : Piece
    {

    }
}