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
    private int _chamadoId = 0; // ID do chamado atual

    public SupportPage()
    {
        InitializeComponent();
        _apiService = ApiService.Instance; // Usar Singleton
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Recuperar dados do chamado
        _chamadoDescricao = Preferences.Get("chamado_descricao", "");
        _chamadoPrioridade = Preferences.Get("chamado_prioridade", "");
        _chamadoCategoria = Preferences.Get("chamado_dispositivo", "");

        // Carregar o chamado já criado (não criar outro)
        CarregarChamadoExistente();

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
            
            // Chamado já foi criado no CreateChamadoPage, apenas carregar o ID
            CarregarChamadoExistente();
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

    private void CarregarChamadoExistente()
    {
        try
        {
            // Recuperar ID do chamado já criado
            var chamadoIdStr = Preferences.Get("chamado_id", "0");
            if (int.TryParse(chamadoIdStr, out int chamadoId) && chamadoId > 0)
            {
                _chamadoId = chamadoId;
                Console.WriteLine($"✅ Chamado carregado: ID {_chamadoId}");
            }
            else
            {
                Console.WriteLine("⚠️ ID do chamado não encontrado nas preferências");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao carregar chamado: {ex.Message}");
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

    private async void OnResolvidoIAClicked(object sender, EventArgs e)
    {
        try
        {
            if (_chamadoId <= 0)
            {
                await DisplayAlert("❌ Erro", "ID do chamado não encontrado.", "OK");
                return;
            }

            var confirm = await DisplayAlert("🤖 Resolvido pela IA", 
                "Confirma que a IA resolveu seu problema?\n\n" +
                "✅ O chamado será FINALIZADO como resolvido pela IA\n" +
                "🔄 Você será deslogado agora", 
                "✅ Sim, resolvido!", "❌ Cancelar");

            if (confirm)
            {
                ResolvidoIAButton.IsEnabled = false;
                ResolvidoIAButton.Text = "🔄 Finalizando...";

                bool success = await _apiService.MarcarComoResolvidoPorIAAsync(_chamadoId);

                if (success)
                {
                    await DisplayAlert("✅ Problema Resolvido!", 
                        "🎉 Chamado finalizado com sucesso!\n\n" +
                        "✅ Status: Resolvido pela IA\n" +
                        "🤖 Obrigado por usar nosso sistema inteligente!\n\n" +
                        "Você será deslogado agora.", 
                        "OK");

                    // Limpar dados e fazer logout
                    Preferences.Clear();
                    
                    // Voltar para tela de login
                    var shell = Shell.Current as AppShell;
                    if (shell != null)
                    {
                        await shell.Logout();
                    }
                }
                else
                {
                    await DisplayAlert("❌ Erro", 
                        "Não foi possível finalizar o chamado. Tente novamente.", 
                        "OK");
                    
                    ResolvidoIAButton.IsEnabled = true;
                    ResolvidoIAButton.Text = "🤖 Resolvido pela IA";
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 Erro ao finalizar chamado pela IA: {ex.Message}");
            await DisplayAlert("❌ Erro", $"Erro inesperado: {ex.Message}", "OK");
            
            ResolvidoIAButton.IsEnabled = true;
            ResolvidoIAButton.Text = "🤖 Resolvido pela IA";
        }
    }

    private async void OnPrecisaSuporteClicked(object sender, EventArgs e)
    {
        try
        {
            var confirm = await DisplayAlert("🆘 Encaminhar para Suporte", 
                "Confirma que precisa do suporte técnico humano?\n\n" +
                "O chamado permanecerá ABERTO para o suporte resolver.\n" +
                "Você será deslogado agora.", 
                "✅ Sim, preciso!", "❌ Cancelar");

            if (confirm)
            {
                PrecisaSuporteButton.IsEnabled = false;
                PrecisaSuporteButton.Text = "🔄 Encaminhando...";

                await DisplayAlert("🎯 Encaminhado para Suporte", 
                    "📋 Chamado encaminhado para o suporte técnico!\n\n" +
                    "✅ Status: Aberto (aguardando suporte)\n" +
                    "🕐 Nossa equipe entrará em contato em breve\n" +
                    "⏱️ Tempo médio de resposta: 2-4 horas úteis\n\n" +
                    "Você será deslogado agora.", 
                    "OK");

                // Limpar dados e fazer logout (sem finalizar o chamado)
                Preferences.Clear();
                
                // Voltar para tela de login
                var shell = Shell.Current as AppShell;
                if (shell != null)
                {
                    await shell.Logout();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 Erro ao encaminhar para suporte: {ex.Message}");
            await DisplayAlert("❌ Erro", $"Erro inesperado: {ex.Message}", "OK");
            
            PrecisaSuporteButton.IsEnabled = true;
            PrecisaSuporteButton.Text = "🆘 Suporte vai resolver";
        }
    }
}
