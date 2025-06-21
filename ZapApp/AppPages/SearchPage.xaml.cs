using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using ZapApp.AppResources;
using Microsoft.EntityFrameworkCore;


namespace ZapApp.AppPages
{
    public partial class SearchPage : ContentPage
    {
        public SearchPage()
        {
            InitializeComponent();
        }

        private async void OnPesquisarClicked(object sender, EventArgs e)
        {
            string cns = EntryCns.Text?.Trim();

            if (string.IsNullOrEmpty(cns))
            {
                await DisplayAlert("Atenção", "Digite um número de CNS.", "OK");
                return;
            }

            using var db = new AppDbContext();

            var resultados = await db.Registros
                                     .Where(r => r.CNS == cns)
                                     .OrderByDescending(r => r.Data_Agenda)
                                     .ToListAsync();

            if (resultados.Count == 0)
            {
                await DisplayAlert("Nenhum resultado", "Nenhum registro encontrado para o CNS informado.", "OK");
                ResultadosCollection.ItemsSource = null;
                return;
            }

            // Opcional: formatar resultado como string ou usar DataTemplate customizado
            ResultadosCollection.ItemsSource = resultados
                .Select(r => $"Procedimento: {r.Procedimento} - Tipo: {r.Local} - Data: {r.Data_Agenda:dd/MM/yyyy} - Telefone: {r.Telefone} - Enviado: {r.Enviado} - Data Envio: {r.Data_Zap:dd/MM/yyyy}")
                .ToList();
        }
    }
}
