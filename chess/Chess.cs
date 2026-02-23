using System.Drawing;

namespace chess
{
    public partial class Chess : Form
    {
        Chessboard boardModel;
        public Button[,] board;
        public Figure selected;
        public Color player;
        Label turn;
        TextBox moves;
        int moveCount = 0;
        Button re;
        Panel Promotes;
        bool gameState;

        //public List<Figure> Whites = new();
        //public List<Figure> Blacks = new();

        public Chess()
        {
            InitializeComponent();
            Setup();
        }

        void Setup()
        {
            gameState = true;
            boardModel = new();
            player = Color.White;

            Panel boardPanel = new Panel
            {
                Name = "boardPanel",
                Location = new Point(0, 0),
                Size = new Size(800, 800),
            };

            turn = new Label
            {
                Text = $"{player.Name} first",
                Font = new Font("Arial", 18),
                BackColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(850, 50),
                Size = new Size(200, 50),
            };

            moves = new TextBox
            {
                Text = "",
                Font = new Font("Arial", 15),
                BackColor = Color.White,
                Location = new Point(850, 120),
                Multiline = true,
                ReadOnly = true,
                WordWrap = true,
                ScrollBars = ScrollBars.Vertical,
                Size = new Size(200, 600)
            };

            re = new Button
            {
                Text = "Concede",
                Font = new Font("Arial", 20),
                Size = new Size(200, 50),
                BackColor = Color.White,
                Location = new Point(850, 750),
                FlatStyle = FlatStyle.Standard,
            };

            Controls.Add(boardPanel);
            Controls.Add(turn);
            Controls.Add(moves);
            Controls.Add(re);

            re.Click += Re;

            board = new Button[8, 8];

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    var square = new Button
                    {
                        Size = new Size(100, 100),
                        Font = new Font("Segoe UI Symbol", 50),
                        Location = new Point(j * 100, 700 - (i * 100)),
                        Tag = (j, i),
                        BackColor = (j + i) % 2 == 0 ? Color.DimGray : Color.White,
                        FlatStyle = FlatStyle.Flat,
                    };

                    square.Text = boardModel.GetFig(j, i)?.Icon.ToString() ?? "";

                    board[j, i] = square;
                    square.Click += GameClick;
                    boardPanel.Controls.Add(square);

                }
            }

        }

        private void Re(object sender, EventArgs e)
        {
            if (gameState)
            {
                turn.Text = $"{player.Name} concedes!";
                re.Text = "Restart";
                gameState = false;
            }
            else
            {
                Application.Restart();
            }

        }
        private void GameClick(object sender, EventArgs e)
        {
            if (!gameState) return;

            Button square = (Button)sender;
            var (col, row) = ((int, int))square.Tag!;
            // board[col, row].Text = (col +""+ row);
            Figure? piece = boardModel.GetFig(col, row) ?? null;

            ResetHighlight();

            var opp = player == Color.White ? Color.Black : Color.White;


            if (piece != null && piece.Color == player)
            {
                selected = piece;
                board[col, row].BackColor = Color.Teal;
                Highlight(piece);

            }
            else if (selected != null && selected.ValidMove(col, row))
            {
                int oldRow = selected.Row;

                string x = !boardModel.IsFoe(selected, col, row) ? "" : selected is Pawn ? $"{ColtoCh(selected)}x" : "x";
                string ep = boardModel.EnPassant(selected, col, row) ? " e.p." : "";

                selected.Move(col, row, out bool Castle, out bool Promote);

                Update(oldRow);
                Update(row);

                AppendMoves(x, ep, Castle);

                if (Promote) BeginPromotion(selected);

                else
                    selected = null;

                player = opp;
                turn.Text = $"{player.Name} moves";
            }

            DoYourChecks();
        }
        
        public void DoYourChecks()
        {
            Color opp = player == Color.White ? Color.Black : Color.White;
            Figure kang = boardModel.FiguresByColor[player].First(fig => fig is King);
            Figure? check = kang.IsChecked(kang.Col, kang.Row);

            if (check != null)
            {
                if (!boardModel.AllValidMoves[player].Any(a => a.Value.Count > 0))
                {
                    board[kang.Col, kang.Row].BackColor = Color.Red;
                    board[check.Col, check.Row].ForeColor = Color.Red;
                    turn.Text = $"{opp.Name} wins!";
                    moves.Text += moves.Text[^1] == '#' ? "" : "#";
                    re.Text = "Restart";
                    gameState = false;
                }
                else
                {
                    board[kang.Col, kang.Row].BackColor = Color.Orange;
                    board[check.Col, check.Row].ForeColor = Color.Orange;
                    moves.Text += moves.Text[^1] == '+' ? "" : "+";
                }
            }
            else if (!boardModel.AllValidMoves[player].Any(a => a.Value.Count > 0))
                turn.Text = $"It's a Draw!";

        }


        private static char ColtoCh(Figure selected) => (char)(97 + selected.Col);
        private void Update(int row)
        {
            for (int i = 0; i < 8; i++)
                board[i, row].Text = boardModel.GetFig(i, row)?.Icon.ToString() ?? "";
        }

        private void Highlight(Figure piece)
        {
            //foreach (var c in piece.GetValidMoves())
            //    board[c.col, c.row].BackColor = Color.LightGreen;
            
            foreach (var c in boardModel.AllValidMoves[piece.Color][piece])
                board[c.col, c.row].BackColor = Color.LightGreen;
        }
        private void ResetHighlight()
        {
            for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        board[j, i].BackColor = (j + i) % 2 == 0 ? Color.DimGray : Color.White;
                        board[j, i].ForeColor = Color.Black;
                    }

                }
        }

        private void AppendMoves(string x, string ep, bool Castle)
        {
            if (player == Color.White)
            {
                moveCount++;
                moves.Text += Environment.NewLine;
                moves.Text += $"{moveCount}. {boardModel.letterString[boardModel.visuString.IndexOf(selected.Icon)]}";
            }
            else
                moves.Text += $" {boardModel.letterString[boardModel.visuString2.IndexOf(selected.Icon)]}";

            if (Castle)
            {
                moves.Text = moves.Text[..^1];

                if (selected.Col == 2)
                    moves.Text += "0-0-0";
                else
                    moves.Text += "0-0";
            }
            else
                moves.Text += x + $"{ColtoCh(selected)}" +
                    $"{selected.Row + 1}{ep}";
        }

        private void BeginPromotion(Figure fig)
        {
            Promotes = new Panel
            {
                Location = new Point(850, 50),
                Size = new Size(200, 50),
                BackColor = Color.Silver
            };
            Controls.Add(Promotes);

            for (int i = 0; i < 4;  i++)
            {
                Button option = new Button
                {
                    Text = fig.Color == Color.White ? boardModel.visuString[i+1].ToString() : boardModel.visuString2[i+1].ToString(),
                    Size = new Size(50, 50),
                    Font = new Font("Segoe UI Symbol", 20),
                    Location = new Point(i*50, 0),
                    BackColor = fig.Color == Color.Black ? Color.DimGray : Color.LightGray,
                    FlatStyle = FlatStyle.System,
                };
                option.Click += Sent;
                Promotes.Controls.Add(option);
            }
            Promotes.BringToFront();

            gameState = false;

        }

        private void Sent(object sender, EventArgs e)
        {
            Button choice = (Button)sender;
            char chosen = Convert.ToChar(choice.Text!);

            boardModel.Promote(selected, chosen);

            Controls.Remove(Promotes);
            gameState = true;

            Update(selected.Row);

            int ind = selected.Color == Color.White ? boardModel.visuString.IndexOf(chosen) : boardModel.visuString2.IndexOf(chosen);
            moves.Text += $"{boardModel.letterString[ind]}";

            selected = null;

            DoYourChecks();
        }
    }

    

}
