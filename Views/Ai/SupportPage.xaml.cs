using ProjetoSuporteTI.Services;
using ProjetoSuporteTI.Models;
using Microsoft.Maui.Controls.Shapes;

namespace ProjetoSuporteTI.Views.Ai;

public partial class SupportPage : ContentPage
{
    private readonly ApiService _apiService;
    private bool _iaResolveuProblema = false;
    private string _chamadoDescricao = "";
    private string _chamadoPrioridade = "";
    private string _chamadoCategoria = "";

    public SupportPage()
    {
        InitializeComponent();
        _apiService = new ApiService();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Recuperar dados do chamado
        _chamadoDescricao = Preferences.Get("chamado_descricao", "");
        _chamadoPrioridade = Preferences.Get("chamado_prioridade", "");
        _chamadoCategoria = Preferences.Get("chamado_categoria", "");

        // Simular análise da IA
        await SimularAnaliseIA();
    }

    private async Task SimularAnaliseIA()
    {
        await Task.Delay(2000); // Simular processamento

        // Análise baseada na categoria e descrição
        var respostas = new List<string>();
        
        if (_chamadoCategoria.ToLower().Contains("impressora"))
        {
            respostas.Add("✅ Verifique se a impressora está conectada");
            respostas.Add("✅ Reinicie o serviço de spooler");
            respostas.Add("✅ Verifique se há papel e tinta");
            _iaResolveuProblema = true;
        }
        else if (_chamadoCategoria.ToLower().Contains("hardware") || _chamadoDescricao.ToLower().Contains("computador"))
        {
            respostas.Add("⚠️ Problema de hardware detectado");
            respostas.Add("⚠️ Recomendo verificação presencial");
            _iaResolveuProblema = false;
        }
        else if (_chamadoCategoria.ToLower().Contains("rede"))
        {
            respostas.Add("✅ Reinicie o roteador");
            respostas.Add("✅ Verifique os cabos de rede");
            respostas.Add("✅ Execute ipconfig /release e /renew");
            _iaResolveuProblema = true;
        }
        else
        {
            respostas.Add("⚠️ Problema complexo identificado");
            respostas.Add("⚠️ Necessário suporte técnico especializado");
            _iaResolveuProblema = false;
        }

        // Mostrar respostas da IA
        foreach (var resposta in respostas)
        {
            AdicionarMensagemIA(resposta);
            await Task.Delay(1000);
        }

        // Mensagem final
        if (_iaResolveuProblema)
        {
            AdicionarMensagemIA("✅ Problema resolvido automaticamente pela IA!");
            AdicionarMensagemIA("Se o problema persistir, entre em contato novamente.");
        }
        else
        {
            AdicionarMensagemIA("⚠️ Não conseguimos resolver automaticamente.");
            AdicionarMensagemIA("📋 Chamado encaminhado para suporte técnico.");
            
            // Salvar como chamado oficial
            await SalvarChamadoOficial();
        }
    }

    private void AdicionarMensagemIA(string mensagem)
    {
        var border = new Border
        {
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#bdc3c7"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
            Padding = 15,
            HorizontalOptions = LayoutOptions.Start,
            MaximumWidthRequest = 300,
            Margin = new Thickness(0, 5)
        };

        var stackLayout = new StackLayout();
        
        var labelBot = new Label
        {
            Text = "🤖 Assistente IA",
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#2c3e50")
        };

        var labelMessage = new Label
        {
            Text = mensagem,
            FontSize = 14,
            TextColor = Color.FromArgb("#2c3e50"),
            LineBreakMode = LineBreakMode.WordWrap
        };

        stackLayout.Children.Add(labelBot);
        stackLayout.Children.Add(labelMessage);
        border.Content = stackLayout;

        ChatStackLayout.Children.Add(border);
        
        // Scroll para o final usando Dispatcher
        Dispatcher.Dispatch(async () =>
        {
            await Task.Delay(100);
            await ChatScrollView.ScrollToAsync(0, ChatStackLayout.Height, true);
        });
    }

    private void AdicionarMensagemUsuario(string mensagem)
    {
        var border = new Border
        {
            BackgroundColor = Color.FromArgb("#3498db"),
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
            Padding = 15,
            HorizontalOptions = LayoutOptions.End,
            MaximumWidthRequest = 300,
            Margin = new Thickness(0, 5)
        };

        var label = new Label
        {
            Text = mensagem,
            FontSize = 14,
            TextColor = Colors.White,
            LineBreakMode = LineBreakMode.WordWrap
        };

        border.Content = label;
        ChatStackLayout.Children.Add(border);
        
        // Scroll para o final usando Dispatcher
        Dispatcher.Dispatch(async () =>
        {
            await Task.Delay(100);
            await ChatScrollView.ScrollToAsync(0, ChatStackLayout.Height, true);
        });
    }

    private async Task SalvarChamadoOficial()
    {
        try
        {
            var chamado = new Models.Chamado
            {
                Titulo = $"{_chamadoCategoria} - {_chamadoPrioridade}",
                Descricao = _chamadoDescricao,
                Status = "Em Andamento",
                Prioridade = _chamadoPrioridade,
                DataCriacao = DateTime.Now,
                UsuarioId = int.Parse(Preferences.Get("user_id", "1"))
            };

            await _apiService.CreateChamadoAsync(chamado);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao salvar chamado: {ex.Message}");
        }
    }

    private async void OnSendClicked(object sender, EventArgs e)
    {
        var mensagem = MessageEntry.Text?.Trim();
        
        if (string.IsNullOrWhiteSpace(mensagem))
            return;

        // Adicionar mensagem do usuário
        AdicionarMensagemUsuario(mensagem);
        MessageEntry.Text = "";

        // Simular resposta da IA
        await Task.Delay(1000);
        AdicionarMensagemIA("Obrigado pelo feedback! Sua mensagem foi registrada.");
    }

    private async void OnConfigClicked(object sender, EventArgs e)
    {
        ConfigOverlay.IsVisible = false;
        await DisplayAlert("⚙️ Configuração", "Funcionalidade em desenvolvimento", "OK");
    }

    private async void OnSairClicked(object sender, EventArgs e)
    {
        ConfigOverlay.IsVisible = false;
        
        var result = await DisplayAlert("🚪 Sair", "Deseja fazer logout?", "Sim", "Não");
        
        if (result)
        {
            var shell = Shell.Current as AppShell;
            if (shell != null)
                await shell.Logout();
        }
    }

    private async void OnVoltarClicked(object sender, EventArgs e)
    {
        var result = await DisplayAlert("⬅️ Voltar", "Deseja voltar para criar um novo chamado?", "Sim", "Não");
        
        if (result)
        {
            // Limpar dados do chamado atual
            Preferences.Remove("chamado_descricao");
            Preferences.Remove("chamado_prioridade");
            Preferences.Remove("chamado_categoria");
            Preferences.Remove("chamado_created_at");
            
            // Voltar para criar chamado
            await Shell.Current.GoToAsync("//createchamado");
        }
    }

    private async void OnFinalizarClicked(object sender, EventArgs e)
    {
        var result = await DisplayAlert("✅ Finalizar", "Deseja finalizar o atendimento e fazer logout?", "Sim", "Não");
        
        if (result)
        {
            // Limpar todos os dados
            Preferences.Clear();
            
            await DisplayAlert("✅ Finalizado", "Atendimento finalizado com sucesso! Obrigado por usar nosso sistema.", "OK");
            
            // Voltar para login
            var shell = Shell.Current as AppShell;
            if (shell != null)
            {
                await shell.Logout();
            }
        }
    }
}
