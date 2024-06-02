using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using UVemyCliente.Conexion;
using UVemyCliente.DTO;
using UVemyCliente.Utilidades;

namespace UVemyCliente.Vistas
{
    public partial class EtiquetasExistentesPagina : Page
    {
        private ObservableCollection<EtiquetaDTO> _etiquetas = new ObservableCollection<EtiquetaDTO>();

        public EtiquetasExistentesPagina()
        {
            InitializeComponent();
            _ = CargarEtiquetasAsync();
        }

        private async Task CargarEtiquetasAsync()
        {
            btnRegresar.IsEnabled = false;
            btnRegistrar.IsEnabled = false;


            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestAsync(HttpMethod.Get, "etiquetas");
            if (respuestaHttp.IsSuccessStatusCode)
            {
                var json = await respuestaHttp.Content.ReadAsStringAsync();
                _etiquetas = JsonSerializer.Deserialize<ObservableCollection<EtiquetaDTO>>(json) ?? new ObservableCollection<EtiquetaDTO>();
                itmControlEtiquetas.ItemsSource = _etiquetas;
                btnRegresar.IsEnabled = true;
                btnRegistrar.IsEnabled = true;

            }
            else
            {
                ErrorMensaje errorMensaje = new("Error. No se pudo conectar con el servidor. Inténtelo de nuevo o hágalo más tarde.");
                errorMensaje.Show();
                NavigationService.GoBack();
            }
        }


        private void ClicRegresar(object sender, RoutedEventArgs e)
        {
            this.NavigationService.GoBack();
        }

        private void ClicRegistrar(object sender, RoutedEventArgs e)
        {
            FormularioEtiquetaPagina formularioEtiquetaPagina = new();
            this.NavigationService.Navigate(formularioEtiquetaPagina);
        }
    }
}
