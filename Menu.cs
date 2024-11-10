using System;
using System.Diagnostics;
using System.Net;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

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
                    // Adicionando a regra de firewall para permitir conexões FTP na porta 21
                    ExecuteCommand("netsh advfirewall firewall add rule name=\"FTP\" protocol=TCP dir=in localport=21 action=allow");

                    // Habilitando os recursos necessários para o servidor FTP no Windows
                    ExecuteCommand("dism /online /enable-feature /featurename:IIS-FTPServer /all");
                    ExecuteCommand("dism /online /enable-feature /featurename:IIS-WebServerRole /all");
                    ExecuteCommand("dism /online /enable-feature /featurename:IIS-FTPExtensibility /all");

                    // Criando o diretório FTP
                    ExecuteCommand("mkdir C:\\FTP");

                    // Criando o site FTP no IIS
                    ExecuteCommand($"powershell -Command \"Import-Module WebAdministration; New-WebFtpSite -Name '{localIP}' -Port 21 -PhysicalPath 'C:\\FTP' -Force\"");

                    // Configurando a vinculação do site FTP para a porta 21 no IP local
                    ExecuteCommand($"powershell -Command \"Set-ItemProperty IIS:\\Sites\\{localIP} -Name Bindings -Value @{{'protocol'='ftp';'bindingInformation'='*:21:0.0.0.0'}}\"");

                    // Criando o usuário 'ftpUser' e atribuindo uma senha
                    string ftpUserName = "ftpUser";
                    string randomPassword = GenerateRandomPassword(); // Substitua com sua lógica de senha
                    ExecuteCommand($"net user {ftpUserName} {randomPassword} /add");

                    // Definindo permissões de leitura e escrita para o diretório FTP
                    ExecuteCommand($"powershell -Command \"$acl = Get-Acl 'C:\\FTP'; $rule = New-Object System.Security.AccessControl.FileSystemAccessRule('{ftpUserName}', 'Read,Write', 'Allow'); $acl.SetAccessRule($rule); Set-Acl 'C:\\FTP' $acl\"");

                    // Desabilitando a autenticação anônima para o FTP
                    ExecuteCommand("powershell -Command \"Set-WebConfigurationProperty -pspath 'MACHINE/WEBROOT/APPHOST' -filter 'system.ftpServer/security/authentication/anonymousAuthentication' -name enabled -value false\"");

                    // Configurando SSL para o FTP (sem criptografia para o controle e dados)
                    ExecuteCommand($"powershell -Command \"Import-Module WebAdministration; Set-ItemProperty -Path 'IIS:\\Sites\\{localIP}' -Name 'ftpServer.security.ssl.controlChannelPolicy' -Value 0; Set-ItemProperty -Path 'IIS:\\Sites\\{localIP}' -Name 'ftpServer.security.ssl.dataChannelPolicy' -Value 0\"");

                    // Adicionando a configuração de autorização no IIS para permitir leitura e escrita para o usuário "ftpuser"
                    ExecuteCommand($"powershell -Command \"Import-Module WebAdministration; Add-WebConfigurationProperty -Filter '/system.ftpServer/security/authorization' -Name '.' -Value @{{accessType='Allow'; users='*'; permissions='Read, Write'}} -PSPath 'IIS:\\'\"");

                    // Iniciando e reiniciando o site FTP
                    ExecuteCommand($"powershell -Command \"Start-WebItem 'IIS:\\Sites\\{localIP}'\"");
                    ExecuteCommand($"powershell -Command \"Restart-WebItem 'IIS:\\Sites\\{localIP}'\"");
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

        private async void button2_Click(object sender, EventArgs e)
        {
            string ftpIp = textBox1.Text;  // IP do servidor FTP
            string ftpPassword = textBox2.Text;  // Senha fornecida pelo usuário

            try
            {
                // Tentar conectar ao FTP
                bool isConnected = await TryConnectToFtpAsync(ftpIp, ftpPassword);

                if (isConnected)
                {
                    // Se a conexão for bem-sucedida, abrir o Suporte
                    Suporte cl = new Suporte(ftpIp, ftpPassword);
                    OpenForm(cl);
                }
                else
                {
                    MessageBox.Show("Falha ao conectar ao FTP. Verifique o IP e a senha.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao tentar conectar ao FTP: {ex.Message}");
            }
        }

        private async Task<bool> TryConnectToFtpAsync(string ip, string password)
        {
            try
            {
                string ftpAddress = $"ftp://{ip}/";
                var ftpRequest = (FtpWebRequest)WebRequest.Create(ftpAddress);
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                ftpRequest.Credentials = new NetworkCredential("ftpUser", password);

                // Defina o modo de uso passivo se necessário (dependendo da configuração do servidor)
                ftpRequest.UsePassive = true;

                using (FtpWebResponse response = (FtpWebResponse)await ftpRequest.GetResponseAsync())
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao conectar ao FTP: {ex.Message}");
                return false;
            }
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
