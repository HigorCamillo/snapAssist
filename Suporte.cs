using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace snapAssist
{
    public partial class Suporte : Form
    {
        private bool isDragging = false;
        private Point lastCursor;
        private Point initialDragPoint; // Ponto inicial do arrasto
        private bool mouseMoved = false; // Flag para detectar movimento do mouse

        private Timer timer;
        public Suporte()
        {
            InitializeComponent();
            this.Resize += Form1_Resize;

            // Adiciona os manipuladores de eventos do mouse ao PictureBox
            this.pictureBox1.MouseClick += PictureBox1_MouseClick;
            this.pictureBox1.MouseDoubleClick += PictureBox1_MouseDoubleClick;
            this.pictureBox1.MouseDown += PictureBox1_MouseDown;
            this.pictureBox1.MouseMove += PictureBox1_MouseMove;
            this.pictureBox1.MouseUp += PictureBox1_MouseUp;

            timer = new Timer();
            timer.Interval = 100; // 5000 milliseconds = 5 seconds
            timer.Tick += new EventHandler(LoadImage);
            timer.Start();
        }
        Image ImageShow = null;
        private void LoadImage(object sender, EventArgs e)
        {
            try
            {
                string imagePath = @"\\desktop-q09qmnv\prints\screenshot.png";
                string suportePath = @"\\desktop-q09qmnv\prints\suporte.png";
                try
                {
                    pictureBox1.Image = null;

                    ImageShow?.Dispose();
                    ImageShow = null;
                }
                catch { }

                File.Copy(imagePath, suportePath, true);

                if (File.Exists(suportePath))
                {
                    ImageShow = Image.FromFile(suportePath);
                    pictureBox1.Image = ImageShow;
                    pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox1.Dock = DockStyle.Fill;

                }
                else
                {
                    MessageBox.Show("Imagem n�o encontrada!");
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Erro ao carregar a imagem: " + ex.Message);
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            pictureBox1.Size = this.ClientSize;
        }

        private void PictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (!mouseMoved) // S� registra clique se o mouse n�o se moveu
            {
                if (e.Button == MouseButtons.Left)
                {
                    LogMouseAction($"Clique: {{X={e.X}, Y={e.Y}}}");
                }
                else if (e.Button == MouseButtons.Right)
                {
                    LogMouseAction($"Clique com Bot�o Direito: {{X={e.X}, Y={e.Y}}}");
                }
            }
        }

        private void PictureBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            LogMouseAction($"Duplo Clique: {{X={e.X}, Y={e.Y}}}");
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                mouseMoved = false; // Reseta o flag de movimento
                initialDragPoint = e.Location; // Armazena o ponto inicial do arrasto
            }
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                if (e.Location != initialDragPoint) // Verifica se o mouse realmente se moveu
                {
                    mouseMoved = true;
                    lastCursor = e.Location; // Atualiza a posi��o do cursor durante o arrasto
                }
            }
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDragging && mouseMoved) // Loga apenas se houve movimento
            {
                isDragging = false;
                string action = $"Arrastando: {{X={initialDragPoint.X}, Y={initialDragPoint.Y}}} at� {{X={lastCursor.X}, Y={lastCursor.Y}}}";
                LogMouseAction(action);
            }
            else
            {
                isDragging = false; // Apenas cancela o arrasto se n�o houve movimento
            }
        }

        private void LogMouseAction(string action)
        {
            // Formata a mensagem de a��o
            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {action}";
            UpdateLog(logMessage);
        }

        private void UpdateLog(string logMessage)
        {
            string logPath = @"C:\Users\higor\Desktop\mouse_log.txt"; // Caminho do arquivo de log

            try
            {
                // Certifica-se de que o diret�rio do arquivo existe
                string directoryPath = Path.GetDirectoryName(logPath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Adiciona o log diretamente no arquivo
                using (StreamWriter sw = new StreamWriter(logPath, true))
                {
                    sw.WriteLine(logMessage);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Erro ao atualizar o arquivo de log: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Suporte_Load(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
