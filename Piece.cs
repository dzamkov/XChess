using System;
using System.Collections.Generic;

namespace XChess
{
    /// <summary>
    /// Represents the state of one square on the board. Can be replaced with null to indicate the square
    /// is empty.
    /// </summary>
    public abstract class Piece : IEquatable<Piece>
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
        public virtual IEnumerable<Move> GetMoves(Board Board, Square Position)
        {
            foreach (Square s in this.GetThreats(Board, Position))
            {
                if (this.CanMove(Board, s))
                {
                    yield return PieceMove.Create(Position, s, this.NextIdleState);
                }
            }
        }

        /// <summary>
        /// Gets the squares this piece is attacking. (That is, if a piece is on one of the squares, it can be taken).
        /// </summary>
        public virtual IEnumerable<Square> GetThreats(Board Board, Square Position)
        {
            return new Square[0];
        }

        /// <summary>
        /// Gets the threats a piece with the specified parameters can make along the given ray defined by its delta and current offsets.
        /// </summary>
        public static IEnumerable<Square> GetRayThreats(int Player, int DR, int DF, int CR, int CF, Board Board, Square Position)
        {
            while (true)
            {
                Square pos = Position.Offset(CR, CF);
                if (Board.InBoard(pos))
                {
                    Piece piece = Board.GetPiece(pos);
                    if (piece == null)
                    {
                        CR += DR;
                        CF += DF;
                        yield return pos;
                        continue;
                    }
                    yield return pos;
                    break;
                }
                break;
            }
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
        /// Gets the value of this piece in relation to a pawn.
        /// </summary>
        public virtual double Value
        {
            get
            {
                return 1.0;
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
        /// Gets if this piece is equivalent to another.
        /// </summary>
        public abstract bool Equals(Piece Other);

        public override bool Equals(object obj)
        {
            Piece p = obj as Piece;
            if (p != null)
            {
                return this.Equals(p);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return 0x00000000;
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

        public override double Value
        {
            get
            {
                return 1.0;
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

        public override IEnumerable<Move> GetMoves(Board Board, Square Position)
        {
            List<Move> moves = new List<Move>();
            int movedir = this.Player == 0 ? 1 : -1;
            int lastrank = this.Player == 0 ? Board.Ranks - 1 : 0;

            // Move one square up
            bool jumpclear = false;
            Square front = Position.Offset(movedir, 0);
            if (Board.InBoard(front) && Board.GetPiece(front) == null)
            {
                jumpclear = true;
                moves.AddRange(_GetActualMoves(Board, PieceMove.Create(Position, front, this.AfterMove), lastrank));
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
                        moves.AddRange(_GetActualMoves(Board, PieceMove.Create(Position, attacksquare, this.AfterMove), lastrank));
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
                                moves.AddRange(_GetActualMoves(Board, new EnPassantMove()
                                {
                                    Source = Position,
                                    Destination = attacksquare,
                                    Captured = behindattacksquare,
                                    NewState = this.AfterMove
                                }, lastrank));
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
                    moves.AddRange(_GetActualMoves(Board, PieceMove.Create(Position, jumpsquare, new PawnPiece()
                    {
                        CanJump = false,
                        EnPassantThreat = true,
                        Player = this.Player
                    }), lastrank));
                }
            }

            return moves;
        }

        private static IEnumerable<Move> _GetActualMoves(Board Board, PieceMove Original, int LastRank)
        {
            // If destination square is on last rank... promte
            if (Original.Destination.Rank == LastRank)
            {
                Piece newstate = Original.NewState;
                EnPassantMove epm = Original as EnPassantMove;
                foreach (Piece possible in new Piece[]
                {
                    new QueenPiece() { Player = newstate.Player },
                    new RookPiece() { Player = newstate.Player, CanCastle = false },
                    new KnightPiece() { Player = newstate.Player },
                    new BishopPiece() { Player = newstate.Player }
                })
                {
                    if (epm != null)
                    {
                        yield return new EnPassantMove()
                        {
                            Captured = epm.Captured,
                            Destination = Original.Destination,
                            NewState = possible,
                            Source = Original.Source
                        };
                    }
                    else
                    {
                        yield return new PieceMove()
                        {
                            Destination = Original.Destination,
                            NewState = possible,
                            Source = Original.Source
                        };
                    }
                }
            }
            else
            {
                yield return Original;
            }
        }

        public override IEnumerable<Square> GetThreats(Board Board, Square Position)
        {
            int movedir = this.Player == 0 ? 1 : -1;
            return new Square[]
            {
                Position.Offset(movedir, 1),
                Position.Offset(movedir, -1)
            };
        }

        public override bool Equals(Piece Other)
        {
            PawnPiece p = Other as PawnPiece;
            if (p != null)
            {
                return p.Player == this.Player && p.EnPassantThreat == this.EnPassantThreat && p.CanJump == this.CanJump;
            }
            return false;
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
        public override IEnumerable<Move> GetMoves(Board Board, Square Position)
        {
            foreach (Square s in this.GetThreats(Board, Position))
            {
                if (this.CanMove(Board, s))
                {
                    yield return PieceMove.Create(Position, s, new RookPiece() { Player = this.Player, CanCastle = false });
                }
            }
        }

        public override IEnumerable<Square> GetThreats(Board Board, Square Position)
        {
            int dx = 1;
            int dy = 0;
            for (int t = 0; t < 4; t++)
            {
                foreach (Square threat in GetRayThreats(this.Player, dx, dy, dx, dy, Board, Position))
                {
                    yield return threat;
                }
                int temp = dy;
                dy = -dx;
                dx = temp;
            }
        }

        public override bool Equals(Piece Other)
        {
            RookPiece p = Other as RookPiece;
            if (p != null)
            {
                return p.Player == this.Player && p.CanCastle == this.CanCastle;
            }
            return false;
        }

        public override double Value
        {
            get
            {
                return 5.0;
            }
        }

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
        public override IEnumerable<Square> GetThreats(Board Board, Square Position)
        {
            for (int r = -1; r < 2; r++)
            {
                for (int f = -1; f < 2; f++)
                {
                    if (r != 0 || f != 0)
                    {
                        foreach (Square threat in GetRayThreats(this.Player, r, f, r, f, Board, Position))
                        {
                            yield return threat;
                        }
                    }
                }
            }
        }

        public override double Value
        {
            get
            {
                return 9.0;
            }
        }

        public override bool Equals(Piece Other)
        {
            QueenPiece p = Other as QueenPiece;
            if (p != null)
            {
                return p.Player == this.Player;
            }
            return false;
        }

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
        public override IEnumerable<Move> GetMoves(Board Board, Square Position)
        {
            foreach (Square s in this.GetThreats(Board, Position))
            {
                if (this.CanMove(Board, s))
                {
                    yield return PieceMove.Create(Position, s, new KingPiece() { Player = this.Player, CanCastle = false });
                }
            }

            // Maybe castle maybe?
            int oplayer = 1 - this.Player;
            if (this.CanCastle && !Board.HasThreat(oplayer, Position))
            {
                CastleMove lc = _TryCastle(Board, Position, Player, -1);
                CastleMove rc = _TryCastle(Board, Position, Player, 1);
                if (lc != null) yield return lc;
                if (rc != null) yield return rc;
            }
        }

        private static CastleMove _TryCastle(Board Board, Square Position, int Player, int DF)
        {
            int oplayer = 1 - Player;
            Square rookdest = Position.Offset(0, DF);
            if(Board.InBoard(rookdest) && Board.GetPiece(rookdest) == null && !Board.HasThreat(oplayer, rookdest))
            {
                int cf = DF + DF;
                while (true)
                {
                    Square test = Position.Offset(0, cf);
                    if (!Board.InBoard(test))
                    {
                        return null;
                    }
                    Piece piece = Board.GetPiece(test);
                    if (piece != null)
                    {
                        RookPiece rook = piece as RookPiece;
                        if (rook != null && rook.Player == Player && rook.CanCastle)
                        {
                            return new CastleMove()
                            {
                                KingSource = Position,
                                KingDestination = Position.Offset(0, DF + DF),
                                RookSource = test,
                                RookDestination = rookdest,
                                NewKingState = new KingPiece()
                                {
                                    Player = Player,
                                    CanCastle = false
                                },
                                NewRookState = new RookPiece()
                                {
                                    Player = Player,
                                    CanCastle = false
                                }
                            };
                        }
                        else
                        {
                            return null;
                        }
                    }
                    cf += DF;
                }
            }
            return null;
        }

        public override IEnumerable<Square> GetThreats(Board Board, Square Position)
        {
            for (int r = -1; r < 2; r++)
            {
                for (int f = -1; f < 2; f++)
                {
                    if (r != 0 || f != 0)
                    {
                        yield return Position.Offset(r, f);
                    }
                }
            }
        }

        public override double Value
        {
            get
            {
                return 5.0;
            }
        }

        public override bool Equals(Piece Other)
        {
            KingPiece p = Other as KingPiece;
            if (p != null)
            {
                return p.Player == this.Player && p.CanCastle == this.CanCastle;
            }
            return false;
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
        public override IEnumerable<Square> GetThreats(Board Board, Square Position)
        {
            return new Square[]
            {
                Position.Offset(2, 1),
                Position.Offset(2, -1),
                Position.Offset(-2, 1),
                Position.Offset(-2, -1),
                Position.Offset(1, 2),
                Position.Offset(1, -2),
                Position.Offset(-1, 2),
                Position.Offset(-1, -2),
            };
        }

        public override double Value
        {
            get
            {
                return 3.0;
            }
        }

        public override bool Equals(Piece Other)
        {
            KnightPiece p = Other as KnightPiece;
            if (p != null)
            {
                return p.Player == this.Player;
            }
            return false;
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
        public override IEnumerable<Square> GetThreats(Board Board, Square Position)
        {
            for (int t = 0; t < 4; t++)
            {
                int dx = t < 2 ? -1 : 1;
                int dy = (t % 2) < 1 ? -1 : 1;
                foreach (Square threat in GetRayThreats(this.Player, dx, dy, dx, dy, Board, Position))
                {
                    yield return threat;
                }
            }
        }

        public override bool Equals(Piece Other)
        {
            BishopPiece p = Other as BishopPiece;
            if (p != null)
            {
                return p.Player == this.Player;
            }
            return false;
        }

        public override double Value
        {
            get
            {
                return 3.0;
            }
        }

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