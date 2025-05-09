﻿using Microsoft.Data.SqlClient;


namespace WebProveedoresN.Data
{
    public static class DBConnectiion
    {
        private static readonly List<string> cadenasSQL;

        static DBConnectiion()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json").Build();
            cadenasSQL = new List<string>
            {
                builder.GetSection("ConnectionStrings:CadenaSQL1").Value ?? throw new InvalidOperationException("la cadena de conexion no puede ser nula."),
                builder.GetSection("ConnectionStrings:CadenaSQL2").Value ?? throw new InvalidOperationException("la cadena de conexion no puede ser nula.")
            };
        }

        public static SqlConnection GetConnection()
        {
            foreach (var cadena in cadenasSQL)
            {
                    var conexion = new SqlConnection(cadena);
                try
                {
                    conexion.Open();
                    conexion.Close();
                    return new SqlConnection(cadena);
                }
                catch (SqlException)
                {

                }
            }
            throw new InvalidOperationException("No se pudo establecer una conexión con ninguna de las cadenas de conexión proporcionadas.");
        }
    }
}
