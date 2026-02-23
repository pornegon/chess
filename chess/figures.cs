using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using static System.ComponentModel.Design.ObjectSelectorEditor;


namespace chess
{
    public abstract class Figure(int col, int row, Color color, Chessboard board)
    {
        public int Row { get; set; } = row;
        public int Col { get; set; } = col;
        public Color Color { get; set; } = color;
        public char Icon { get; set; }
        public bool HasMoved { get; set; } = false;
        public bool canMove { get; set; } = true;

        public Chessboard Board = board;

        public abstract bool CouldMove(int tarCol, int tarRow);

        public abstract bool ValidMove(int tarCol, int tarRow);


        public void Move(int tarCol, int tarRow, out bool Castle, out bool Promotion)
        {
            Castle = false;
            Promotion = false;
            if (!ValidMove(tarCol, tarRow)) return;


            if (this is King && Math.Abs(this.Col - tarCol) == 2)
            {
                int rookCol = tarCol > Col ? 7 : 0;
                int newCol = tarCol > Col ? Col + 1 : Col - 1;
                Figure ToMove = Board.GetFig(rookCol, tarRow)!;
                Castle = true;

                Board.Move(ToMove, newCol, Row);
            }

            Board.Move(this, tarCol, tarRow);

            if (this is Pawn && tarRow == (((Pawn)this).dir == 1 ? 7 : 0))
                Promotion = true;
        }

        public bool IsntBlocked(int tarCol, int tarRow)
        {
            var colDiff = Math.Sign(tarCol - Col);
            var rowDiff = Math.Sign(tarRow - Row);

            var curCol = Col + colDiff;
            var curRow = Row + rowDiff;

            while (curCol != tarCol || curRow != tarRow)
            {
                if (!Board.EmptySquare(curCol, curRow))
                    return false;
                curCol += colDiff;
                curRow += rowDiff;
            }
            return true;
        }
        public bool ClearsCheck(int tarCol, int tarRow)
        {
            bool clears = false;

            var cacheCol = this.Col;
            var cacheRow = this.Row;
            var cacheMoved = HasMoved;
            var cacheFig = Board.GetFig(tarCol, tarRow);


            Color tar = Color == Color.White ? Color.Black : Color.White;


            Figure kang = Board.FiguresByColor[Color].First(fig => fig is King);

            if (kang.IsChecked(kang.Col, kang.Row) == null && IsChecked(this.Col, this.Row) == null)
            //   || !Board.AllValidMoves[tar].Any(a => a.Value.Contains((col, row)) && !new[] { typeof(Knight), typeof(Pawn) }.Contains(a.Key.GetType())))
                return true;
            else
            {
                Board.Move(this, tarCol, tarRow, this.HasMoved);
                if (kang.IsChecked(kang.Col, kang.Row) == null)
                    clears = true;
                Board.Move(this, cacheCol, cacheRow, cacheMoved, cacheFig);
            }
            return clears;


        }

        public bool CanMove()
        {
            if (this.IsChecked(this.Col, this.Row) is null or Pawn or Knight)
                return true;
            
            var moves = this.GetValidMoves();

            if (!moves.Any()) return true;

            else return ClearsCheck(moves[0].col, moves[0].row);
        }

