﻿using System;
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
        /// Gets all valid moves from this position.
        /// </summary>
        public IEnumerable<Move> Moves
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
                            foreach (PieceMove piecemove in piece.GetMoves(this, pos))
                            {
                                yield return piecemove;
                            }
                        }
                    }
                }
            }
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