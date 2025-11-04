namespace ProjetoSuporteTI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        try
        {
            InitializeComponent();
            CheckLoginStatus();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro no AppShell: {ex.Message}");
        }
    }

    private async void CheckLoginStatus()
    {
        try
        {
            await Task.Delay(100); // Pequeno delay para estabilizar
            
            // Sempre mostrar login primeiro (forçar logout)
            Preferences.Set("is_logged_in", false);
            
            ShowLoginPage();
            await Shell.Current.GoToAsync("//login");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao verificar login: {ex.Message}");
        }
    }

    public void ShowLoggedInPages()
    {
        try
        {
            MainTabBar.IsVisible = true;
            
            var loginContent = Items.FirstOrDefault(item => item.Route == "login");
            if (loginContent != null)
                loginContent.IsVisible = false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao mostrar páginas logadas: {ex.Message}");
        }
    }

    public void ShowLoginPage()
    {
        try
        {
            MainTabBar.IsVisible = false;
            
            var loginContent = Items.FirstOrDefault(item => item.Route == "login");
            if (loginContent != null)
                loginContent.IsVisible = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao mostrar login: {ex.Message}");
        }
    }

    public async Task Logout()
    {
        try
        {
            Preferences.Clear();
            ShowLoginPage();
            await Shell.Current.GoToAsync("//login");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro no logout: {ex.Message}");
        }
    }
}
