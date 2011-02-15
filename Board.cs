using System;
using System.Collections.Generic;

namespace XChess
{
    /// <summary>
    /// The state of the chess board at any one time. Can be used to create hypothetical boards too.
    /// </summary>
    public class Board
    {
        public Board(int Ranks, int Files)
        {
            this.Pieces = new Piece[Files, Ranks];
        }

        /// <summary>
        /// Gets the initial state of the board in a game of chess.
        /// </summary>
        public static Board Initial
        {
            get
            {
                Board board = new Board(8, 8);
                Piece[] whiterank = new Piece[] { Piece.Rook, Piece.Knight, Piece.Bishop, Piece.Queen, Piece.King, Piece.Bishop, Piece.Knight, Piece.Rook };
                Piece[] blackrank = new Piece[] { Piece.Rook, Piece.Knight, Piece.Bishop, Piece.Queen, Piece.King, Piece.Bishop, Piece.Knight, Piece.Rook };
                Piece whitepawn = Piece.Pawn;
                Piece blackpawn = Piece.Pawn;

                for (int t = 0; t < blackrank.Length; t++)
                {
                    blackrank[t].Player = 1;
                }
                blackpawn.Player = 1;

                for (int t = 0; t < 8; t++)
                {
                    board.SetPiece(new Square(0, t), whiterank[t]);
                    board.SetPiece(new Square(1, t), whitepawn);
                    board.SetPiece(new Square(6, t), blackpawn);
                    board.SetPiece(new Square(7, t), blackrank[t]);
                }
                return board;
            }
        }

        /// <summary>
        /// Gets the amount of ranks this board has.
        /// </summary>
        public int Ranks
        {
            get
            {
                return this.Pieces.GetLength(1);
            }
        }

        /// <summary>
        /// Gets the amount of files this board has.
        /// </summary>
        public int Files
        {
            get
            {
                return this.Pieces.GetLength(0);
            }
        }

        /// <summary>
        /// Gets if the king of the current player is in check.
        /// </summary>
        public bool Check
        {
            get
            {
                return this.HasThreat(1 - this.PlayerToMove, this.GetKing(this.PlayerToMove));
            }
        }

        /// <summary>
        /// Gets the piece at the specified square.
        /// </summary>
        public Piece GetPiece(Square Square)
        {
            return this.Pieces[Square.File, Square.Rank];
        }

        /// <summary>
        /// Sets the piece at the specified square.
        /// </summary>
        public void SetPiece(Square Square, Piece Piece)
        {
            this.Pieces[Square.File, Square.Rank] = Piece;
        }

        /// <summary>
        /// Gets if the specified square is in the range of the board.
        /// </summary>
        public bool InBoard(Square Square)
        {
            return Square.File >= 0 && Square.Rank >= 0 && Square.File < this.Files && Square.Rank < this.Ranks;
        }

