namespace ProjetoSuporteTI;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        try
        {
            InitializeComponent();
            System.Diagnostics.Debug.WriteLine(" MainPage carregada com sucesso");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($" Erro na MainPage: {ex}");
        }
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        try
        {
            await DisplayAlert(" Sucesso!", 
                "App funcionando perfeitamente!\n\n Próximos passos:\n Implementar tela de login\n Adicionar funcionalidades\n Testar no celular", 
                "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($" Erro no botão: {ex}");
        }
    }
}
