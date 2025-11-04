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
        
        // Limpar campos
        EmailEntry.Text = "";
        PasswordEntry.Text = "";
        
        // Verificar se já está logado
        var isLoggedIn = Preferences.Get("is_logged_in", false);
        if (isLoggedIn)
        {
            NavigateToMain();
        }
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim();
        var password = PasswordEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("❌ Erro", "Por favor, preencha email e senha!", "OK");
            return;
        }

        // Desabilitar botão durante login
        LoginButton.IsEnabled = false;
        LoginButton.Text = "🔄 Entrando...";

        try
        {
            // ✅ LOGIN FIXO PARA DESENVOLVIMENTO
            if (email.ToLower() == "adm" && password == "123")
            {
                // Salvar dados do administrador
                Preferences.Set("user_id", "1");
                Preferences.Set("user_name", "Administrador");
                Preferences.Set("user_email", "adm@suporteti.com");
                Preferences.Set("user_cargo", "Administrador do Sistema");
                Preferences.Set("is_logged_in", true);

                await DisplayAlert("✅ Sucesso", "Bem-vindo, Administrador!", "Continuar");

                // Limpar campos
                EmailEntry.Text = "";
                PasswordEntry.Text = "";

                // Navegar para tela principal
                NavigateToMain();
                return;
            }

            // LOGIN VIA API PARA OUTROS USUÁRIOS
            var loginResult = await _apiService.LoginAsync(email, password);

            if (loginResult.Success)
            {
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
                await DisplayAlert("❌ Login Falhou", loginResult.Message, "Tentar novamente");
                PasswordEntry.Text = ""; // Limpar apenas senha
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("❌ Erro de Conexão", 
                $"Não foi possível conectar ao servidor: {ex.Message}", "OK");
        }
        finally
        {
            // Reabilitar botão
            LoginButton.IsEnabled = true;
            LoginButton.Text = "🚀 Entrar";
        }
    }

    private async void OnCreateAccountClicked(object sender, EventArgs e)
    {
        await DisplayAlert("ℹ️ Criar Conta", 
            "Para criar uma nova conta, entre em contato com o administrador do sistema.\n\n📧 admin@empresa.com\n📞 (11) 99999-9999\n\n🔐 Para testes use:\nUsuário: adm\nSenha: 123", "OK");
    }

    private void NavigateToMain()
    {
        var shell = Shell.Current as AppShell;
        shell?.ShowLoggedInPages();
        
        Shell.Current.GoToAsync("//mainpage");
    }
}
