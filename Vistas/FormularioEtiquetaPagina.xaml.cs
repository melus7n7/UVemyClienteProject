using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UVemyCliente.Conexion;
using UVemyCliente.DTO;
using UVemyCliente.Utilidades;

namespace UVemyCliente.Vistas
{
    /// <summary>
    /// Interaction logic for FormularioEtiquetaPagina.xaml
    /// </summary>
    public partial class FormularioEtiquetaPagina : Page
    {
        private string _nombre = string.Empty;
        public FormularioEtiquetaPagina()
        {
            InitializeComponent();
        }

        private void ClicRegistrar(object sender, RoutedEventArgs e)
        {
            if (ValidarCampos())
            {
                ContinuarRegistroAsync();
            }
        }

        private bool ValidarCampos()
        {
            _nombre = txtBoxNombre.Text.Trim();

            bool sonValidos = true;
            string mensajeError = string.Empty;

            if (string.IsNullOrEmpty(_nombre))
            {
                mensajeError = "Nombre requerido";
                sonValidos = false;
            }

            if (!sonValidos)
            {
                ErrorMensaje errorMensaje = new(mensajeError);
                errorMensaje.ShowDialog();
            }

            return sonValidos;
        }

        private async void ContinuarRegistroAsync()
        {
            btnRegistrar.IsEnabled = false;
            btnRegresar.IsEnabled = false;

            EtiquetaDTO etiqueta = new EtiquetaDTO()
            {
                Nombre = _nombre
            };

            var json = JsonSerializer.Serialize(etiqueta);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestAsync(HttpMethod.Post, "etiquetas", content);
            int codigoRespuesta = (int)respuestaHttp.StatusCode;


            if (codigoRespuesta == 201)
            {
                var jsonString = await respuestaHttp.Content.ReadAsStringAsync();
                EtiquetaDTO? etiquetaNueva = JsonSerializer.Deserialize<EtiquetaDTO>(jsonString);
                if (etiquetaNueva != null)
                {
                    ExitoMensaje exitoMensaje = new ExitoMensaje("Se ha registrado la nueva etiqueta");
                    exitoMensaje.ShowDialog();
                    RedirigirEtiquetasExistentes();
                }
            }
            else
            {
                ErrorMensaje error = new ErrorMensaje("Ocurrió un error inesperado y no se pudo guardar la etiqueta.");
                error.ShowDialog();
                RedirigirMenuPrincipal();
            }

        }

        private void RedirigirMenuPrincipal()
        {
            MenuPrincipalAdministradorPagina menuPrincipalAdministradorPagina = new MenuPrincipalAdministradorPagina();
            NavigationService.Navigate(menuPrincipalAdministradorPagina);
        }

        private void RedirigirEtiquetasExistentes()
        {
            EtiquetasExistentesPagina etiquetasExistentesPagina = new EtiquetasExistentesPagina();
            NavigationService.Navigate(etiquetasExistentesPagina);
        }


        private void ClicRegresar(object sender, RoutedEventArgs e)
        {
            this.NavigationService.GoBack();
        }
    }
}
