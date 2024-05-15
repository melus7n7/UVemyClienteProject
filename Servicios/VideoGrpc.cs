using Google.Protobuf;
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
                //Esa no
                await using var videoStream = File.OpenRead(@"C:\Users\sulem\Downloads\VideoIntroKirbyDreamLand3.mp4");

                var buffer = new byte[_tamanioChunks];
                DocumentoVideo documento = new DocumentoVideo { IdClase = 1, Nombre = "video.mp4", IdTipoArchivo = 1 };
                VideoPartesEnvio envioInicial = new VideoPartesEnvio { DatosVideo = documento };

                VideoService.VideoServiceClient stub = ObtenerStub();
                var respuestaEnviando = stub.EnviarVideoClase();

                await respuestaEnviando.RequestStream.WriteAsync(envioInicial);

                int numBytesLeidos = 0;
                while ((numBytesLeidos = await videoStream.ReadAsync(buffer)) > 0)
                {
                    await respuestaEnviando.RequestStream.WriteAsync(new VideoPartesEnvio
                    {
                        Chunks = UnsafeByteOperations.UnsafeWrap(buffer.AsMemory(0, numBytesLeidos))
                    });
                }

                await respuestaEnviando.RequestStream.CompleteAsync();

                EnvioVideoRespuesta respuestaFinal = await respuestaEnviando;
                respuesta = 200;
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
