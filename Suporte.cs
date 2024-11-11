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
        private Point initialDragPoint; 
        private bool mouseMoved = false; 

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

            this.KeyPreview = true; // Permite que o formulário capture eventos de teclado antes dos controles
            this.KeyDown += Suporte_KeyDown;

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
                return;
            }

            try
            {
                isImageLoading = true; 

                string ftpImagePath = $"ftp://{ftpIp}/screenshot.png";

                // Baixar a imagem do servidor FTP diretamente em um Stream
                using (WebClient client = new WebClient())
                {
                    client.Credentials = new NetworkCredential("ftpUser", ftpPassword);
                    using (Stream stream = client.OpenRead(ftpImagePath))
                    {
                        // Limpar a imagem existente no PictureBox
                        pictureBox1.Image?.Dispose();
                        pictureBox1.Image = null;

                        ImageShow = Image.FromStream(stream);
                        pictureBox1.Image = ImageShow; 
                        pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage; 
                        pictureBox1.Dock = DockStyle.Fill; 
                    }
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                isImageLoading = false;
            }
        }


        private void Form1_Resize(object sender, EventArgs e)
        {
            pictureBox1.Size = this.ClientSize;
        }

        private void PictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (!mouseMoved)
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
                mouseMoved = false; 
                initialDragPoint = e.Location;
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
            if (isDragging && mouseMoved)
            {
                isDragging = false;
                string action = $"Arrastando: {{X={initialDragPoint.X}, Y={initialDragPoint.Y}}} até {{X={lastCursor.X}, Y={lastCursor.Y}}}";
                LogMouseAction(action);
            }
            else
            {
                isDragging = false;
            }
        }

        private void LogMouseAction(string action)
        {
            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {action}";
            UpdateLog(logMessage);
        }

        private void Suporte_KeyDown(object sender, KeyEventArgs e)
        {
            string keyAction = $"Tecla Pressionada: {e.KeyCode}";
            UpdateLog(keyAction);
        }

        private void UpdateLog(string logMessage)
        {
            try
            {
                string ftpLogPath = $"ftp://{ftpIp}/mouse_log.txt";

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpLogPath);
                request.Method = WebRequestMethods.Ftp.AppendFile; // Usar "AppendFile" para adicionar dados ao arquivo existente

                request.Credentials = new NetworkCredential("ftpUser", ftpPassword);

                using (Stream requestStream = request.GetRequestStream())
                {
                    using (StreamWriter writer = new StreamWriter(requestStream))
                    {
                        writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {logMessage}");
                    }
                }

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                   
                }
            }
            catch (Exception ex)
            {
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
