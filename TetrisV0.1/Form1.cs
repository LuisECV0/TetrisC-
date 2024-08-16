using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Tetris
{
    public partial class Form1 : Form
    {
        private int gridWidth = 10;
        private int gridHeight = 20;
        private int cellSize = 30;
        private int[,] grid;
        private Timer gameTimer;
        private Tetromino currentPiece;
        private Point piecePosition;
        private Random random = new Random();

        public Form1()
        {
            InitializeComponent();
            grid = new int[gridHeight, gridWidth];
            gameTimer = new Timer();
            gameTimer.Interval = 500;  // Intervalo de caída de la pieza
            gameTimer.Tick += GameTick;
            gameTimer.Start();
            currentPiece = GenerateRandomPiece();
            piecePosition = new Point(gridWidth / 2 - 1, 0);  // Posición inicial
        }

        private Tetromino GenerateRandomPiece()
        {
            Tetromino[] pieces = new Tetromino[]
            {
                new Tetromino(new Point[] { new Point(0, 0), new Point(1, 0), new Point(-1, 0), new Point(2, 0) }, Color.Cyan), // I
                new Tetromino(new Point[] { new Point(0, 0), new Point(1, 0), new Point(0, 1), new Point(1, 1) }, Color.Yellow), // O
                new Tetromino(new Point[] { new Point(0, 0), new Point(-1, 0), new Point(1, 0), new Point(0, 1) }, Color.Purple), // T
                new Tetromino(new Point[] { new Point(0, 0), new Point(1, 0), new Point(-1, 1), new Point(0, 1) }, Color.Green), // S
                new Tetromino(new Point[] { new Point(0, 0), new Point(-1, 0), new Point(1, 1), new Point(0, 1) }, Color.Red), // Z
                new Tetromino(new Point[] { new Point(0, 0), new Point(-1, 0), new Point(-2, 0), new Point(0, 1) }, Color.Blue), // L
                new Tetromino(new Point[] { new Point(0, 0), new Point(1, 0), new Point(2, 0), new Point(0, 1) }, Color.Orange) // J
            };
            return pieces[random.Next(pieces.Length)];
        }

        private void GameTick(object sender, EventArgs e)
        {
            MovePieceDown();
            Invalidate();  // Redibuja la pantalla
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            // Dibujar la cuadrícula
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    if (grid[y, x] != 0)
                    {
                        g.FillRectangle(Brushes.Blue, x * cellSize, y * cellSize, cellSize, cellSize);
                    }
                    g.DrawRectangle(Pens.Black, x * cellSize, y * cellSize, cellSize, cellSize);
                }
            }

            // Dibujar la pieza actual
            foreach (Point block in currentPiece.Blocks)
            {
                g.FillRectangle(new SolidBrush(currentPiece.Color), (block.X + piecePosition.X) * cellSize, (block.Y + piecePosition.Y) * cellSize, cellSize, cellSize);
                g.DrawRectangle(Pens.Black, (block.X + piecePosition.X) * cellSize, (block.Y + piecePosition.Y) * cellSize, cellSize, cellSize);
            }
        }

        private void MovePieceDown()
        {
            piecePosition.Y++;

            if (CheckCollision())
            {
                piecePosition.Y--;
                FixPiece();
                ClearFullRows();
                currentPiece = GenerateRandomPiece();
                piecePosition = new Point(gridWidth / 2 - 1, 0);

                if (CheckCollision())  // Si colisiona al generar la nueva pieza, el juego termina
                {
                    gameTimer.Stop();
                    MessageBox.Show("Juego Terminado");
                }
            }
        }

        private bool CheckCollision()
        {
            foreach (Point block in currentPiece.Blocks)
            {
                int newX = block.X + piecePosition.X;
                int newY = block.Y + piecePosition.Y;

                if (newX < 0 || newX >= gridWidth || newY >= gridHeight)
                    return true;

                if (newY >= 0 && grid[newY, newX] != 0)
                    return true;
            }

            return false;
        }

        private void FixPiece()
        {
            foreach (Point block in currentPiece.Blocks)
            {
                int x = block.X + piecePosition.X;
                int y = block.Y + piecePosition.Y;

                if (y >= 0)
                {
                    grid[y, x] = 1;  // Marca la celda como ocupada
                }
            }
        }

        private void ClearFullRows()
        {
            for (int y = gridHeight - 1; y >= 0; y--)
            {
                bool isFull = true;
                for (int x = 0; x < gridWidth; x++)
                {
                    if (grid[y, x] == 0)
                    {
                        isFull = false;
                        break;
                    }
                }

                if (isFull)
                {
                    // Desplazar filas hacia abajo
                    for (int row = y; row > 0; row--)
                    {
                        for (int col = 0; col < gridWidth; col++)
                        {
                            grid[row, col] = grid[row - 1, col];
                        }
                    }

                    for (int col = 0; col < gridWidth; col++)
                    {
                        grid[0, col] = 0;  // Limpia la fila superior
                    }

                    y++;  // Revisa la misma fila de nuevo después de haberla desplazado
                }
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Left:
                    piecePosition.X--;
                    if (CheckCollision()) piecePosition.X++;
                    break;

                case Keys.Right:
                    piecePosition.X++;
                    if (CheckCollision()) piecePosition.X--;
                    break;

                case Keys.Down:
                    MovePieceDown();
                    break;

                case Keys.Up:
                    RotatePiece();
                    break;
            }
            Invalidate();  // Redibuja el formulario después de mover
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void RotatePiece()
        {
            // Rotación de la pieza
            for (int i = 0; i < currentPiece.Blocks.Length; i++)
            {
                int x = currentPiece.Blocks[i].X;
                int y = currentPiece.Blocks[i].Y;

                currentPiece.Blocks[i].X = -y;
                currentPiece.Blocks[i].Y = x;
            }

            if (CheckCollision())  // Si colisiona, deshace la rotación
            {
                for (int i = 0; i < currentPiece.Blocks.Length; i++)
                {
                    int x = currentPiece.Blocks[i].X;
                    int y = currentPiece.Blocks[i].Y;

                    currentPiece.Blocks[i].X = y;
                    currentPiece.Blocks[i].Y = -x;
                }
            }
        }
    }

    public class Tetromino
    {
        public Point[] Blocks { get; private set; }
        public Color Color { get; private set; }

        public Tetromino(Point[] blocks, Color color)
        {
            Blocks = blocks;
            Color = color;
        }
    }
}

