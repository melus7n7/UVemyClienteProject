using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UVemyCliente.Utilidades;
using System.Diagnostics;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.IO;

namespace UVemyCliente.Conexion
{
    public class APIConexion
    {
        private static HttpClient _cliente;
        public static HttpClient ObtenerClient()
        {
            string apiUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

            if (_cliente == null)
            {
                _cliente = new HttpClient();
                _cliente.DefaultRequestHeaders.Accept.Clear();
                _cliente.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _cliente.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/pdf"));
                _cliente.BaseAddress = new Uri(apiUrl);
            }

            _cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SingletonUsuario.JWT);

            return _cliente;
        }

        public static HttpClient ObtenerClientSinAutenticacion()
        {
            string apiUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];

            if (_cliente == null)
            {
                _cliente = new HttpClient();
                _cliente.DefaultRequestHeaders.Accept.Clear();
                _cliente.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _cliente.BaseAddress = new Uri(apiUrl);
            }

            _cliente.DefaultRequestHeaders.Authorization = null;

            return _cliente;
        }

        public static async Task<HttpResponseMessage> EnviarRequestAsync(HttpMethod method, string relativeUri, HttpContent content = null)
        {
            HttpResponseMessage respuesta = new HttpResponseMessage();
            try
            {
                respuesta = await ObtenerClient().SendAsync(new HttpRequestMessage(method, relativeUri)
                {
                    Content = content
                });

                if (respuesta.Headers.Contains("Set-Authorization"))
                {
                    var tokenNuevo = respuesta.Headers.GetValues("Set-Authorization").FirstOrDefault();
                    if (!string.IsNullOrEmpty(tokenNuevo))
                    {
                        SingletonUsuario.JWT = tokenNuevo;
                        _cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SingletonUsuario.JWT);
                    }
                }

                if(respuesta.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    ErrorMensaje errorMensaje = new ("Por inactividad su sesión ha vencido. Por favor, vuelva a iniciar sesión");
                    errorMensaje.Show();
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine(ex);
                if (ex.InnerException is SocketException socketEx && socketEx.SocketErrorCode == SocketError.ConnectionRefused)
                {
                    Debug.WriteLine("InnerException");
                    respuesta.StatusCode = System.Net.HttpStatusCode.ServiceUnavailable;
                }
                else
                {
                    Debug.WriteLine("NoInnerException");
                    respuesta.StatusCode = System.Net.HttpStatusCode.InternalServerError; 
                }
            }
            return respuesta;
        }

        public static async Task<HttpResponseMessage> EnviarRequestSinAutenticacionAsync(HttpMethod method, string relativeUri, HttpContent content = null)
        {
            HttpResponseMessage respuesta = new HttpResponseMessage();
            try
            {
                respuesta = await ObtenerClientSinAutenticacion().SendAsync(new HttpRequestMessage(method, relativeUri)
                {
                    Content = content
                });
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine(ex);
                if (ex.InnerException is SocketException socketEx && socketEx.SocketErrorCode == SocketError.ConnectionRefused)
                {
                    respuesta.StatusCode = System.Net.HttpStatusCode.ServiceUnavailable;
                }
                else
                {
                    respuesta.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                }
            }
            return respuesta;
        }
    }
}

