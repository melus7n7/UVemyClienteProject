using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Lógica de interacción para FormularioUsuarioPagina.xaml
    /// </summary>
    public partial class FormularioUsuarioPagina : Page
    {
        private static readonly int BYTES_POR_KILOBYTE = 1024;
        private static readonly int KILOBYTES_POR_MEGABYTE = 1024;

        private string _nombres = string.Empty;
        private string _apellidos = string.Empty;
        private string _correoElectronico = string.Empty;
        private string _contrasena = string.Empty;
        private string _confirmarContrasena = string.Empty;
        private byte[] _imagenPerfil = [];

        public FormularioUsuarioPagina()
        {
            InitializeComponent();
        }

        public void CargarPaginaConsultaPerfil()
        {
            txtBoxNombres.Text = SingletonUsuario.Nombres;
            txtBoxNombres.IsEnabled = false;
            txtBoxApellidos.Text = SingletonUsuario.Apellidos;
            txtBoxApellidos.IsEnabled = false;
            txtBoxCorreoElectronico.Text = SingletonUsuario.CorreoElectronico;
            txtBoxCorreoElectronico.IsEnabled = false;
            vwBoxImagenPerfil.Visibility = Visibility.Visible;
            btnSubirImagen.Visibility = Visibility.Collapsed;
            btnRegistrate.Visibility = Visibility.Collapsed;
            btnModificar.Visibility = Visibility.Visible;
            grdContrasenas.Visibility = Visibility.Collapsed;
            txtBlockContrasena.Text += " " + "(Opcional)";

            _ = ObtenerFotoPerfilAsync();
        }

        private async Task ObtenerFotoPerfilAsync()
        {
            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestAsync(HttpMethod.Get, "perfil/foto");

            if (respuestaHttp.IsSuccessStatusCode)
            {
                _imagenPerfil = await respuestaHttp.Content.ReadAsByteArrayAsync();

                BitmapImage bitmap = new();
                using (MemoryStream ms = new(_imagenPerfil))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                }

                imgPerfil.Source = bitmap;
            }
            else if (respuestaHttp.StatusCode >= System.Net.HttpStatusCode.InternalServerError)
            {
                ErrorMensaje errorMensaje = new("Error. No se pudo conectar con el servidor. Inténtelo de nuevo o hágalo más tarde.");
                errorMensaje.Show();
            }   
        }

        private void ClicRegistrar(object sender, RoutedEventArgs e)
        {
            if(ValidarCampos())
            {
                ContinuarRegistro();
            }
        }

        private bool ValidarCampos()
        {
            _nombres = txtBoxNombres.Text.Trim();
            _apellidos = txtBoxApellidos.Text.Trim();
            _correoElectronico = txtBoxCorreoElectronico.Text.Trim();
            _contrasena = pwdBoxContrasena.Password.Trim();
            _confirmarContrasena = pwdBoxContrasenaRepetida.Password.Trim();

            bool sonValidos = true;
            string mensajeError = string.Empty;

            if (string.IsNullOrEmpty(_nombres))
            {
                mensajeError = "Nombre/s requerido";
                sonValidos = false;
            }

            if(string.IsNullOrEmpty(_apellidos))
            {
                mensajeError += string.IsNullOrEmpty(mensajeError) ? "Apellido/s requerido" : ", apellido/s requerido";
                sonValidos = false;
            }

            if(string.IsNullOrEmpty(_correoElectronico))
            {
                mensajeError += string.IsNullOrEmpty(mensajeError) ? "Correo electrónico requerido" : ", correo electrónico requerido";
                sonValidos = false;
            }
            else if(!CredencialesValidador.EsCorreoValido(_correoElectronico))
            {
                mensajeError += string.IsNullOrEmpty(mensajeError) ? "Correo electrónico inválido" : ", correo electrónico inválido";
                sonValidos = false;
            }

            if(string.IsNullOrEmpty(_contrasena) || string.IsNullOrEmpty(_confirmarContrasena))
            {
                mensajeError += string.IsNullOrEmpty(mensajeError) ? "Contraseñas requeridas" : ", contraseñas requeridas";
                sonValidos = false;
            }
            else if(_contrasena != _confirmarContrasena)
            {
                mensajeError += string.IsNullOrEmpty(mensajeError) ? "Las contraseñas no coinciden" : ", las contraseñas no coinciden";
                sonValidos = false;
            }
            else if(!CredencialesValidador.EsContraseñaSegura(_contrasena))
            {
                mensajeError += string.IsNullOrEmpty(mensajeError) ? "Contraseña inválida (debe contener al menos una minúscula, una mayúscula y un número)" : ", contraseña inválida (debe contener al menos una minúscula, una mayúscula y un número)";
                sonValidos = false;
            }

            if (!sonValidos)
            {
                ErrorMensaje errorMensaje = new(mensajeError);
                errorMensaje.ShowDialog();
            }

            return sonValidos;
        }

        private void ContinuarRegistro()
        {
            UsuarioDTO usuario = new()
            {
                Nombres = _nombres,
                Apellidos = _apellidos,
                CorreoElectronico = _correoElectronico,
                Contrasena = _contrasena,
            };

            SeleccionEtiquetasPagina seleccionEtiquetasPagina = new(usuario);
            this.NavigationService.Navigate(seleccionEtiquetasPagina);
        }

        private void ClicActualizar(object sender, RoutedEventArgs e)
        {
            _contrasena = pwdBoxContrasena.Password.Trim();
            _confirmarContrasena = pwdBoxContrasenaRepetida.Password.Trim();

            if(string.IsNullOrEmpty(_contrasena) && string.IsNullOrEmpty(_confirmarContrasena))
            {
                pwdBoxContrasena.Password = "Contrasena1";
                pwdBoxContrasenaRepetida.Password = "Contrasena1";

                if(ValidarCampos())
                {
                    _contrasena = string.Empty;
                    _confirmarContrasena = string.Empty;
                    _ = ActualizarPerfilAsync();
                }
            }
            else
            {
                if (ValidarCampos())
                {
                    _ = ActualizarPerfilAsync();
                }
            }
        }

        private async Task ActualizarPerfilAsync()
        {
            var usuarioDto = new Dictionary<string, object>
            {
                { "idUsuario", SingletonUsuario.IdUsuario },
                { "nombres", _nombres },
                { "apellidos", _apellidos },
                { "correoElectronico", _correoElectronico }
            };

            if (!string.IsNullOrEmpty(_contrasena))
            {
                usuarioDto["contrasena"] = _contrasena;
            }

            var json = JsonSerializer.Serialize(usuarioDto);
            var contenido = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestAsync(HttpMethod.Put, "perfil", contenido);
            if (respuestaHttp.IsSuccessStatusCode)
            {
                SingletonUsuario.Nombres = _nombres;
                SingletonUsuario.Apellidos = _apellidos;

                ExitoMensaje exitoMensaje = new("Datos del perfil actualizado");
                exitoMensaje.Show();

                txtBoxNombres.IsEnabled = false;
                txtBoxApellidos.IsEnabled = false;
                pwdBoxContrasena.Password = string.Empty;
                pwdBoxContrasenaRepetida.Password = string.Empty;
                grdContrasenas.Visibility = Visibility.Collapsed;
                btnActualizar.Visibility = Visibility.Collapsed;
                btnModificar.Visibility = Visibility.Visible;
                btnEtiquetas.Visibility = Visibility.Collapsed;
                btnCambiarImagen.Visibility = Visibility.Collapsed;
                btnSubirImagen.Visibility = Visibility.Collapsed;
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

        private void ClicCambiarImagen(object sender, RoutedEventArgs e)
        {
            ClicSubirImagen(sender,e);
        }

        private void ClicSubirImagen(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialogo = new()
            {
                Filter = "Archivos de imagen|*.png;"
            };

            if (dialogo.ShowDialog() == true)
            {
                string rutaImagen = dialogo.FileName;
                ValidarTamanioImagen(rutaImagen);
            }
        }

        private void ValidarTamanioImagen(string rutaImagen)
        {
            try
            {
                FileInfo informacionArchivo = new(rutaImagen);
                long tamañoEnBytes = informacionArchivo.Length;
                long tamañoEnKilobytes = tamañoEnBytes / BYTES_POR_KILOBYTE;

                if (tamañoEnKilobytes <= KILOBYTES_POR_MEGABYTE)
                {
                    _imagenPerfil = File.ReadAllBytes(rutaImagen);

                    _ = SubirImagenAsync(informacionArchivo);
                }
                else
                {
                    ErrorMensaje errorMensaje = new("La imagen no puede superar 1MB");
                    errorMensaje.ShowDialog();
                }
            }
            catch (IOException)
            {
                ErrorMensaje errorMensaje = new("Error al cargar la imagen");
                errorMensaje.ShowDialog();
            }
        }

        private async Task SubirImagenAsync(FileInfo informacionArchivo)
        {
            var contenido = new MultipartFormDataContent()
            {
                { new StringContent(SingletonUsuario.IdUsuario.ToString()), "idUsuario"}
            };

            var contenidoImagen = new ByteArrayContent(_imagenPerfil);
            contenidoImagen.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            contenido.Add(contenidoImagen, "imagen", informacionArchivo.Name + ".png");

            HttpResponseMessage respuestaHttp = await APIConexion.EnviarRequestAsync(HttpMethod.Put, "perfil/foto", contenido);

            if (respuestaHttp.IsSuccessStatusCode)
            {
                BitmapImage bitmapImagen = new(new Uri(informacionArchivo.FullName));
                imgPerfil.Source = bitmapImagen;

                btnCambiarImagen.Visibility = Visibility.Visible;
                btnSubirImagen.Visibility = Visibility.Collapsed;

                ExitoMensaje exitoMensaje = new("Imagen de perfil actualizada");
                exitoMensaje.Show();
            }
            else
            {
                _imagenPerfil = [];
                var jsonString = await respuestaHttp.Content.ReadAsStringAsync();

                using JsonDocument document = JsonDocument.Parse(jsonString);
                JsonElement root = document.RootElement;

                if (root.TryGetProperty("Detalles", out JsonElement detallesElement))
                {
                    if (detallesElement.ValueKind == JsonValueKind.Array)
                    {
                        List<string> detallesList = [];
                        foreach (JsonElement detalle in detallesElement.EnumerateArray())
                        {
                            detallesList.Add(detalle.GetString());
                        }
                        string detallesConcatenados = string.Join(", ", detallesList);
                        ErrorMensaje errorMensaje = new(detallesConcatenados + ". Verifique la información e intentélo de nuevo más tarde");
                        errorMensaje.Show();
                    }
                }
            }
        }

        private void ClicEtiquetas(object sender, RoutedEventArgs e)
        {
            //TODO: Implementar
        }

        private void ClicRegresar(object sender, RoutedEventArgs e)
        {
            this.NavigationService.GoBack();
        }

        private void ClicModificar(object sender, RoutedEventArgs e)
        {
            txtBoxNombres.IsEnabled = true;
            txtBoxApellidos.IsEnabled = true;
            grdContrasenas.Visibility = Visibility.Visible;
            btnModificar.Visibility = Visibility.Collapsed;
            btnActualizar.Visibility = Visibility.Visible;
            btnEtiquetas.Visibility = Visibility.Visible;

            if (_imagenPerfil.Length == 0 || _imagenPerfil == null)
            {
                btnSubirImagen.Visibility = Visibility.Visible;
            }
            else
            {
                btnCambiarImagen.Visibility = Visibility.Visible;
            }
        }
    }
}
