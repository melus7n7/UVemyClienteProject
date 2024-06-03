using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
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
using static UVemyCliente.Vistas.ListaCursosPagina;

namespace UVemyCliente.Vistas
{
    /// <summary>
    /// Lógica de interacción para DetallesCursoPagina.xaml
    /// </summary>
    public partial class DetallesCurso : Page
    {
        private int _idCurso = 4;
        private CursoDTO _curso = new CursoDTO();
        private CursoDetalle _cursoDetalle = new CursoDetalle();
        ObservableCollection<ClaseListBox> _listaClases = new ObservableCollection<ClaseListBox>();
        Visibility _visibilidadBoton = Visibility.Visible;
        public DetallesCurso(CursoDTO curso)
        {
            _curso = curso;
            _idCurso = (int)_curso.IdCurso;
            InitializeComponent();
            _ = CargarCursoAsync();
        }

        private async Task CargarCursoAsync()
        {
            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestAsync(HttpMethod.Get, "cursos/"+ _idCurso);
            if (respuestaHttp.IsSuccessStatusCode)
            {
                var json = await respuestaHttp.Content.ReadAsStringAsync();
                _cursoDetalle = JsonConvert.DeserializeObject<CursoDetalle>(json);
                CargarCurso();
                CargarBotones();
            }
            else
            {
                ErrorMensaje errorMensaje = new("Error. No se pudo conectar con el servidor. Inténtelo de nuevo o hágalo más tarde.");
                errorMensaje.Show();
            }
        }

        private async Task CargarClasesAsync()
        {
            string urlBusqueda = "clases/curso/" + _idCurso;
            urlBusqueda += $"?rol={HttpUtility.UrlEncode(_cursoDetalle.Rol)}";
            Debug.WriteLine("url "+urlBusqueda);
            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestAsync(HttpMethod.Get, urlBusqueda);
            Debug.WriteLine(respuestaHttp.StatusCode + "Estatus code");
            Debug.WriteLine(_idCurso);
            Debug.WriteLine(respuestaHttp);
            Debug.WriteLine("clases/curso/" + _idCurso);
            if (respuestaHttp.IsSuccessStatusCode)
            {
                var json = await respuestaHttp.Content.ReadAsStringAsync();
                _listaClases = JsonConvert.DeserializeObject<ObservableCollection<ClaseListBox>>(json);
                if(_listaClases.Count > 0)
                {
                    CargarClases();
                }
            }
            else if (respuestaHttp.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                ErrorMensaje errorMensaje = new("Error. No existen clases para este curso.");
                errorMensaje.Show();
            }
            else
            {
                ErrorMensaje errorMensaje = new("Error. No se pudo conectar con el servidor. Inténtelo de nuevo o hágalo más tarde.");
                errorMensaje.Show();
            }
        }

        private void CargarClases()
        {
            for (int i = 0; i < _listaClases.Count; i++)
            {
                _listaClases[i].NumeroClase = i + 1;
                Debug.WriteLine("_visibilidadBoton" + _visibilidadBoton);
                _listaClases[i].Visibilidad = _visibilidadBoton;
            }
            lstBoxClases.ItemsSource = _listaClases;
        }

        private void CargarCurso()
        {
            Debug.WriteLine("_curso.Titulo " + _curso.Titulo);
            txtBlockTitulo.Text = _curso.Titulo;

            _curso.Objetivos = _cursoDetalle.Objetivos;
            txtBoxObjetivos.Text = _curso.Objetivos;
            _curso.Etiquetas = new List<int>();
            foreach (EtiquetaDTO etiqueta in _cursoDetalle.Etiquetas)
            {
                _curso.Etiquetas.Add(etiqueta.IdEtiqueta);
                txtBoxEtiquetas.Text += etiqueta.Nombre+" ";
            }

            _curso.Descripcion = _cursoDetalle.Descripcion;
            txtBoxDescripcion.Text = _curso.Descripcion;

            if (_cursoDetalle.Calificacion != null)
            {
                txtBoxCalificacion.Text = _cursoDetalle.Calificacion;
            } 
            else
            {
                txtBoxCalificacion.Text += "S/C";
            }
            txtBoxTProfesor.Text = _cursoDetalle.Profesor;

            _curso.Requisitos = _cursoDetalle.Requisitos;
            txtBoxRequisitos.Text = _curso.Requisitos;

            if (_curso.Archivo != null)
            {
                byte[] imageData = _curso.Archivo;
                BitmapImage bitmap = new BitmapImage();
                using (MemoryStream stream = new MemoryStream(imageData))
                {
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                }
                mgMiniatura.Source = bitmap;
            }
        }

