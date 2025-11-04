namespace ProjetoSuporteTI.Views.Auth;

public partial class ProfilePage : ContentPage
{
    public ProfilePage()
    {
        InitializeComponent();
        LoadUserInfo();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadUserInfo();
    }

    private void LoadUserInfo()
    {
        var userName = Preferences.Get("user_name", "Usuário");
        var userEmail = Preferences.Get("user_email", "email@exemplo.com");
        var userCargo = Preferences.Get("user_cargo", "Cargo");

        // Atualizar labels (se existirem)
        Title = $"Perfil - {userName}";
    }

    private async void OnUpdateProfileClicked(object sender, EventArgs e)
    {
        await DisplayAlert("ℹ Atualizar Perfil", 
            "Funcionalidade em desenvolvimento.", "OK");
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        var result = await DisplayAlert(" Logout", "Deseja realmente sair?", "Sim", "Não");
        
        if (result)
        {
            var shell = Shell.Current as AppShell;
            if (shell != null)
            {
                await shell.Logout();
            }
        }
    }
}
