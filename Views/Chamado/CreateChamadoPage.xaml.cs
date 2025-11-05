using ProjetoSuporteTI.Services;
using ProjetoSuporteTI.Models;

namespace ProjetoSuporteTI.Views.Chamado;

public partial class CreateChamadoPage : ContentPage
{
    private readonly ApiService _apiService;

    public CreateChamadoPage()
    {
        InitializeComponent();
        _apiService = ApiService.Instance; // Usar Singleton para manter o usuário logado
        
        // Tentar restaurar usuário das preferências se necessário
        _apiService.RestoreUserFromPreferences();
        
        // Debug: Verificar status do usuário
        Console.WriteLine($"🔍 Status do usuário na página de chamados: {_apiService.GetUserInfo()}");
        
        // Definir valores padrão
        PrioridadePicker.SelectedIndex = 0; // Baixo
        DispositivoPicker.SelectedIndex = 4; // Outros
    }

    private async void OnEnviarClicked(object sender, EventArgs e)
    {
        // PRIMEIRA VERIFICAÇÃO: Usuário logado
        if (!_apiService.IsUserLoggedIn)
        {
            await DisplayAlert("❌ Erro de Autenticação", 
                "Usuário não está logado. Retornando para a tela de login...", 
                "OK");
            
            Console.WriteLine("🔍 DEBUG: Usuário não logado, redirecionando...");
            await Shell.Current.GoToAsync("//login");
            return;
        }
        
        Console.WriteLine($"🔍 DEBUG no envio: {_apiService.GetUserInfo()}");
        
        var descricao = DescricaoEditor.Text?.Trim();
        var prioridade = PrioridadePicker.SelectedItem?.ToString();
        var dispositivo = DispositivoPicker.SelectedItem?.ToString();

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

        if (string.IsNullOrWhiteSpace(dispositivo))
        {
            await DisplayAlert("❌ Erro", "Por favor, selecione o dispositivo!", "OK");
            return;
        }

        // Desabilitar botão durante processamento
        EnviarButton.IsEnabled = false;
        EnviarButton.Text = "🔄 Enviando...";

        try
        {
            // Criar título baseado no dispositivo e prioridade
            var titulo = $"[{dispositivo}] {(descricao.Length > 50 ? descricao.Substring(0, 47) + "..." : descricao)}";

            // Mapear prioridade texto → número
            var prioridadeNumero = prioridade switch
            {
                "Baixo" => 1,
                "Médio" => 2, 
                "Alto" => 3,
                _ => 2 // Padrão: Médio
            };

            Console.WriteLine($"🎯 Criando chamado via API:");
            Console.WriteLine($"   📝 Título: {titulo}");
            Console.WriteLine($"   📄 Descrição: {descricao}");
            Console.WriteLine($"   ⚡ Prioridade: {prioridade} ({prioridadeNumero})");
            Console.WriteLine($"   � Dispositivo: {dispositivo}");

            // Enviar para a API
            var result = await _apiService.CreateChamadoAsync(titulo, descricao, prioridade, dispositivo);

            if (result.Success)
            {
                Console.WriteLine($"✅ Chamado criado com sucesso! ID: {result.ChamadoId}");

                // Salvar dados para a IA (opcional, para continuar o fluxo existente)
                Preferences.Set("chamado_descricao", descricao);
                Preferences.Set("chamado_prioridade", prioridade);
                Preferences.Set("chamado_dispositivo", dispositivo);
                Preferences.Set("chamado_titulo", titulo);
                Preferences.Set("chamado_id", result.ChamadoId.ToString());
                Preferences.Set("chamado_created_at", DateTime.Now.ToString());

                // Tentar buscar os chamados para confirmar
                try
                {
                    var chamados = await _apiService.GetChamadosAsync();
                    var chamadoRecemCriado = chamados.FirstOrDefault(c => c.Id == result.ChamadoId);
                    
                    string detalhesConfirmacao = result.ChamadoId > 0 ? 
                        $"✅ CONFIRMADO NO BANCO!\n\n📋 ID do Chamado: {result.ChamadoId}" : 
                        "✅ Enviado para API (ID não disponível)";
                    
                    if (chamadoRecemCriado != null)
                    {
                        detalhesConfirmacao += $"\n🔍 Status no banco: {chamadoRecemCriado.Status}";
                    }

                    await DisplayAlert("✅ Sucesso", 
                        $"{detalhesConfirmacao}\n\n" +
                        $"📝 Título: {titulo}\n" +
                        $"� Dispositivo: {dispositivo}\n" +
                        $"⚡ Prioridade: {prioridade}\n\n" +
                        $"Agora você pode verificar no banco de dados!\n" +
                        $"A IA também irá tentar resolver seu problema.", 
                        "Continuar");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Erro ao verificar chamados: {ex.Message}");
                    
                    await DisplayAlert("✅ Sucesso", 
                        $"Chamado criado com sucesso!\n\n" +
                        $"🎫 ID: {(result.ChamadoId > 0 ? result.ChamadoId.ToString() : "N/A")}\n" +
                        $" Título: {titulo}\n" +
                        $"� Dispositivo: {dispositivo}\n" +
                        $"⚡ Prioridade: {prioridade}\n\n" +
                        $"✅ Verificar no banco de dados!\n" +
                        $"A IA irá tentar resolver seu problema.", 
                        "Continuar");
                }

            // Limpar campos
            DescricaoEditor.Text = "";
            PrioridadePicker.SelectedIndex = 0;
            DispositivoPicker.SelectedIndex = 4;

                // Navegar para a IA
                await Shell.Current.GoToAsync("//support");
            }
            else
            {
                Console.WriteLine($"❌ Erro ao criar chamado: {result.Message}");
                await DisplayAlert("❌ Erro", $"Não foi possível criar o chamado:\n\n{result.Message}", "OK");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 Exceção ao criar chamado: {ex.Message}");
            await DisplayAlert("❌ Erro", $"Erro inesperado ao criar chamado:\n\n{ex.Message}", "OK");
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
            DispositivoPicker.SelectedIndex = 4;
            
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
