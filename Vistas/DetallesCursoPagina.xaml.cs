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
    /// Lógica de interacción para DetallesCursoPagina.xaml
    /// </summary>
    public partial class DetallesCurso : Page
    {
        int _idCurso = 4;
        CursoDTO _curso = new CursoDTO();
        CursoDetalle _cursoDetalle = new CursoDetalle();
        public DetallesCurso(CursoDTO curso)
        {
            _curso = curso;
            _idCurso = (int)_curso.IdCurso;
            InitializeComponent();
            _ = CargarCursoAsync();
            _ = CargarClasesAsync();
            //Cargar clases
            //Botones dependiendo del rol
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
            /*HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestAsync(HttpMethod.Get, "clases/" + _idCurso);
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
            }*/
        }

        private void CargarCurso()
        {
            txtBlockTitulo.Text = _curso.Titulo;

            _curso.Objetivos = _cursoDetalle.Objetivos;
            txtBoxObjetivos.Text = _curso.Objetivos;
            _curso.Etiquetas = new List<int>();
            foreach (EtiquetaDTO etiqueta in _cursoDetalle.Etiquetas)
            {
                _curso.Etiquetas.Add(etiqueta.IdEtiqueta);
                txtBoxEtiquetas.Text += etiqueta.Nombre;
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
            //btnVerClase.Visibility = Visibility.Visible;

            switch (_cursoDetalle.Rol)
            {
                case "Profesor":
                    btnAgregarClase.Visibility = Visibility.Visible;
                    btnModificarCurso.Visibility = Visibility.Visible;
                    btnVerEstadisticas.Visibility = Visibility.Visible;
                    break;
                case "Estudiante":
                    btnCalificarCurso.Visibility = Visibility.Visible;
                    break;
                case "Usuario":
                    btnInscribirse.Visibility = Visibility.Visible;
                    //btnVerClase.Visibility = Visibility.Hidden;
                    break;
                default:
                    break;
            }

        }

        private void ClicAgregarClase(object sender, RoutedEventArgs e)
        {
            FormularioClase formulario = new FormularioClase(_idCurso);
            NavigationService.Navigate(formulario);
        }

        private void ClicVerClase(object sender, RoutedEventArgs e)
        {
            DetallesClase detalles = new DetallesClase(53, this);
            NavigationService.Navigate(detalles);
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
            }

            btnRegresar.IsEnabled = true;
            grdBackground.IsEnabled = true;
        }

        private void ClicCalificarCurso(object sender, RoutedEventArgs e)
        {
            CalificacionCurso pagina = new CalificacionCurso(new CursoDTO { IdCurso = 1, Titulo = "si"});
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
    }
}
