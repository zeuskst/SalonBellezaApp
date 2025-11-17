using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace SalonBellezaApp
{
    public class ClienteService
    {
        private readonly string _connectionString = @"Data Source=LAPTOP-28BR3PVP\SQLEXPRESS;
Initial Catalog=SalonBellezaDB;
Integrated Security=True;
Persist Security Info=False;
Pooling=False;
MultipleActiveResultSets=False;
Encrypt=True;
TrustServerCertificate=True;
Application Name=""MiAppCSharp"";
Command Timeout=30;";

        public async Task<List<Cliente>> ObtenerTodosAsync()
        {
            var clientes = new List<Cliente>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT IdCliente, Nombre,Paterno,Materno, Telefono, Correo, Preferencias, Alergias, Comentarios FROM Clientes ORDER BY Nombre";

                    using (var command = new SqlCommand(query, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            clientes.Add(new Cliente
                            {
                                IdCliente = reader.GetInt32("IdCliente"),
                                Nombre = reader.GetString("Nombre"),
                                Paterno = reader.IsDBNull("Paterno") ? "" : reader.GetString("Paterno"),
                                Materno = reader.IsDBNull("Materno") ? "" : reader.GetString("Materno"),
                                Telefono = reader.GetString("Telefono"),
                                Correo = reader.IsDBNull("Correo") ? "" : reader.GetString("Correo"),
                                Preferencias = reader.IsDBNull("Preferencias") ? "" : reader.GetString("Preferencias"),
                                Alergias = reader.IsDBNull("Alergias") ? "" : reader.GetString("Alergias"),
                                Comentarios = reader.IsDBNull("Comentarios") ? "" : reader.GetString("Comentarios")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener clientes: {ex.Message}");
            }

            return clientes;
        }

        public async Task<bool> AgregarAsync(Cliente cliente)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"INSERT INTO Clientes (Nombre,Paterno, Materno, Telefono, Correo, Preferencias, Alergias, Comentarios) 
                                   VALUES (@Nombre,@Paterno,@Materno, @Telefono, @Correo, @Preferencias, @Alergias, @Comentarios)";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Nombre", cliente.Nombre);
                        command.Parameters.AddWithValue("@Paterno", cliente.Paterno ?? "");
                        command.Parameters.AddWithValue("@Materno", cliente.Materno ?? "");
                        command.Parameters.AddWithValue("@Telefono", cliente.Telefono);
                        command.Parameters.AddWithValue("@Correo", cliente.Correo ?? "");
                        command.Parameters.AddWithValue("@Preferencias", cliente.Preferencias ?? "");
                        command.Parameters.AddWithValue("@Alergias", cliente.Alergias ?? "");
                        command.Parameters.AddWithValue("@Comentarios", cliente.Comentarios ?? "");

                        return await command.ExecuteNonQueryAsync() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al agregar cliente: {ex.Message}");
            }
        }

        public async Task<bool> ActualizarAsync(Cliente cliente)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"UPDATE Clientes 
                                   SET Nombre=@Nombre,Paterno=@Paterno,Materno=@Materno, Telefono=@Telefono, Correo=@Correo, 
                                       Preferencias=@Preferencias, Alergias=@Alergias, Comentarios=@Comentarios 
                                   WHERE IdCliente=@IdCliente";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdCliente", cliente.IdCliente);
                        command.Parameters.AddWithValue("@Nombre", cliente.Nombre);
                        command.Parameters.AddWithValue("@Paterno", cliente.Paterno ?? "");
                        command.Parameters.AddWithValue("@Materno", cliente.Materno ?? "");
                        command.Parameters.AddWithValue("@Telefono", cliente.Telefono);
                        command.Parameters.AddWithValue("@Correo", cliente.Correo ?? "");
                        command.Parameters.AddWithValue("@Preferencias", cliente.Preferencias ?? "");
                        command.Parameters.AddWithValue("@Alergias", cliente.Alergias ?? "");
                        command.Parameters.AddWithValue("@Comentarios", cliente.Comentarios ?? "");

                        return await command.ExecuteNonQueryAsync() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar cliente: {ex.Message}");
            }
        }

        public async Task<bool> EliminarAsync(int idCliente)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = "DELETE FROM Clientes WHERE IdCliente = @IdCliente";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdCliente", idCliente);
                        return await command.ExecuteNonQueryAsync() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar cliente: {ex.Message}");
            }
        }

        public async Task<List<Cliente>> BuscarAsync(string termino)
        {
            var clientes = new List<Cliente>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"SELECT IdCliente, Nombre,Paterno,Materno, Telefono, Correo, Preferencias, Alergias, Comentarios 
                                   FROM Clientes 
                                   WHERE Nombre LIKE @Termino OR Telefono LIKE @Termino OR Correo LIKE @Termino
                                   ORDER BY Nombre";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Termino", $"%{termino}%");

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                clientes.Add(new Cliente
                                {
                                    IdCliente = reader.GetInt32("IdCliente"),
                                    Nombre = reader.GetString("Nombre"),

                                    Telefono = reader.GetString("Telefono"),
                                    Correo = reader.IsDBNull("Correo") ? "" : reader.GetString("Correo"),
                                    Preferencias = reader.IsDBNull("Preferencias") ? "" : reader.GetString("Preferencias"),
                                    Alergias = reader.IsDBNull("Alergias") ? "" : reader.GetString("Alergias"),
                                    Comentarios = reader.IsDBNull("Comentarios") ? "" : reader.GetString("Comentarios")
                                    
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar clientes: {ex.Message}");
            }

            return clientes;
        }
       
    }
    public class Cliente
    {
        public int IdCliente { get; set; }
        public string Nombre { get; set; }
        public string Paterno { get; set; }
        public string Materno { get; set; }
        public string Telefono { get; set; }
        public string Correo { get; set; }
        public string Preferencias { get; set; }
        public string Alergias { get; set; }
        public string Comentarios { get; set; }
    }
}
