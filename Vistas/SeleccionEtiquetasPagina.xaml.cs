using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using UVemyCliente.Conexion;
using UVemyCliente.DTO;
using UVemyCliente.Utilidades;
using System.Collections.Generic;
using System.Windows.Controls.Primitives;
using System.Text;
using System.Diagnostics;

namespace UVemyCliente.Vistas
{
    public partial class SeleccionEtiquetasPagina : Page
    {
        private ObservableCollection<EtiquetaDTO> _etiquetas = [];
        private UsuarioDTO _usuario;
        private bool _esFormularioCurso;
        private List<string> _listNombreEtiquetas = new List<string>() { };
        private List<string> _listNombreEtiquetasAntiguas = new List<string>() { };
        private List<int> _listIdEtiquetas = new List<int>();
        private List<int> _listIdEtiquetasAntiguas = new List<int>();
        private CursoDTO _curso;
        private DocumentoDTO _documento;
        public SeleccionEtiquetasPagina(UsuarioDTO usuario)
        {
            InitializeComponent();
            _usuario = usuario;
        }

        public SeleccionEtiquetasPagina(List<int> listIdEtiquetas, List<string> listNombreEtiquetas, CursoDTO curso, DocumentoDTO documento)
        {
            InitializeComponent();
            _curso = curso;
            _documento = documento;
            _listIdEtiquetasAntiguas = new List<int>(listIdEtiquetas);
            _listNombreEtiquetasAntiguas = new List<string>(listNombreEtiquetas);

            _listIdEtiquetas = listIdEtiquetas;
            _listNombreEtiquetas = listNombreEtiquetas;
            _ = CargarEtiquetasAsync();
        }

        private void CargarPagina(object sender, RoutedEventArgs e)
        {
            _esFormularioCurso = true;
            _ = CargarEtiquetasAsync();
        }

        private async Task CargarEtiquetasAsync()
        {
            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestSinAutenticacionAsync(HttpMethod.Get, "etiquetas");
            if (respuestaHttp.IsSuccessStatusCode)
            {
                var json = await respuestaHttp.Content.ReadAsStringAsync();
                _etiquetas = JsonSerializer.Deserialize<ObservableCollection<EtiquetaDTO>>(json) ?? [];
                itmControlEtiquetas.ItemsSource = _etiquetas;
            }
            else
            {
                ErrorMensaje errorMensaje = new("Error. No se pudo conectar con el servidor. Inténtelo de nuevo o hágalo más tarde.");
                errorMensaje.Show();

                //TODO: Regresar a menú principal o a FormularioUsuarioPagina, yo tambien lo ocupo en formulario curso, entonces hay que ponernos de acuerdo
            }
        }

        private void SeleccionarEtiqueta(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton toggleButton && toggleButton.DataContext is EtiquetaDTO etiqueta)
            {
                if (_esFormularioCurso)
                {
                    if (!_listIdEtiquetas.Contains(etiqueta.IdEtiqueta))
                    {
                        _listIdEtiquetas.Add(etiqueta.IdEtiqueta);
                        _listNombreEtiquetas.Add(etiqueta.Nombre);
                    }
                }
                else
                {
                    _usuario.IdsEtiqueta ??= [];
                    _usuario.IdsEtiqueta.Add(etiqueta.IdEtiqueta);
                }

            }
        }

        private void DeseleccionarEtiqueta(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton toggleButton && toggleButton.DataContext is EtiquetaDTO etiqueta)
            {
                if (_esFormularioCurso)
                {
                    if (_listIdEtiquetas.Contains(etiqueta.IdEtiqueta))
                    {
                        _listIdEtiquetas.Remove(etiqueta.IdEtiqueta);
                        _listNombreEtiquetas.Remove(etiqueta.Nombre);
                    }
                }
                else
                {
                    _usuario.IdsEtiqueta?.Remove(etiqueta.IdEtiqueta);
                }
            }
        }

        private void ClicConfirmar(object sender, RoutedEventArgs e)
        {
            if (_esFormularioCurso)
            {
                List<EtiquetaDTO> etiquetas = CrearListaEtiquetas(_listNombreEtiquetas, _listIdEtiquetas);
                FormularioCursoPagina pagina = new FormularioCursoPagina(_curso, etiquetas, _documento);
                this.NavigationService.Navigate(pagina);
            }
            else
            {
                if (_usuario.IdsEtiqueta?.Count > 0)
                {
                    _ = SolicitarCodigoVerificacion();
                }
                else
                {
                    ErrorMensaje errorMensaje = new("Debes seleccionar al menos una etiqueta");
                    errorMensaje.Show();
                }
            }
        }

        private async Task SolicitarCodigoVerificacion()
        {
            var json = JsonSerializer.Serialize(_usuario);
            var contenido = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestSinAutenticacionAsync(HttpMethod.Post, "autenticacion/verificacion", contenido);
            if (respuestaHttp.IsSuccessStatusCode)
            {
                var jsonString = await respuestaHttp.Content.ReadAsStringAsync();
                if (JsonSerializer.Deserialize<UsuarioDTO>(jsonString) != null)
                {
                    SingletonUsuario.JWT = JsonSerializer.Deserialize<UsuarioDTO>(jsonString).Token;
                    CodigoVerificacionPagina codigoVerificacionPagina = new(_usuario);
                    this.NavigationService.Navigate(codigoVerificacionPagina);
                }
            }
            else
            {
                var jsonString = await respuestaHttp.Content.ReadAsStringAsync();
                UsuarioDTO? errorJson = JsonSerializer.Deserialize<UsuarioDTO>(jsonString);

                string[] detalles = errorJson?.Detalles.ToArray() ?? ["Error desconocido"];
                string detallesConcatenados = string.Join(", ", detalles);
                ErrorMensaje errorMensaje = new(detallesConcatenados + ". Verifique la información e intentélo de nuevo más tarde");
                errorMensaje.Show();
            }
        }

        private void ClicRegresar(object sender, RoutedEventArgs e)
        {
            if (_esFormularioCurso)
            {
                List<EtiquetaDTO> etiquetas = CrearListaEtiquetas(_listNombreEtiquetasAntiguas, _listIdEtiquetasAntiguas);
                FormularioCursoPagina pagina = new FormularioCursoPagina(_curso, etiquetas, _documento);
                this.NavigationService.Navigate(pagina);
            }
            else
            {
                this.NavigationService.GoBack();
            }
        }

        private void CargarEtiquetas(object sender, RoutedEventArgs e)
        {
            if(_listIdEtiquetas != null && _listIdEtiquetas.Count > 0)
            {
                ToggleButton toggleButton = sender as ToggleButton;
                if (toggleButton != null && toggleButton.DataContext is EtiquetaDTO etiqueta)
                {
                    if (_listIdEtiquetas.Contains(etiqueta.IdEtiqueta))
                    {
                        toggleButton.IsChecked = true;
                    }
                }
            }
        }

        private List<EtiquetaDTO> CrearListaEtiquetas(List<string> nombres, List<int> ids)
        {
            List<EtiquetaDTO> etiquetas = new List<EtiquetaDTO>();
            if (ids.Count > 0)
            {
                for (int i = 0; i < nombres.Count; i++)
                {
                    EtiquetaDTO etiqueta = new EtiquetaDTO
                    {
                        Nombre = nombres[i],
                        IdEtiqueta = ids[i]
                    };

                    etiquetas.Add(etiqueta);
                }
            }
            return etiquetas;
        }
    }
}
