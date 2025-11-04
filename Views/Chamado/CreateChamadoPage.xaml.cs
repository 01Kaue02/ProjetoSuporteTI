using ProjetoSuporteTI.Services;
using ProjetoSuporteTI.Models;

namespace ProjetoSuporteTI.Views.Chamado;

public partial class CreateChamadoPage : ContentPage
{
    private readonly ApiService _apiService;

    public CreateChamadoPage()
    {
        InitializeComponent();
        _apiService = new ApiService();
        
        // Definir valores padrão
        PrioridadePicker.SelectedIndex = 0; // Baixo
        CategoriaPicker.SelectedIndex = 0; // Hardware
    }

    private async void OnEnviarClicked(object sender, EventArgs e)
    {
        var descricao = DescricaoEditor.Text?.Trim();
        var prioridade = PrioridadePicker.SelectedItem?.ToString();
        var categoria = CategoriaPicker.SelectedItem?.ToString();

        // Validação
        if (string.IsNullOrWhiteSpace(descricao))
        {
            await DisplayAlert("❌ Erro", "Por favor, descreva o problema!", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(prioridade))
        {
            await DisplayAlert("❌ Erro", "Por favor, selecione a prioridade!", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(categoria))
        {
            await DisplayAlert("❌ Erro", "Por favor, selecione a categoria!", "OK");
            return;
        }

        // Desabilitar botão durante processamento
        EnviarButton.IsEnabled = false;
        EnviarButton.Text = "🔄 Enviando...";

        try
        {
            // Salvar dados do chamado para passar para a IA
            Preferences.Set("chamado_descricao", descricao);
            Preferences.Set("chamado_prioridade", prioridade);
            Preferences.Set("chamado_categoria", categoria);
            Preferences.Set("chamado_created_at", DateTime.Now.ToString());

            await DisplayAlert("✅ Sucesso", "Chamado criado! A IA irá tentar resolver seu problema.", "Continuar");

            // Limpar campos
            DescricaoEditor.Text = "";
            PrioridadePicker.SelectedIndex = 0;
            CategoriaPicker.SelectedIndex = 0;

            // Navegar para a IA
            await Shell.Current.GoToAsync("//support");
        }
        catch (Exception ex)
        {
            await DisplayAlert("❌ Erro", $"Erro ao criar chamado: {ex.Message}", "OK");
        }
        finally
        {
            // Reabilitar botão
            EnviarButton.IsEnabled = true;
            EnviarButton.Text = "🚀 Enviar chamado";
        }
    }

    private async void OnCancelarClicked(object sender, EventArgs e)
    {
        var result = await DisplayAlert("❌ Cancelar", "Deseja realmente cancelar? Todos os dados serão perdidos.", "Sim", "Não");
        
        if (result)
        {
            // Limpar todos os campos
            DescricaoEditor.Text = "";
            PrioridadePicker.SelectedIndex = 0;
            CategoriaPicker.SelectedIndex = 0;
            
            // Limpar preferências
            Preferences.Clear();
            
            // Voltar para login
            var shell = Shell.Current as AppShell;
            if (shell != null)
            {
                await shell.Logout();
            }
        }
    }
}
