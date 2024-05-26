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

namespace UVemyCliente.Conexion
{
    public class APIConexion
    {
        private static HttpClient _cliente;
        public static HttpClient ObtenerClient()
        {
            if (_cliente == null)
            {
                _cliente = new HttpClient();
                _cliente.DefaultRequestHeaders.Accept.Clear();
                _cliente.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _cliente.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/pdf"));
                _cliente.BaseAddress = new Uri("http://localhost:3000/api/");
            }

            //To-DO
            SingletonUsuario.JWT = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3ByaW1hcnlzaWQiOjQsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6Imtpa2dhbWJvYUBnbWFpbC5jb20iLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9naXZlbm5hbWUiOiJFbnJpcXVlIFBVVCIsImlzcyI6IlVWZW15U2Vydmlkb3JKV1QiLCJhdWQiOiJVc3Vhcmlvc1VWZW15SldUIiwiaWF0IjoxNzE2NjgzMjcxLCJleHAiOjE3MTY3MjY0NzF9.EowL6xG89Mhjr6L2Xk6xDt4mU7PNsO9W3JEEQ14_vig";
            _cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SingletonUsuario.JWT);

            return _cliente;
        }

        public static HttpClient ObtenerClientSinAutenticacion()
        {
            if (_cliente == null)
            {
                _cliente = new HttpClient();
                _cliente.DefaultRequestHeaders.Accept.Clear();
                _cliente.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                _cliente.BaseAddress = new Uri("http://localhost:3000/api/");
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
                    respuesta.StatusCode = System.Net.HttpStatusCode.ServiceUnavailable;
                }
                else
                {
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
                Console.WriteLine(ex);
                respuesta.StatusCode = System.Net.HttpStatusCode.InternalServerError;
            }
            return respuesta;
        }
    }
}

