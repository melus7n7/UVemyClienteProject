using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UVemyCliente.DTO
{
    public class UsuarioDTO
    {
        [JsonPropertyName("idUsuario")]
        public int? Id { get; set; }
        [JsonPropertyName("nombres")]
        public string? Nombres { get; set; }
        [JsonPropertyName("apellidos")]
        public string? Apellidos { get; set; }
        [JsonPropertyName("correoElectronico")]
        public string? CorreoElectronico { get; set; }
        [JsonPropertyName("contrasena")]
        public string? Contrasena { get; set; }
        [JsonPropertyName("imagen")]
        public byte[]? Imagen { get; set; }
        [JsonPropertyName("idsEtiqueta")]
        public List<int>? IdsEtiqueta { get; set; }
        [JsonPropertyName("codigoVerificacion")]
        public string? CodigoVerificacion { get; set; }
        [JsonPropertyName("jwt")]
        public string? Token { get; set; }
        [JsonPropertyName("detalles")]
        public List<string>? Detalles { get; set; }
    }
}
