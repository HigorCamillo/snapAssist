using System;
using System.Diagnostics;
using System.Net;
using System.Windows.Forms;

namespace snapAssist
{
    public partial class Menu : Form
    {
        Form FormOpen = null;

        public Menu()
        {
            InitializeComponent();
            this.Load += new EventHandler(Menu_Load);
            this.FormClosing += new FormClosingEventHandler(Menu_FormClosing); // Conectar evento de fechamento
        }

        private async void Menu_Load(object sender, EventArgs e)
        {
            try
            {
                // Obter o endereço IP local
                string localIP = GetLocalIPAddress();
                label2.Text = localIP; // Exibir IP
                string randomPassword = GenerateRandomPassword(); // Gerar senha
                label5.Text = randomPassword; // Exibir senha

                // Configuração do FTP
                await Task.Run(() =>
                {
                    ExecuteCommand("netsh advfirewall firewall add rule name=\"FTP\" protocol=TCP dir=in localport=21 action=allow");
                    ExecuteCommand("dism /online /enable-feature /featurename:IIS-FTPServer /all");
                    ExecuteCommand("dism /online /enable-feature /featurename:IIS-WebServerRole /all");
                    ExecuteCommand("dism /online /enable-feature /featurename:IIS-FTPExtensibility /all");
                    ExecuteCommand("mkdir C:\\FTP");
                    ExecuteCommand($"powershell -Command \"Import-Module WebAdministration; New-WebFtpSite -Name '{localIP}' -Port 21 -PhysicalPath 'C:\\FTP' -Force\"");

                    ExecuteCommand($"powershell -Command \"Set-ItemProperty IIS:\\Sites\\{localIP} -Name Bindings -Value @{{'protocol'='ftp';'bindingInformation'='*:21:0.0.0.0'}}\"");

                    string ftpUserName = "ftpUser";
                    ExecuteCommand($"net user {ftpUserName} {randomPassword} /add");
                    ExecuteCommand($"powershell -Command \"$acl = Get-Acl 'C:\\FTP'; $rule = New-Object System.Security.AccessControl.FileSystemAccessRule('{ftpUserName}', 'Read,Write', 'Allow'); $acl.SetAccessRule($rule); Set-Acl 'C:\\FTP' $acl\"");

                    ExecuteCommand("powershell -Command \"Set-WebConfigurationProperty -pspath 'MACHINE/WEBROOT/APPHOST' -filter 'system.ftpServer/security/authentication/anonymousAuthentication' -name enabled -value false\"");
                    ExecuteCommand($"powershell -Command \"Start-WebItem 'IIS:\\Sites\\{localIP}'\"");
                });

                MessageBox.Show("Configuração do FTP concluída com sucesso!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao configurar o FTP: {ex.Message}");
            }
        }

        // Método para gerar uma senha aleatória
        private string GenerateRandomPassword(int length = 8)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            Random random = new Random();
            char[] res = new char[length];

            for (int i = 0; i < length; i++)
            {
                res[i] = valid[random.Next(valid.Length)];
            }

            return new string(res);
        }

        // Método para executar comandos no CMD
        private void ExecuteCommand(string command)
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = "/C " + command;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao executar comando: {ex.Message}");
            }
        }

        // Método para obter o IP da máquina
        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Nenhum IP local encontrado!");
        }

        // Método para parar o servidor FTP ao fechar o aplicativo
        private void Menu_FormClosing(object sender, FormClosingEventArgs e)
        {
            RemoveFtpConfiguration();
        }

        private void RemoveFtpConfiguration()
        {
            try
            {
                string localIP = GetLocalIPAddress();
                // Remover o site FTP
                ExecuteCommand($"powershell -Command \"Import-Module WebAdministration; Remove-WebSite -Name '{localIP}'\"");

                // Excluir o usuário FTP
                string ftpUserName = "ftpUser"; // Nome do usuário FTP
                ExecuteCommand($"net user {ftpUserName} /delete");

                // Remover a pasta FTP, se necessário
                ExecuteCommand("rmdir /S /Q C:\\FTP"); // Tenha cuidado com esse comando!
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao remover a configuração do FTP: {ex.Message}");
            }
        }

        // Seus outros métodos (para abrir formulários, etc.)
        private void button1_Click(object sender, EventArgs e)
        {
            Cliente cl = new Cliente();
            OpenForm(cl);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Suporte cl = new Suporte();
            OpenForm(cl);
        }

        private void OpenForm(Form NewForm)
        {
            try
            {
                if (FormOpen != null)
                {
                    FormOpen.Close();
                    FormOpen.Dispose();
                }
                FormOpen = NewForm;
                FormOpen.Show();
            }
            catch (Exception ex) { }
        }

    }
}