        public List<(int col, int row)> GetValidMoves()
        {
            List<(int col, int row)> ValidMoves = new();

            if (!this.canMove) return ValidMoves;

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (ValidMove(j, i)) ValidMoves.Add((j, i));
                }
            }
            return ValidMoves;
        }

        public Figure? IsChecked(int tarCol, int tarRow)
        {
            Color tar = Color == Color.White ? Color.Black : Color.White;

            foreach (Figure fig in Board.FiguresByColor[tar])
                if ((fig is Knight && fig.CouldMove(tarCol, tarRow)) || (fig.CouldMove(tarCol, tarRow) && fig.IsntBlocked(tarCol, tarRow)))
                    return fig;

            return null;
        }

        public bool IsThreatened()
        {
            Color tar = this.Color == Color.White ? Color.Black : Color.White;
            return Board.AllValidMoves[tar].Any(a => a.Value.Contains((this.Col, this.Row)));
        }
    }

    public class Pawn : Figure
    {
        public int dir;
        public Pawn(int col, int row, Color color, Chessboard board) : base(col, row, color, board)
        {
            this.Icon = color == Color.White ? '♙' : '♟';
            this.dir = color == Color.White ? 1 : -1;
        }
        public override bool CouldMove(int tarCol, int tarRow) => Math.Abs(tarCol - Col) == 1 && tarRow - Row == dir;

        public bool enPassant(int tarCol, int tarRow) =>
                    (tarRow - Row == 2 * dir && !HasMoved
                    && Board.EmptySquare(tarCol, tarRow)
                    && Board.EmptySquare(tarCol, tarRow - dir));

        public override bool ValidMove(int tarCol, int tarRow)
        {

            if (tarCol == Col)
            {
                if (tarRow - Row == dir && Board.EmptySquare(tarCol, tarRow) || enPassant(tarCol, tarRow))
                    return ClearsCheck(tarCol, tarRow);

                else return false;
            }
            else return CouldMove(tarCol, tarRow) && (Board.IsFoe(this, tarCol, tarRow) || Board.EnPassant(this, tarCol, tarRow)) && ClearsCheck(tarCol, tarRow);
        }
    }
    public class Bishop : Figure
    {
        public Bishop(int col, int row, Color color, Chessboard board) : base(col, row, color, board)
        {
            this.Icon = color == Color.White ? '♗' : '♝';
        }

        public override bool CouldMove(int tarCol, int tarRow) => Math.Abs(tarCol - Col) == Math.Abs(tarRow - Row) && tarRow != Row;

        public override bool ValidMove(int tarCol, int tarRow)
        {
            return CouldMove(tarCol, tarRow) && Board.CanLand(this, tarCol, tarRow);
        }
    }
    public class Knight : Figure
    {
        public Knight(int col, int row, Color color, Chessboard board) : base(col, row, color, board)
        {
            this.Icon = color == Color.White ? '♘' : '♞';
        }

        public override bool CouldMove(int tarCol, int tarRow)
            => Math.Abs(tarCol - Col) == 1 && Math.Abs(tarRow - Row) == 2 || Math.Abs(tarCol - Col) == 2 && Math.Abs(tarRow - Row) == 1;

        public override bool ValidMove(int tarCol, int tarRow)
        {
            return CouldMove(tarCol, tarRow)
                && Board.CanLand(this, tarCol, tarRow); 
        }
    }
    public class Rook : Figure
    {
        public Rook(int col, int row, Color color, Chessboard board) : base(col, row, color, board)
        {
            this.Icon = color == Color.White ? '♖' : '♜';
        }
        public override bool CouldMove(int tarCol, int tarRow) => tarCol == Col ^ tarRow == Row;

        public override bool ValidMove(int tarCol, int tarRow)
        {
            return CouldMove(tarCol, tarRow) && Board.CanLand(this, tarCol, tarRow); 
        }
    }
    public class Queen : Figure
    {
        public Queen(int col, int row, Color color, Chessboard board) : base(col, row, color, board)
        {
            this.Icon = color == Color.White ? '♕' : '♛';
        }

        public override bool CouldMove(int tarCol, int tarRow) => (Math.Abs(tarCol - Col) == Math.Abs(tarRow - Row) && tarRow != Row) || (tarCol == Col ^ tarRow == Row);

        public override bool ValidMove(int tarCol, int tarRow)
        {
            return CouldMove(tarCol, tarRow) && Board.CanLand(this, tarCol, tarRow);
        }
    }
    public class King : Figure
    {
        public King(int col, int row, Color color, Chessboard board) : base(col, row, color, board)
        {
            this.Icon = color == Color.White ? '♔' : '♚';
        }
        public override bool CouldMove(int tarCol, int tarRow)
        {
            var colDiff = Math.Abs(tarCol - Col);
            var rowDiff = Math.Abs(tarRow - Row);
            return colDiff <= 1 && rowDiff <= 1 && !(colDiff == 0 && rowDiff == 0);
        }

        public override bool ValidMove(int tarCol, int tarRow)
        {
            var colDiff = Math.Abs(tarCol - Col);
            var rowDiff = Math.Abs(tarRow - Row);
            return CouldMove(tarCol, tarRow) && Board.CanLand(this, tarCol, tarRow) || (colDiff == 2 && Castle(tarCol, tarRow));
        }

        public bool Castle(int tarCol, int tarRow)
        {
            if (HasMoved || tarRow != Row || IsChecked(Col, Row) != null) return false;

            int rookCol = tarCol > Col ? 7 : 0; 
            var passing = tarCol > Col ? Enumerable.Range(Col, 3) : Enumerable.Range(tarCol, 3);
            var a = Board.GetFig(rookCol, Row);

            return a is Rook && a.Color == Color && !a.HasMoved && a.IsntBlocked(Col, Row) && passing.All(a => IsChecked(a, Row) == null);
        }

    }
}
