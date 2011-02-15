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
        /// Gets the mesh used to display this piece.
        /// </summary>
        public virtual Mesh DisplayMesh
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets if this piece (which moves and attacks normally) can move to or attack the specified tile.
        /// </summary>
        public bool CanMove(Board Board, Square Position)
        {
            if (Board.InBoard(Position))
            {
                Piece piece = Board.GetPiece(Position);
                if (piece == null || piece.Player != this.Player)
                {
                    return true;
                }
            }
            return false;
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
            bool jumpclear = false;
            Square front = Position.Offset(movedir, 0);
            if (Board.InBoard(front) && Board.GetPiece(front) == null)
            {
                jumpclear = true;
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
            if (this.CanJump && jumpclear)
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

        public override Mesh DisplayMesh
        {
            get
            {
                return Mesh;
            }
        }

        public static readonly Mesh Mesh = Mesh.LoadOBJ(Path.Resources["Models"]["Pawn.obj"]);
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

        public override Mesh DisplayMesh
        {
            get
            {
                return Mesh;
            }
        }

        public static readonly Mesh Mesh = Mesh.LoadOBJ(Path.Resources["Models"]["Rook.obj"]);
    }

    /// <summary>
    /// A queen.
    /// </summary>
    public class QueenPiece : Piece
    {

        public override Mesh DisplayMesh
        {
            get
            {
                return Mesh;
            }
        }

        public static readonly Mesh Mesh = Mesh.LoadOBJ(Path.Resources["Models"]["Queen.obj"]);
    }

    /// <summary>
    /// A king.
    /// </summary>
    public class KingPiece : Piece
    {
        public override IEnumerable<PieceMove> GetMoves(Board Board, Square Position)
        {
            for (int r = -1; r < 2; r++)
            {
                for (int f = -1; f < 2; f++)
                {
                    if (r != 0 || f != 0)
                    {
                        Square pos = Position.Offset(r, f);
                        if (this.CanMove(Board, pos))
                        {
                            yield return PieceMove.Create(Position, pos, this);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Can this king castle eventually?
        /// </summary>
        public bool CanCastle;

        public override Mesh DisplayMesh
        {
            get
            {
                return Mesh;
            }
        }

        public static readonly Mesh Mesh = Mesh.LoadOBJ(Path.Resources["Models"]["King.obj"]);
    }

    /// <summary>
    /// A knight.
    /// </summary>
    public class KnightPiece : Piece
    {
        public override IEnumerable<PieceMove> GetMoves(Board Board, Square Position)
        {
            int[] movetable = new int[]
            {
                2, -1,
                2, 1,
                -2, -1,
                -2, 1,
                1, -2,
                1, 2,
                -1, -2,
                -1, 2
            };
            for (int t = 0; t < 8; t++)
            {
                Square pos = Position.Offset(movetable[t * 2 + 0], movetable[t * 2 + 1]);
                if (this.CanMove(Board, pos))
                {
                    yield return PieceMove.Create(Position, pos, this);
                }
            }
        }

        public override Mesh DisplayMesh
        {
            get
            {
                return Mesh;
            }
        }

        public static readonly Mesh Mesh = Mesh.LoadOBJ(Path.Resources["Models"]["Knight.obj"]);
    }

    /// <summary>
    /// A bishop.
    /// </summary>
    public class BishopPiece : Piece
    {

        public override Mesh DisplayMesh
        {
            get
            {
                return Mesh;
            }
        }

        public static readonly Mesh Mesh = Mesh.LoadOBJ(Path.Resources["Models"]["Bishop.obj"]);
    }
}