
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CatTrap
{
    public partial class MainWindow : Window
    {
        //Bunch of math,, thx stackoverflow for telling me how its done lmao
        private const int Rows = 11;
        private const int Columns = 11;

        private const double HexRadius = 36;

        private static readonly double HorizontalSpacing =
            Math.Sqrt(3) * HexRadius;

        private static readonly double VerticalSpacing =
            1.5 * HexRadius;

        // Board
        private bool[,] Blocked = new bool[Rows, Columns];

        private Polygon[,] Hexagons =
            new Polygon[Rows, Columns];

        // Cat position
        private int CatRow = 5;
        private int CatCol = 5;

        private TextBlock catVisual;

        // Game data
        private int moves = 0;
        private bool GameOver = false;

        // Colors!! We love those
        private readonly Brush NormalColor =
            new SolidColorBrush(Color.FromRgb(185, 255, 0));

        private readonly Brush BlockedColor =
            new SolidColorBrush(Color.FromRgb(119, 137, 0));

        private readonly Brush BorderColor =
            new SolidColorBrush(Color.FromRgb(247, 249, 235));

        public MainWindow()
        {
            InitializeComponent();

            InitializeGame();
        }

        // INITIALIZE THE GAME
        private void InitializeGame()
        {
            SetInitialObstacles();

            DrawBoard();

            DrawCat();
        }

        private void SetInitialObstacles()
        {
            Random rnd = new Random();
            int CordRow = rnd.Next(0, 11);
            int CordCol = rnd.Next(0, 11);
            int[,] obstacles =
            {
                /*{ 1, 1 },
                { 1, 3 },
                { 1, 6 },
                { 2, 0 },
                { 2, 2 },
                { 2, 10 },
                { 4, 3 },
                { 4, 10 },
                { 7, 2 },
                { 7, 8 },
                { 8, 6 },
                { 9, 1 },
                { 9, 2 },
                { 9, 3 },
                { 9, 8 },
                { 10, 3 },
                { 10, 6 }*/
            };

            for (int i = 0; i < 17; i++)
            {
                int row = obstacles[i, 0];
                int col = obstacles[i, 1];

                Blocked[row, col] = true;
            }
        }

        // DRAW BOARD

        private void DrawBoard()
        {
            GameCanvas.Children.Clear();

            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    Polygon hexagon = CreateHexagon(row, col);

                    Hexagons[row, col] = hexagon;

                    GameCanvas.Children.Add(hexagon);
                }
            }
            /*for (int row = 0; row < Rows; row++) For cords!!!! 🔥🔥🔥🔥
                {
                    for (int col = 0; col < Columns; col++)
                    {
                        Polygon hexagon = CreateHexagon(row, col);
                        Hexagons[row, col] = hexagon;
                        GameCanvas.Children.Add(hexagon);

                        // Add coordinate text label on each cell so you can see them easily
                        Point center = GetCellCenter(row, col);
                        TextBlock coordText = new TextBlock
                        {
                            Text = $"{row},{col}",
                            FontSize = 9,
                            Foreground = Brushes.DarkSlateGray,
                            IsHitTestVisible = false, // Allows clicking right through the text onto the hexagon
                            TextAlignment = TextAlignment.Center,
                            Width = 40
                        };

                        Canvas.SetLeft(coordText, center.X - 20);
                        Canvas.SetTop(coordText, center.Y - 6);
                        GameCanvas.Children.Add(coordText);
                    }
                }*/
        }

        private Point GetCellCenter(int row, int col)
        {
            double offsetX = (row % 2) *
                             (HorizontalSpacing / 2);

            double centerX = 45 +
                             col * HorizontalSpacing +
                             offsetX;

            double centerY = 45 +
                             row * VerticalSpacing;

            return new Point(centerX, centerY);
        }

        //Creating hexagons are complicated bro
        private Polygon CreateHexagon(int row, int col)
        {
            Polygon hexagon = new Polygon();

            Point center = GetCellCenter(row, col);

            for (int i = 0; i < 6; i++)
            {
                double angle = -Math.PI / 2 +
                               Math.PI / 3 * i;

                double x = center.X +
                           HexRadius * Math.Cos(angle);

                double y = center.Y +
                           HexRadius * Math.Sin(angle);

                hexagon.Points.Add(new Point(x, y));
            }

            hexagon.Fill = Blocked[row, col] ? BlockedColor : NormalColor;

            hexagon.Stroke = BorderColor;
            hexagon.StrokeThickness = 3;

            // Store cell coordinates cuz we need those
            hexagon.Tag = (row, col);

            // Make them clickable
            hexagon.MouseLeftButtonDown += Hexagon_MouseLeftButtonDown;

            return hexagon;
        }

        // DRAW CAT,, Live Laugh Cats
        private void DrawCat()
        {
            Point center = GetCellCenter(CatRow, CatCol);

            catVisual = new TextBlock
            {
                Text = "🐈‍⬛",
                FontFamily = new FontFamily("Segoe UI Emoji"),
                FontSize = 42,
                Width = 60,
                Height = 60,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                IsHitTestVisible = false
            };

            Canvas.SetLeft(catVisual, center.X - 30);
            Canvas.SetTop(catVisual, center.Y - 32);

            GameCanvas.Children.Add(catVisual);
        }

        private void UpdateCatPosition()
        {
            Point center = GetCellCenter(CatRow, CatCol);

            Canvas.SetLeft(catVisual, center.X - 30);
            Canvas.SetTop(catVisual, center.Y - 32);
        }

        // PLAYER CLICK 
        private void Hexagon_MouseLeftButtonDown( object sender, MouseButtonEventArgs e)
        {
            if (GameOver) return;

            Polygon hexagon = sender as Polygon;

            if (hexagon == null) return;

            var position = ((int, int))hexagon.Tag;

            int row = position.Item1;
            int col = position.Item2;

            // Cannot block an occupied or Blocked cell
            if (Blocked[row, col]) return;

            if (row == CatRow && col == CatCol) return;

            // Block the selected one
            Blocked[row, col] = true;

            hexagon.Fill = BlockedColor;

            // Update moves
            moves++;

            MovesText.Text = moves.ToString();

            // And finally move the cat
            MoveCat();
        }

        // CAT MOVEMENT
        private void MoveCat()
        {
            if (GameOver)
                return;

            if (!FindNextCatStep(out var NextPosition))
            {
                WinGame();
                return;
            }

            CatRow = NextPosition.Row;
            CatCol = NextPosition.Col;

            UpdateCatPosition();

            // Check if the cat escaped
            if (IsEdgeCell(CatRow, CatCol))
            {
                LoseGame();
            }
        }

        // PATHFINDING,, okay i had a little help from AI here cuz this is complicated for my brain,, ill will fill my brain in on how this works later so he cant suspect a thing (we just say we looked it up on the internet,, theres a few games with similar pathfinding available)


        private bool FindNextCatStep(out (int Row, int Col) NextPosition)
        {
            var start = (CatRow, CatCol);

            Queue<(int Row, int Col)> queue = new Queue<(int Row, int Col)>();

            HashSet<(int Row, int Col)> visited =  new HashSet<(int Row, int Col)>();

            Dictionary<(int Row, int Col),(int Row, int Col)> parent = new Dictionary<(int Row, int Col), (int Row, int Col)>();

            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                // Found an exit
                if (IsEdgeCell(current.Row, current.Col))
                {
                    var step = current;

                    while (parent[step] != start)
                    {
                        step = parent[step];
                    }

                    NextPosition = step;

                    return true;
                }

                foreach (var neighbor in GetNeighbors(
                    current.Row, current.Col))
                {
                    int r = neighbor.Row;
                    int c = neighbor.Col;

                    if (r < 0 || r >= Rows ||
                        c < 0 || c >= Columns)
                        continue;

                    if (Blocked[r, c]) continue;

                    if (visited.Contains(neighbor)) continue;

                    visited.Add(neighbor);

                    parent[neighbor] = current;

                    queue.Enqueue(neighbor);
                }
            }

            NextPosition = default;

            return false;
        }

        // HEXAGON NEIGHBORS

        private IEnumerable<(int Row, int Col)> GetNeighbors(
            int row, int col)
        {
            // Left and right
            yield return (row, col - 1);
            yield return (row, col + 1);

            if (row % 2 == 0)
            {
                // Even ones
                yield return (row - 1, col - 1);
                yield return (row - 1, col);

                yield return (row + 1, col - 1);
                yield return (row + 1, col);
            }
            else
            {
                // Odd ones
                yield return (row - 1, col);
                yield return (row - 1, col + 1);

                yield return (row + 1, col);
                yield return (row + 1, col + 1);
            }
        }

        // CHECK EDGE

        private bool IsEdgeCell(int row, int col)
        {
            return row == 0 ||
                   row == Rows - 1 ||
                   col == 0 ||
                   col == Columns - 1;
        }

        // WIN or LOSE pop ups,, we can change these if we want fancier ones maybe somehow
        private void WinGame()
        {
            GameOver = true;

            MessageBox.Show(
                $"Congratulations! You trapped the cat!\n\nMoves: {moves}",
                "You Win!",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void LoseGame()
        {
            GameOver = true;

            MessageBox.Show(
                $"The cat escaped!\n\nMoves: {moves}",
                "Game Over",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
