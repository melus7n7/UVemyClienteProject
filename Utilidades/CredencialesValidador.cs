using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UVemyCliente.Utilidades
{
    public static class CredencialesValidador
    {
        public static bool EsCorreoValido(string correoElectronico)
        {
            bool EsValido;
            if (string.IsNullOrEmpty(correoElectronico))
            {
                EsValido = false;
            }
            else
            {
                try
                {
                    _ = new MailAddress(correoElectronico);
                    EsValido = true;
                }
                catch (FormatException)
                {
                    EsValido = false;
                }
            }

            return EsValido;
        }

        public static bool EsContraseñaSegura(string contraseña)
        {
            bool esContraseñaSegura;
            string patron = @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).+$";

            TimeSpan tiempoLimite = TimeSpan.FromMilliseconds(500);

            try
            {
                esContraseñaSegura = Regex.IsMatch(contraseña, patron, RegexOptions.None, tiempoLimite);
            }
            catch (RegexMatchTimeoutException)
            {
                esContraseñaSegura = false;
            }

            return esContraseñaSegura;
        }
    }
}
