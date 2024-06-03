using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
                errorMensaje.ShowDialog();
                RedirigirMenuPrincipal();
            }
        }


        private void ClicRegresar(object sender, RoutedEventArgs e)
        {
            RedirigirMenuPrincipal();
        }

        private void ClicRegistrar(object sender, RoutedEventArgs e)
        {
            FormularioEtiquetaPagina formularioEtiquetaPagina = new();
            this.NavigationService.Navigate(formularioEtiquetaPagina);
        }

        private async void ClicEliminarEtiqueta(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                var idEtiqueta = (int)button.Tag;

                var result = MessageBox.Show("¿Estás seguro de que quieres eliminar esta etiqueta?", "Confirmación de eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    await EliminarEtiquetaAsync(idEtiqueta);
                }
            }
        }

        private async Task EliminarEtiquetaAsync(int idEtiqueta)
        {
            DeshabilitarBotones();

            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestAsync(HttpMethod.Delete, $"etiquetas/{idEtiqueta}");
            if (respuestaHttp.IsSuccessStatusCode)
            {
                ExitoMensaje exitoMensaje = new ("Etiqueta eliminada correctamente.");
                _ = CargarEtiquetasAsync();
                exitoMensaje.ShowDialog();
            }
            else
            {
                ErrorMensaje errorMensaje = new("Error al eliminar la etiqueta. Intente de nuevo o hágalo más tarde.");
                errorMensaje.ShowDialog();
                RedirigirMenuPrincipal();
            }
        }

        private void DeshabilitarBotones()
        {
            btnRegistrar.IsEnabled = false;
            btnRegresar.IsEnabled = false;

            foreach (var item in itmControlEtiquetas.Items)
            {
                var container = itmControlEtiquetas.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                if (container != null)
                {
                    var button = FindVisualChild<Button>(container);
                    if (button != null)
                    {
                        button.IsEnabled = false;
                    }
                }
            }
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child != null && child is T t)
                {
                    return t;
                }
                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                {
                    return childOfChild;
                }
            }
            return null;
        }

        private void RedirigirMenuPrincipal()
        {
            MenuPrincipalAdministradorPagina menuPrincipalAdministradorPagina = new MenuPrincipalAdministradorPagina();
            NavigationService.Navigate(menuPrincipalAdministradorPagina);
        }
    }
}
