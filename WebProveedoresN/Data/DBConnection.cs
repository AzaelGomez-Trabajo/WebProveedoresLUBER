using Microsoft.Data.SqlClient;


namespace WebProveedoresN.Data
{
    public class DBConnection
    {
        private static readonly List<string> cadenasSQL;

        static DBConnection()
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

        public async Task<SqlConnection> GetConnectionAsync()
        {
            foreach (var cadena in cadenasSQL)
            {
                var conexion = new SqlConnection(cadena);
                try
                {
                    await conexion.OpenAsync();
                    await conexion.CloseAsync();
                    return new SqlConnection(cadena);
                }
                catch (SqlException)
                {
                    // Handle exception or continue to the next connection string
                }
            }
            throw new InvalidOperationException("No se pudo establecer una conexión con ninguna de las cadenas de conexión proporcionadas.");
        }
    }
}
