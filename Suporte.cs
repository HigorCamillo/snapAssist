using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace snapAssist
{
    public partial class Suporte : Form
    {
        private bool isDragging = false;
        private Point lastCursor;

        public Suporte()
        {
            InitializeComponent();
            LoadImage();
            this.Resize += Form1_Resize;

            // Adiciona os manipuladores de eventos do mouse ao PictureBox
            this.pictureBox1.MouseClick += PictureBox1_MouseClick;
            this.pictureBox1.MouseDoubleClick += PictureBox1_MouseDoubleClick;
            this.pictureBox1.MouseDown += PictureBox1_MouseDown;
            this.pictureBox1.MouseMove += PictureBox1_MouseMove;
            this.pictureBox1.MouseUp += PictureBox1_MouseUp;
        }

        private void LoadImage()
        {
            try
            {
                string imagePath = @"C:\Users\higor\Desktop\screenshot.png";
                if (File.Exists(imagePath))
                {
                    pictureBox1.Image = Image.FromFile(imagePath);
                    pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox1.Dock = DockStyle.Fill;
                }
                else
                {
                    MessageBox.Show("Imagem não encontrada!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar a imagem: " + ex.Message);
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            pictureBox1.Size = this.ClientSize;
        }

        private void PictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            LogMouseAction($"Clique: {e.Location}");
        }

        private void PictureBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            LogMouseAction($"Duplo Clique: {e.Location}");
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                LogMouseAction($"Clique com Botão Direito: {e.Location}");
            }
            else
            {
                isDragging = true;
                lastCursor = e.Location;
            }
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                LogMouseAction($"Arrastando: {e.Location}");
            }
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
            LogMouseAction($"Soltou o botão do mouse: {lastCursor}");
        }

        private void LogMouseAction(string action)
        {
            // Formata a mensagem de ação
            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {action}";
            UpdateLog(logMessage); // Atualiza o log diretamente
        }

        private void UpdateLog(string logMessage)
        {
            string logPath = @"C:\Users\higor\Desktop\mouse_log.txt"; // Caminho do arquivo de log

            try
            {
                // Certifica-se de que o diretório do arquivo existe
                string directoryPath = Path.GetDirectoryName(logPath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Adiciona o log diretamente no arquivo sem armazenar em uma lista
                using (StreamWriter sw = new StreamWriter(logPath, true))
                {
                    sw.WriteLine(logMessage);
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao atualizar o arquivo de log: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
