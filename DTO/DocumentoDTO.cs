using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UVemyCliente.DTO
{
    public class DocumentoDTO
    {
        [JsonPropertyName("idDocumento")]
        public int IdDocumento { get; set; }
        [JsonPropertyName("archivo")]
        public byte[] Archivo { get; set; }
        [JsonPropertyName("nombre")]
        public string Nombre { get; set; }
        [JsonPropertyName("idTipoArchivo")]
        public int IdTipoArchivo { get; set; }
        [JsonPropertyName("idCurso")]
        public int IdCurso { get; set; }
        [JsonPropertyName("idClase")]
        public int IdClase { get; set; }
    }
}
