using ProjetoSuporteTI.Services;

namespace ProjetoSuporteTI.Views.Auth;

public partial class LoginPage : ContentPage
{
    private readonly ApiService _apiService;

    public LoginPage()
    {
        InitializeComponent();
        _apiService = new ApiService();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        System.Diagnostics.Debug.WriteLine("[LOGIN] Página de login aparecendo...");
        
        // Limpar campos sempre que a página aparecer
        EmailEntry.Text = "";
        PasswordEntry.Text = "";
        
        // Garantir que o estado de login seja false
        Preferences.Set("is_logged_in", false);
        
        System.Diagnostics.Debug.WriteLine("[LOGIN] Campos limpos e estado resetado");
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("=== INÍCIO DO LOGIN ===");
            
            var email = EmailEntry.Text?.Trim();
            var password = PasswordEntry.Text?.Trim();

            System.Diagnostics.Debug.WriteLine($"Email digitado: '{email}'");
            System.Diagnostics.Debug.WriteLine($"Senha digitada: '{password}'");

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("❌ Erro", "Por favor, preencha email e senha!", "OK");
                return;
            }

            // Desabilitar botão durante login
            LoginButton.IsEnabled = false;
            LoginButton.Text = "🔄 Entrando...";

            System.Diagnostics.Debug.WriteLine("Verificando credenciais...");

            // ✅ LOGIN FIXO PARA DESENVOLVIMENTO
            if (email.ToLower() == "adm" && password == "123")
            {
                System.Diagnostics.Debug.WriteLine("✅ Credenciais corretas! Salvando dados...");
                
                // Salvar dados do administrador
                Preferences.Set("user_id", "1");
                Preferences.Set("user_name", "Administrador");
                Preferences.Set("user_email", "adm@suporteti.com");
                Preferences.Set("user_cargo", "Administrador do Sistema");
                Preferences.Set("is_logged_in", true);

                System.Diagnostics.Debug.WriteLine("Dados salvos, mostrando mensagem...");
                await DisplayAlert("✅ Sucesso", "Bem-vindo, Administrador!", "Continuar");

                // Limpar campos
                EmailEntry.Text = "";
                PasswordEntry.Text = "";

                System.Diagnostics.Debug.WriteLine("Navegando para tela principal...");
                // Navegar para tela principal
                NavigateToMain();
                return;
            }

            System.Diagnostics.Debug.WriteLine("Credenciais não conferem, tentando API...");

            // LOGIN VIA API PARA OUTROS USUÁRIOS
            var loginResult = await _apiService.LoginAsync(email, password);

            if (loginResult.Success)
            {
                System.Diagnostics.Debug.WriteLine("✅ Login via API bem-sucedido!");
                
                // Salvar dados do usuário
                Preferences.Set("user_id", loginResult.User?.Id.ToString() ?? "");
                Preferences.Set("user_name", loginResult.User?.Nome ?? "");
                Preferences.Set("user_email", loginResult.User?.Email ?? "");
                Preferences.Set("user_cargo", loginResult.User?.Cargo ?? "");
                Preferences.Set("is_logged_in", true);

                await DisplayAlert("✅ Sucesso", $"Bem-vindo, {loginResult.User?.Nome}!", "Continuar");

                // Limpar campos
                EmailEntry.Text = "";
                PasswordEntry.Text = "";

                // Navegar para tela principal
                NavigateToMain();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("❌ Login via API falhou");
                await DisplayAlert("❌ Login Falhou", loginResult.Message, "Tentar novamente");
                PasswordEntry.Text = ""; // Limpar apenas senha
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ ERRO NO LOGIN: {ex.Message}");
            await DisplayAlert("❌ Erro de Conexão", 
                $"Não foi possível conectar ao servidor: {ex.Message}", "OK");
        }
        finally
        {
            // Reabilitar botão
            LoginButton.IsEnabled = true;
            LoginButton.Text = "🚀 Entrar";
            System.Diagnostics.Debug.WriteLine("=== FIM DO LOGIN ===");
        }
    }

    private async void OnForgotPasswordClicked(object sender, EventArgs e)
    {
        await DisplayAlert("🔑 Recuperar Senha", 
            "Para recuperar sua senha, entre em contato com o administrador do sistema.\n\n📧 admin@empresa.com\n📞 (11) 99999-9999\n\nInforme seu email cadastrado e uma nova senha será enviada.", "OK");
    }

    private async void NavigateToMain()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[LOGIN] === INICIANDO NAVEGAÇÃO ===");
            
            // Primeiro mostrar as páginas
            var shell = Shell.Current as AppShell;
            if (shell != null)
            {
                System.Diagnostics.Debug.WriteLine("[LOGIN] Mostrando páginas logadas...");
                shell.ShowLoggedInPages();
                
                // Aguardar para garantir que as mudanças sejam aplicadas
                await Task.Delay(500);
            }
            
            System.Diagnostics.Debug.WriteLine("[LOGIN] Tentando navegar para createchamado...");
            
            // Navegar usando rota absoluta
            await Shell.Current.GoToAsync("//createchamado");
            
            System.Diagnostics.Debug.WriteLine("[LOGIN] === NAVEGAÇÃO CONCLUÍDA ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LOGIN] === ERRO DE NAVEGAÇÃO ===");
            System.Diagnostics.Debug.WriteLine($"[LOGIN] Erro: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[LOGIN] StackTrace: {ex.StackTrace}");
            
            await DisplayAlert("❌ Erro de Navegação", 
                $"Não foi possível navegar para a página de chamados.\n\nErro: {ex.Message}\n\nTente novamente ou entre em contato com o suporte.", 
                "OK");
        }
    }
}