        /// <summary>
        /// Gets all valid moves (and the corresponding boards they produce) from this board.
        /// </summary>
        public IEnumerable<KeyValuePair<Move, Board>> Moves
        {
            get
            {
                for (int r = 0; r < this.Ranks; r++)
                {
                    for (int f = 0; f < this.Files; f++)
                    {
                        Square pos = new Square(r, f);

                        // Piece here?
                        Piece piece = this.GetPiece(pos);
                        if (piece != null && piece.Player == this.PlayerToMove)
                        {
                            foreach (Move piecemove in piece.GetMoves(this, pos))
                            {
                                Board next = this.GetNext(piecemove);
                                if (!next.HasThreat(1 - this.PlayerToMove, next.GetKing(this.PlayerToMove)))
                                {
                                    yield return new KeyValuePair<Move, Board>(piecemove, next);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets if the specified player has a piece that threatens the given position.
        /// </summary>
        public bool HasThreat(int Player, Square Position)
        {
            for (int r = 0; r < this.Ranks; r++)
            {
                for (int f = 0; f < this.Files; f++)
                {
                    Square square = new Square(r, f);
                    Piece piece = this.GetPiece(square);
                    if (piece != null && piece.Player == Player)
                    {
                        foreach (Square threat in piece.GetThreats(this, square))
                        {
                            if (threat == Position)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the position of the king for the specified player.
        /// </summary>
        public Square GetKing(int Player)
        {
            for (int r = 0; r < this.Ranks; r++)
            {
                for (int f = 0; f < this.Files; f++)
                {
                    Square square = new Square(r, f);
                    KingPiece piece = this.GetPiece(square) as KingPiece;
                    if (piece != null && piece.Player == Player)
                    {
                        return square;
                    }
                }
            }
            return new Square(0, 0);
        }

        /// <summary>
        /// Gets the immediate score for the specified player.
        /// </summary>
        public double GetScore(int Player)
        {
            if (this.Moves.GetEnumerator().MoveNext() == false)
            {
                // No moves left!
                if(this.Check)
                {
                    if(this.PlayerToMove == Player)
                    {
                        return double.NegativeInfinity;
                    }
                    else
                    {
                        return double.PositiveInfinity;
                    }
                }
                return 0.0;
            }
            return this.GetScoreNormal(Player);
        }

        /// <summary>
        /// Gets the immediate score for the specified player assuming that the game is not over.
        /// </summary>
        public double GetScoreNormal(int Player)
        {
            double total = 0.0;
            for (int r = 0; r < this.Ranks; r++)
            {
                for (int f = 0; f < this.Files; f++)
                {
                    Square square = new Square(r, f);
                    Piece piece = this.GetPiece(square);
                    if (piece != null)
                    {
                        double value = piece.Value;
                        double spotmultiplier = value * 0.025 + 0.05;
                        double piecemultiplier = piece.Player == Player ? 1.0 : -1.0;
                        value *= piecemultiplier;
                        spotmultiplier *= piecemultiplier;
                        total += value;
                        foreach (Square threat in piece.GetThreats(this, square))
                        {
                            total += spotmultiplier;
                        }
                    }
                }
            }
            return total;
        }

        /// <summary>
        /// Gets the next state of the board if the given move (assumed to be valid) is played.
        /// </summary>
        public Board GetNext(Move Move)
        {
            Board nboard = new Board(this.Ranks, this.Files);
            nboard.PlayerToMove = 1 - this.PlayerToMove;

            for (int r = 0; r < this.Ranks; r++)
            {
                for (int f = 0; f < this.Files; f++)
                {
                    Piece cur = this.Pieces[r, f];
                    if (cur != null)
                    {
                        nboard.Pieces[r, f] = cur.NextIdleState;
                    }
                }
            }

            PieceMove pm = Move as PieceMove;
            if (pm != null)
            {
                nboard.SetPiece(pm.Source, null);
                nboard.SetPiece(pm.Destination, pm.NewState);

                EnPassantMove epm = pm as EnPassantMove;
                if (epm != null)
                {
                    nboard.SetPiece(epm.Captured, null);
                }
            }

            CastleMove cm = Move as CastleMove;
            if (cm != null)
            {
                nboard.SetPiece(cm.KingSource, null);
                nboard.SetPiece(cm.RookSource, null);
                nboard.SetPiece(cm.KingDestination, cm.NewKingState);
                nboard.SetPiece(cm.RookDestination, cm.NewRookState);
            }

            return nboard;
        }

        public Piece[,] Pieces;

        /// <summary>
        /// The player to move.
        /// </summary>
        public int PlayerToMove;
    }

    /// <summary>
    /// References a square on the board.
    /// </summary>
    public struct Square : IEquatable<Square>
    {
        public Square(int Rank, int File)
        {
            this.Rank = Rank;
            this.File = File;
        }

        /// <summary>
        /// Gets this square with the specified offset applied.
        /// </summary>
        public Square Offset(int Rank, int File)
        {
            return new Square(this.Rank + Rank, this.File + File);
        }

        /// <summary>
        /// Gets the X coordinate of the square from white's perspective.
        /// </summary>
        public int X
        {
            get
            {
                return this.File;
            }
        }

        /// <summary>
        /// Gets the Y coordinate of the square from white's perspective.
        /// </summary>
        public int Y
        {
            get
            {
                return this.Rank;
            }
        }

        /// <summary>
        /// Gets the name of this square.
        /// </summary>
        public string Name
        {
            get
            {
                return ((char)(this.File + 97)) + (this.Rank + 1).ToString();
            }
        }

        public override bool Equals(object obj)
        {
            Square? sqr = obj as Square?;
            if (sqr != null)
            {
                return this == sqr.Value;
            }
            return false;
        }

        public bool Equals(Square Square)
        {
            return this == Square;
        }

        public override int GetHashCode()
        {
            return this.Rank ^ (this.File + int.MinValue);
        }

        public static bool operator ==(Square A, Square B)
        {
            return A.Rank == B.Rank && A.File == B.File;
        }

        public static bool operator !=(Square A, Square B)
        {
            return A.Rank != B.Rank || A.File != B.File;
        }

        /// <summary>
        /// Gets the rank of the square.
        /// </summary>
        public int Rank;

        /// <summary>
        /// Gets the file of the square.
        /// </summary>
        public int File;
    }
}