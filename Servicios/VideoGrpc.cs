using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UVemyCliente.DTO;
using UVemyCliente.Utilidades;

namespace UVemyCliente.Servicios
{
    public class VideoGrpc
    {
        private static int _tamanioChunks = 18 * 1024;
        private static VideoService.VideoServiceClient _stubCliente;
        private static VideoService.VideoServiceClient ObtenerStub()
        {
            if(_stubCliente == null)
            {
                var channel = GrpcChannel.ForAddress("http://localhost:3001");
                _stubCliente = new VideoService.VideoServiceClient(channel);
            }
            return _stubCliente;
        }

        public static async Task<int> EnviarVideoDeClaseAsync(DocumentoDTO documentoDTO)
        {
            int respuesta;

            try
            {
                var buffer = new byte[_tamanioChunks];
                byte[] videoBytes = documentoDTO.Archivo;

                DocumentoVideo documento = new DocumentoVideo { IdClase = documentoDTO.IdClase, Nombre = documentoDTO.Nombre, 
                    Jwt = "Bearer " +  SingletonUsuario.JWT, IdVideo = documentoDTO.IdDocumento};
                VideoPartesEnvio envioInicial = new VideoPartesEnvio { DatosVideo = documento };

                VideoService.VideoServiceClient stub = ObtenerStub();

                AsyncClientStreamingCall<VideoPartesEnvio, EnvioVideoRespuesta> respuestaEnviando;
                if (documento.IdVideo == 0)
                {
                    respuestaEnviando = stub.EnviarVideoClase();
                }
                else
                {
                    respuestaEnviando = stub.ActualizarVideoClase();

                }

                await respuestaEnviando.RequestStream.WriteAsync(envioInicial);

                int numBytesLeidos = 0;
                while (numBytesLeidos < videoBytes.Length)
                {
                    int count = Math.Min(_tamanioChunks, videoBytes.Length - numBytesLeidos);
                    byte[] chunk = new byte[count];
                    Buffer.BlockCopy(videoBytes, numBytesLeidos, chunk, 0, count);

                    await respuestaEnviando.RequestStream.WriteAsync(new VideoPartesEnvio
                    {
                        Chunks = UnsafeByteOperations.UnsafeWrap(chunk.AsMemory())
                    });

                    numBytesLeidos += count;
                }

                await respuestaEnviando.RequestStream.CompleteAsync();

                EnvioVideoRespuesta respuestaFinal = await respuestaEnviando;
                respuesta = respuestaFinal.Respuesta;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
                respuesta = 500;
            }

            return respuesta;
        }
    }
}
