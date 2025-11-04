namespace ProjetoSuporteTI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        try
        {
            InitializeComponent();
            
            // Registrar rotas explicitamente
            Routing.RegisterRoute("login", typeof(Views.Auth.LoginPage));
            Routing.RegisterRoute("createchamado", typeof(Views.Chamado.CreateChamadoPage));
            Routing.RegisterRoute("support", typeof(Views.Ai.SupportPage));
            
            System.Diagnostics.Debug.WriteLine("[SHELL] AppShell inicializado com rotas registradas");
            
            // Limpar qualquer estado de login anterior
            Preferences.Set("is_logged_in", false);
            Preferences.Remove("user_id");
            Preferences.Remove("user_name");
            Preferences.Remove("user_email");
            Preferences.Remove("user_cargo");
            
            System.Diagnostics.Debug.WriteLine("[SHELL] Estado de login limpo - sempre iniciar no login");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro no AppShell: {ex.Message}");
        }
    }

    public void ShowLoggedInPages()
    {
        // Não precisa mais fazer nada - todas as páginas estão sempre disponíveis
        System.Diagnostics.Debug.WriteLine("[SHELL] Páginas logadas habilitadas (não há mais controle de visibilidade)");
    }

    public void HideLoggedInPages()
    {
        // Não precisa mais fazer nada - todas as páginas estão sempre disponíveis  
        System.Diagnostics.Debug.WriteLine("[SHELL] Páginas logadas desabilitadas (não há mais controle de visibilidade)");
    }

    public async Task Logout()
    {
        try
        {
            Preferences.Clear();
            await Shell.Current.GoToAsync("//login");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro no logout: {ex.Message}");
        }
    }
}
