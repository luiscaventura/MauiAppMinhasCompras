using MauiAppMinhasCompras.Models;
using System.Collections.ObjectModel;

namespace MauiAppMinhasCompras.Views;

public partial class ListaProduto : ContentPage
{
    ObservableCollection<Produto> lista = new ObservableCollection<Produto>();
    private string categoriaSelecionada = "Todos";

    public ListaProduto()
    {
        InitializeComponent();
        lst_produtos.ItemsSource = lista;
    }

    protected async override void OnAppearing()
    {
        try
        {
            await CarregarCategorias();
            await CarregarProdutos();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ops", ex.Message, "OK");
        }
    }

    private async Task CarregarCategorias()
    {
        try
        {
            List<string> categorias = await App.Db.GetCategorias();
            pickerCategoria.Items.Clear();

            pickerCategoria.Items.Add("Todos");

            foreach (var categoria in categorias)
            {
                pickerCategoria.Items.Add(categoria);
            }

            pickerCategoria.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao carregar categorias: {ex.Message}", "OK");
        }
    }

    private async Task CarregarProdutos()
    {
        lista.Clear();
        List<Produto> tmp;

        if (categoriaSelecionada == "Todos")
        {
            tmp = await App.Db.GetAll();
        }
        else
        {
            tmp = await App.Db.GetByCategoria(categoriaSelecionada);
        }

        tmp.ForEach(i => lista.Add(i));
    }

    private void ToolbarItem_Clicked(object sender, EventArgs e)
    {
        try
        {
            Navigation.PushAsync(new Views.NovoProduto());
        }
        catch (Exception ex)
        {
            DisplayAlert("Ops", ex.Message, "OK");
        }
    }

    private async void txt_search_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            string q = e.NewTextValue;
            lst_produtos.IsRefreshing = true;
            lista.Clear();

            List<Produto> tmp;

            if (categoriaSelecionada == "Todos")
            {
                tmp = await App.Db.Search(q);
            }
            else
            {
                tmp = await App.Db.SearchByDescricaoECategoria(q, categoriaSelecionada);
            }

            tmp.ForEach(i => lista.Add(i));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ops", ex.Message, "OK");
        }
        finally
        {
            lst_produtos.IsRefreshing = false;
        }
    }

    private async void ToolbarItem_Clicked_1(object sender, EventArgs e)
    {
        double somaGeral = lista.Sum(i => i.Total);
        var totaisPorCategoria = await App.Db.GetTotalPorCategoria();

        string mensagem = $"O total é {somaGeral:C}\n\n";

        foreach (var item in totaisPorCategoria)
        {
            mensagem += $"{item.Key}: {item.Value:C}\n";
        }

        await DisplayAlert("Total dos Produtos", mensagem, "OK");
    }


    private async void MenuItem_Clicked(object sender, EventArgs e)
    {
        try
        {
            MenuItem selecionado = sender as MenuItem;
            Produto p = selecionado.BindingContext as Produto;

            bool confirm = await DisplayAlert("Tem Certeza?", $"Remover {p.Descricao}?", "Sim", "Não");

            if (confirm)
            {
                await App.Db.Delete(p.Id);
                lista.Remove(p);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ops", ex.Message, "OK");
        }
    }

    private void lst_produtos_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        try
        {
            Produto p = e.SelectedItem as Produto;
            Navigation.PushAsync(new Views.EditarProduto
            {
                BindingContext = p,
            });
        }
        catch (Exception ex)
        {
            DisplayAlert("Ops", ex.Message, "OK");
        }
    }

    private async void lst_produtos_Refreshing(object sender, EventArgs e)
    {
        try
        {
            await CarregarProdutos();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ops", ex.Message, "OK");
        }
        finally
        {
            lst_produtos.IsRefreshing = false;
        }
    }

    private async void OnCategoriaSelecionada(object sender, EventArgs e)
    {
        try
        {
            var picker = sender as Picker;
            if (picker.SelectedItem != null)
            {
                categoriaSelecionada = picker.SelectedItem.ToString();
                await CarregarProdutos();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ops", ex.Message, "OK");
        }
    }
}
