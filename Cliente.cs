using System;
using System.Drawing;
using System.Windows.Forms;
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
            timer.Interval = 500; // 5000 milliseconds = 5 seconds
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            CaptureScreen();
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao capturar a tela: {ex.Message}");
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
    }
}
