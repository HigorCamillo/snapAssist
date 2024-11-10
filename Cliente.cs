using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Threading;
using System.Collections.Generic;
using Timer = System.Windows.Forms.Timer;

namespace snapAssist
{
    public partial class Cliente : Form
    {
        private Timer timer;

        public Cliente()
        {
            InitializeComponent();
            InitializeTimer();
        }

        private void InitializeTimer()
        {
            timer = new Timer();
            timer.Interval = 500; // Intervalo de 500ms
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            CaptureScreen();
            ProcessMouseLog();  // Processa o arquivo de log de eventos do mouse
        }

        private void CaptureScreen()
        {
            try
            {
                // Define os limites da tela
                Rectangle bounds = Screen.GetBounds(Point.Empty);
                using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        // Captura a tela
                        g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);

                        // Obtém a posição do mouse
                        Point mousePosition = Cursor.Position;

                        // Desenha o ponteiro do mouse na captura
                        DrawCursor(g, mousePosition);
                    }

                    // Salvar a captura de tela
                    SaveScreenshotToFTP(bitmap);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao capturar a tela: {ex.Message}");
            }
        }

        private void SaveScreenshotToFTP(Bitmap bitmap)
        {
            try
            {
                // Defina o diretório FTP para salvar a captura
                string ftpDirectory = @"C:\FTP";

                if (!Directory.Exists(ftpDirectory))
                {
                    Directory.CreateDirectory(ftpDirectory);
                }

                string fileName = Path.Combine(ftpDirectory, "screenshot.png");

                // Salva a imagem no diretório FTP
                bitmap.Save(fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar a captura de tela: {ex.Message}");
            }
        }

        private void DrawCursor(Graphics g, Point mousePosition)
        {
            // Desenha o cursor do mouse
            Cursor cursor = Cursors.Default; // Você pode mudar para outro cursor, se desejar
            int cursorX = mousePosition.X - cursor.Size.Width / 2; // Centraliza o cursor
            int cursorY = mousePosition.Y - cursor.Size.Height / 2;

            // Desenha o cursor na imagem capturada
            cursor.Draw(g, new Rectangle(cursorX, cursorY, cursor.Size.Width, cursor.Size.Height));
        }

        // Função para processar os eventos do mouse do arquivo de log
        private void ProcessMouseLog()
        {
            // Caminho do arquivo
            string filePath = @"C:\FTP\mouse_log.txt";

            // Verificar se o arquivo existe
            if (!File.Exists(filePath))
            {
                return;
            }

            // Ler todas as linhas do arquivo
            List<string> lines = new List<string>(File.ReadAllLines(filePath));

            // Processar as linhas uma por uma
            while (lines.Count > 0)
            {
                string line = lines[0]; // Pega a primeira linha
                ProcessMouseEvent(line); // Processa o evento do mouse
                lines.RemoveAt(0); // Remove a linha processada

                // Regrava o arquivo com as linhas restantes
                File.WriteAllLines(filePath, lines);
                Thread.Sleep(200); // Atraso entre a execução de eventos
            }

        }

        // Função para processar os eventos de mouse do arquivo
        private void ProcessMouseEvent(string line)
        {
            // Regex para identificar os comandos do mouse
            var clickRegex = new Regex(@"Clique: {X=(\d+), Y=(\d+)}");
            var dragRegex = new Regex(@"Arrastando: {X=(\d+), Y=(\d+)} até {X=(\d+), Y=(\d+)}");
            var doubleClickRegex = new Regex(@"Duplo Clique: {X=(\d+), Y=(\d+)}");

            if (clickRegex.IsMatch(line))
            {
                var match = clickRegex.Match(line);
                int x = int.Parse(match.Groups[1].Value);
                int y = int.Parse(match.Groups[2].Value);
                MouseClick(x, y);
            }
            else if (dragRegex.IsMatch(line))
            {
                var match = dragRegex.Match(line);
                int startX = int.Parse(match.Groups[1].Value);
                int startY = int.Parse(match.Groups[2].Value);
                int endX = int.Parse(match.Groups[3].Value);
                int endY = int.Parse(match.Groups[4].Value);
                MouseDrag(startX, startY, endX, endY);
            }
            else if (doubleClickRegex.IsMatch(line))
            {
                var match = doubleClickRegex.Match(line);
                int x = int.Parse(match.Groups[1].Value);
                int y = int.Parse(match.Groups[2].Value);
                MouseDoubleClick(x, y);
            }
        }

        // Função para simular clique do mouse
        private void MouseClick(int x, int y)
        {
            // Simulando clique do mouse
            Cursor.Position = new System.Drawing.Point(x, y);
            MouseEvent(MouseEventFlags.LeftDown);
            Thread.Sleep(100); // Atraso entre os eventos
            MouseEvent(MouseEventFlags.LeftUp);
            Console.WriteLine($"Clique em {x}, {y}");
        }

        // Função para simular arrasto do mouse
        private void MouseDrag(int startX, int startY, int endX, int endY)
        {
            // Simulando o arrastar do mouse
            Cursor.Position = new System.Drawing.Point(startX, startY);
            MouseEvent(MouseEventFlags.LeftDown);
            Thread.Sleep(100); // Atraso entre os eventos
            Cursor.Position = new System.Drawing.Point(endX, endY);
            Thread.Sleep(100); // Atraso entre os eventos
            MouseEvent(MouseEventFlags.LeftUp);
            Console.WriteLine($"Arrastando de {startX}, {startY} até {endX}, {endY}");
        }

        // Função para simular duplo clique do mouse
        private void MouseDoubleClick(int x, int y)
        {
            // Simulando duplo clique do mouse
            Cursor.Position = new System.Drawing.Point(x, y);
            MouseEvent(MouseEventFlags.LeftDown);
            Thread.Sleep(100);
            MouseEvent(MouseEventFlags.LeftUp);
            Thread.Sleep(100);
            MouseEvent(MouseEventFlags.LeftDown);
            Thread.Sleep(100);
            MouseEvent(MouseEventFlags.LeftUp);
            Console.WriteLine($"Duplo clique em {x}, {y}");
        }

        // Função para simular eventos do mouse com a API do Windows
        private void MouseEvent(MouseEventFlags value)
        {
            mouse_event((int)value, 0, 0, 0, 0);
        }

        // Definição das flags de eventos do mouse
        [Flags]
        public enum MouseEventFlags : int
        {
            Move = 0x0001,
            LeftDown = 0x0002,
            LeftUp = 0x0004,
            RightDown = 0x0008,
            RightUp = 0x0010,
            MiddleDown = 0x0020,
            MiddleUp = 0x0040,
            Wheel = 0x0800,
            XDown = 0x0080,
            XUp = 0x0100
        }
        private void Cliente_Load(object sender, EventArgs e)
        {
            // Centralizar horizontalmente e posicionar no topo
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;

            int formWidth = this.Width;
            int formHeight = this.Height;

            // Definir a posição do formulário (centrado horizontalmente e no topo da tela)
            this.Location = new Point((screenWidth - formWidth) / 2, 0);


        }
        // Importando a função mouse_event da User32.dll
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
    }
}
