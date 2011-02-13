using System;
using System.Collections.Generic;

namespace XChess
{
    /// <summary>
    /// Represents the state of one square on the board. Can be replaced with null to indicate the square
    /// is empty.
    /// </summary>
    public abstract class Piece
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
        /// Gets the next state of this piece if it doesn't move.
        /// </summary>
        public virtual Piece NextIdleState
        {
            get
            {
                return this;
            }
        }

        /// <summary>
        /// Gets the avaiable moves for this piece if it as on the specified position on the board.
        /// </summary>
        public virtual IEnumerable<PieceMove> GetMoves(Board Board, Square Position)
        {
            return new PieceMove[0];
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
        public override Piece NextIdleState
        {
            get
            {
                if (this.EnPassantThreat)
                {
                    return new PawnPiece()
                    {
                        CanJump = this.CanJump,
                        EnPassantThreat = false,
                        Player = this.Player
                    };
                }
                return this;
            }
        }

        /// <summary>
        /// Gets the state of the pawn after a normal (non-jump) move.
        /// </summary>
        public PawnPiece AfterMove
        {
            get
            {
                return new PawnPiece()
                {
                    CanJump = false,
                    EnPassantThreat = false,
                    Player = this.Player
                };
            }
        }

        public override IEnumerable<PieceMove> GetMoves(Board Board, Square Position)
        {
            int movedir = this.Player == 0 ? 1 : -1;

            // Move one square up
            Square front = Position.Offset(movedir, 0);
            if (Board.InBoard(front) && Board.GetPiece(front) == null)
            {
                yield return PieceMove.Create(Position, front, this.AfterMove);
            }

            // Attack
            foreach (Square attacksquare in new Square[]
                {
                    Position.Offset(movedir, 1),
                    Position.Offset(movedir, -1)
                })
            {
                if (Board.InBoard(attacksquare))
                {
                    Piece target = Board.GetPiece(attacksquare);
                    if (target != null && target.Player != this.Player)
                    {
                        yield return PieceMove.Create(Position, attacksquare, this.AfterMove);
                    }

                    // En passant
                    if (target == null)
                    {
                        Square behindattacksquare = attacksquare.Offset(-movedir, 0);
                        if (Board.InBoard(behindattacksquare))
                        {
                            PawnPiece pawntarget = Board.GetPiece(behindattacksquare) as PawnPiece;
                            if (pawntarget != null && pawntarget.EnPassantThreat)
                            {
                                yield return new EnPassantMove()
                                {
                                    Source = Position,
                                    Destination = attacksquare,
                                    Captured = behindattacksquare,
                                    NewState = this.AfterMove
                                };
                            }
                        }
                    }
                }
            }

            // Jump
            if (this.CanJump)
            {
                Square jumpsquare = Position.Offset(movedir * 2, 0);
                if (Board.InBoard(jumpsquare) && Board.GetPiece(jumpsquare) == null)
                {
                    yield return PieceMove.Create(Position, jumpsquare, new PawnPiece()
                    {
                        CanJump = false,
                        EnPassantThreat = true,
                        Player = this.Player
                    });
                }
            }
        }

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