using Microsoft.Data.SqlClient;

namespace WebProveedoresN.Conexion
{
    public static class DBConexion
    {
        private static readonly string cadenaSQL = "Server=WIN-DJE4SG0JF5L; DataBase=Demo_Programacion; User Id=Noe; Password=Prog@25S;";

        public static SqlConnection ObtenerConexion()
        {
            return new SqlConnection(cadenaSQL);
        }


    }
}