        private void CargarBotones()
        {
            btnAgregarClase.Visibility = Visibility.Hidden;
            btnModificarCurso.Visibility = Visibility.Hidden;
            btnVerEstadisticas.Visibility = Visibility.Hidden;
            btnCalificarCurso.Visibility = Visibility.Hidden;
            btnInscribirse.Visibility = Visibility.Hidden;
            _visibilidadBoton = Visibility.Visible;
            switch (_cursoDetalle.Rol)
            {
                case "Profesor":
                    btnAgregarClase.Visibility = Visibility.Visible;
                    btnModificarCurso.Visibility = Visibility.Visible;
                    btnVerEstadisticas.Visibility = Visibility.Visible;
                    _visibilidadBoton = Visibility.Visible;
                    break;
                case "Estudiante":
                    btnCalificarCurso.Visibility = Visibility.Visible;
                    _visibilidadBoton = Visibility.Visible;
                    break;
                case "Usuario":
                    btnInscribirse.Visibility = Visibility.Visible;
                    _visibilidadBoton = Visibility.Hidden;
                    break;
                default:
                    break;
            }

            _ = CargarClasesAsync();
        }

        private void ClicAgregarClase(object sender, RoutedEventArgs e)
        {
            FormularioClase formulario = new FormularioClase(_curso);
            NavigationService.Navigate(formulario);
        }

        private void ClicVerClase(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                bool esProfesor = _cursoDetalle.Rol.Equals("Profesor");

                ClaseListBox clase = (ClaseListBox)btn.DataContext;
                DetallesClase detalles = new DetallesClase(_curso, clase.IdClase, esProfesor);
                NavigationService.Navigate(detalles);
            }
            
        }

        private void ClicVerEstadisticasCurso(object sender, RoutedEventArgs e)
        {
            EstadisticasCurso estadisticas = new EstadisticasCurso(_idCurso);
            NavigationService.Navigate(estadisticas);
        }

        private void ClicInscribirseAlCurso(object sender, RoutedEventArgs e)
        {
            string message = "¿Desea inscribirse al curso?";
            string title = "Inscripción al curso";
            MessageBoxButton buttons = MessageBoxButton.YesNo;
            MessageBoxResult result = MessageBox.Show(message, title, buttons);
            if (result == MessageBoxResult.Yes)
            {
                _ = InscribirseAlCursoAsync();
            }
        }

        private async Task InscribirseAlCursoAsync()
        {
            btnRegresar.IsEnabled = false;
            grdBackground.IsEnabled = false;

            UsuarioCursoDTO usuario = new UsuarioCursoDTO { IdCurso = _idCurso, IdUsuario = 4 };
            var json = System.Text.Json.JsonSerializer.Serialize(usuario);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            string url = "cursos/inscripcion/" + _idCurso;
            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestAsync(HttpMethod.Post, url, content);
            int codigoRespuesta = (int)respuestaHttp.StatusCode;

            if (codigoRespuesta >= 400)
            {
                Debug.WriteLine(codigoRespuesta);
                ErrorMensaje error = new ErrorMensaje("Ocurrió un error y no se pudo inscribir, inténtelo más tarde");
                error.Show();
            }
            else
            {
                ExitoMensaje mensaje = new ExitoMensaje("Se ha inscribido al curso exitosamente");
                mensaje.Show();
                DetallesCurso curso = new DetallesCurso(_curso);
                this.NavigationService.Navigate(curso);
            }

            btnRegresar.IsEnabled = true;
            grdBackground.IsEnabled = true;
        }

        private void ClicCalificarCurso(object sender, RoutedEventArgs e)
        {
            CalificacionCurso pagina = new CalificacionCurso(_curso);
            NavigationService.Navigate(pagina);
        }

        public class CursoDetalle
        {
            [JsonProperty("descripcion")]
            public string Descripcion { get; set; }

            [JsonProperty("objetivos")]
            public string Objetivos { get; set; }

            [JsonProperty("requisitos")]
            public string Requisitos { get; set; }

            [JsonProperty("idUsuario")]
            public int IdUsuario { get; set; }

            [JsonProperty("etiquetas")]
            public List<EtiquetaDTO> Etiquetas { get; set; }

            [JsonProperty("calificacion")]
            public string Calificacion { get; set; }

            [JsonProperty("rol")]
            public string Rol { get; set; }
            [JsonProperty("profesor")]
            public string Profesor { get; set; }
        }

        private void ClicRegresar(object sender, RoutedEventArgs e)
        {
            ListaCursosPagina lista = new ListaCursosPagina();
            NavigationService.Navigate(lista);
        }

        private void ClicModificarCurso(object sender, RoutedEventArgs e)
        {
            bool esCrearCurso = false;
            DocumentoDTO documento = new DocumentoDTO() 
            {
                IdDocumento = (int)_curso.idDocumento,
                Archivo = _curso.Archivo
            };
            List<EtiquetaDTO> etiquetas = new List<EtiquetaDTO>(_cursoDetalle.Etiquetas);
            FormularioCursoPagina formulario = new FormularioCursoPagina(_curso, etiquetas, documento, false);
            this.NavigationService.Navigate(formulario);
        }

        public class ClaseListBox
        {
            [JsonProperty("idClase")]
            public int IdClase { get; set; }
            public int NumeroClase { get; set; }
            [JsonProperty("nombre")]
            public string TituloClase { get; set; }
            public Visibility Visibilidad { get; set; }
        }
    }
}
