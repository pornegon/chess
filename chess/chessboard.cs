using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Text;

namespace chess
{
    public class Chessboard
    {
        private string[] visuBoard = new string[8]
            {
            "♜♞♝♛♚♝♞♜",
            "♟♟♟♟♟♟♙♟", 
            "        ", 
            "        ",
            "        ", 
            "        ", 
            "♙♙♙♙♙♙♙♙",
            "♖♘♗♕♔♗♘♖"
            };
        public string visuString = "♙♗♘♖♕♔";
        public string visuString2 ="♟♝♞♜♛♚";
        public string letterString = " BNRQK";
        private Figure?[,] board = new Figure[8, 8];
        public Dictionary<Color, List<Figure>> FiguresByColor = new() { [Color.White] = new(), [Color.Black] = new() };

        public Dictionary<Color, Dictionary<Figure, List<(int col, int row)>>> AllValidMoves = new () { [Color.White] = new (), [Color.Black] = new () };

        (int col, int row)? enPassant;
        Pawn? cachePawn;

        public Chessboard()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    char icon = visuBoard[i][j];
                    int I = 7 - i;
                    Color color = visuString.Contains(icon) ? Color.White : Color.Black;
                    board[j, I] = Ordain(j, I, color, icon);
                    
                    if (board[j, I] != null)
                        FiguresByColor[color].Add(board[j, I]!);
                }
            }

            GetAllValidMoves(Color.White);
        }
        public Figure? Ordain(int j, int I, Color color, char icon) => icon switch
        {
            '♙' or '♟' => new Pawn(j, I, color, this),
            '♖' or '♜' => new Rook(j, I, color, this),
            '♘' or '♞' => new Knight(j, I, color, this),
            '♗' or '♝' => new Bishop(j, I, color, this),
            '♕' or '♛' => new Queen(j, I, color, this),
            '♔' or '♚' => new King(j, I, color, this),
            _ => null
        };

        public void Promote(Figure fig, char chosen)
        {

            Color opposing = fig.Color == Color.White ? Color.Black : Color.White;

            FiguresByColor[fig.Color].Remove(fig);
            AllValidMoves[fig.Color].Remove(fig);


            fig = Ordain(fig.Col, fig.Row, fig.Color, chosen);


            board[fig.Col, fig.Row] = fig;

            FiguresByColor[fig.Color].Add(fig);
            AllValidMoves[fig.Color][fig] = fig.GetValidMoves();

            GetAllValidMoves(opposing);

        }

        public Figure? GetFig(int col, int row) => board[col, row];


        public bool EmptySquare(int col, int row) => board[col, row] == null;

        public bool IsFoe(Figure fig, int col, int row) => board[col, row] != null ? board[col, row]!.Color != fig.Color : false;

        public bool EnPassant(Figure fig, int col, int row) => fig is Pawn && cachePawn is not null && cachePawn.Color != fig.Color && enPassant == (col, row);

        public bool CanLand(Figure fig, int col, int row) => fig switch
        {
            Knight => (EmptySquare(col, row) || IsFoe(fig, col, row)) && fig.ClearsCheck(col, row),
            King => (EmptySquare(col, row) || IsFoe(fig, col, row)) && fig.IsChecked(col, row) == null && fig.ClearsCheck(col, row),
            _ => (EmptySquare(col, row) || IsFoe(fig, col, row)) && fig.IsntBlocked(col, row) && fig.ClearsCheck(col, row)
        };


        public void Move(Figure piece, int col, int row, bool? cacheMoved = null, Figure? cacheFig = null)
        {
            int oldcol = piece.Col;
            int oldrow = piece.Row;

            Color opposing = piece.Color == Color.White ? Color.Black : Color.White;

            if (IsFoe(piece, col, row))
                FiguresByColor[opposing].Remove(GetFig(col, row)!);

            else if (cacheFig != null)
                FiguresByColor[opposing].Add(cacheFig);

            if (cacheMoved == null)
            {
                if (IsFoe(piece, col, row))
                    AllValidMoves[opposing].Remove(GetFig(col, row)!);
                if (EnPassant(piece, col, row))
                {
                    FiguresByColor[opposing].Remove(cachePawn);
                    AllValidMoves[opposing].Remove(cachePawn);
                    board[cachePawn.Col, cachePawn.Row] = null;
                }
                else if (cachePawn?.Color == piece.Color) 
                {
                    cachePawn = null;
                    enPassant = null;
                }

                if (piece is Pawn && ((Pawn)piece).enPassant(col, row))
                {
                    cachePawn = (Pawn)piece;
                    enPassant = (col, row - cachePawn.dir);
                }
            }

            board[oldcol, oldrow] = cacheFig == null ? null : cacheFig;

            board[col, row] = piece;

            piece.Col = col;
            piece.Row = row;
            piece.HasMoved = cacheMoved != null ? (bool)cacheMoved : true;

            if (cacheMoved == null)
                GetAllValidMoves(opposing);
        }

        public void GetAllValidMoves(Color color)
        {
            foreach (Figure fig in FiguresByColor[color])
            {
                fig.canMove = fig.CanMove();
                AllValidMoves[color][fig] = fig.GetValidMoves();
            }
        }
    }
}
