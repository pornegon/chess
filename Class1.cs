using System;

namespace chess
{
    public class Figure((int, int) pos, Color color)
    {
    }

    public class Pawn : Figure
    {
        public Color color;
        public string icon;
        public Pawn((int row, int col) pos, Color color) : base(pos, color)
        {
            this.icon = color == Color.White ? "♙" : "♟";
        }
    }
}
