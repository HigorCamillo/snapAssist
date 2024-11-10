using System;
using System.Drawing;
using System.IO;
using System.Net;
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
        private string ftpIp;
        private string ftpPassword;
        public Suporte(string ip, string password)
        {
            InitializeComponent();
            this.ftpIp = ip;
            this.ftpPassword = password;

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
        private bool isImageLoading = false;  // Flag para controlar se a imagem está sendo carregada

        private void LoadImage(object sender, EventArgs e)
        {
            if (isImageLoading)
            {
                return; // Impede novas tentativas de carregar enquanto já há uma em andamento
            }

            try
            {
                isImageLoading = true;  // Marca que estamos carregando uma imagem

                // Caminho no servidor FTP
                string ftpImagePath = $"ftp://{ftpIp}/screenshot.png";

                // Baixar a imagem do servidor FTP diretamente em um Stream
                using (WebClient client = new WebClient())
                {
                    client.Credentials = new NetworkCredential("ftpUser", ftpPassword);
                    using (Stream stream = client.OpenRead(ftpImagePath))
                    {
                        // Limpar a imagem existente no PictureBox
                        pictureBox1.Image?.Dispose(); // Liberar os recursos da imagem anterior
                        pictureBox1.Image = null; // Limpar o PictureBox

                        // Carregar a imagem diretamente do fluxo
                        ImageShow = Image.FromStream(stream);
                        pictureBox1.Image = ImageShow; // Atribuir a imagem ao PictureBox
                        pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage; // Ajustar a imagem
                        pictureBox1.Dock = DockStyle.Fill; // Ajustar o PictureBox
                    }
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                isImageLoading = false; // Permite que a próxima atualização aconteça
            }
        }


        private void Form1_Resize(object sender, EventArgs e)
        {
            pictureBox1.Size = this.ClientSize;
        }

        private void PictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (!mouseMoved) // Só registra clique se o mouse não se moveu
            {
                if (e.Button == MouseButtons.Left)
                {
                    LogMouseAction($"Clique: {{X={e.X}, Y={e.Y}}}");
                }
                else if (e.Button == MouseButtons.Right)
                {
                    LogMouseAction($"Clique com Botão Direito: {{X={e.X}, Y={e.Y}}}");
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
                    lastCursor = e.Location; // Atualiza a posição do cursor durante o arrasto
                }
            }
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDragging && mouseMoved) // Loga apenas se houve movimento
            {
                isDragging = false;
                string action = $"Arrastando: {{X={initialDragPoint.X}, Y={initialDragPoint.Y}}} até {{X={lastCursor.X}, Y={lastCursor.Y}}}";
                LogMouseAction(action);
            }
            else
            {
                isDragging = false; // Apenas cancela o arrasto se não houve movimento
            }
        }

        private void LogMouseAction(string action)
        {
            // Formata a mensagem de ação
            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {action}";
            UpdateLog(logMessage);
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
