using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace SalonBellezaApp
{
    public class EmpleadoService
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

        public async Task<List<Empleado>> ObtenerTodosAsync()
        {
            var empleados = new List<Empleado>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT IdEmpleado, Nombre, Paterno, Materno, Especialidad, HorarioDisponible FROM Empleados ORDER BY Nombre";

                    using (var command = new SqlCommand(query, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            empleados.Add(new Empleado
                            {
                                IdEmpleado = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Paterno = reader.GetString(2),
                                Materno = reader.GetString(3),
                                Especialidad = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                HorarioDisponible = reader.IsDBNull(5) ? "" : reader.GetString(5)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener empleados: {ex.Message}");
            }

            return empleados;
        }

        public async Task<bool> AgregarAsync(Empleado empleado)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"INSERT INTO Empleados (Nombre, Paterno, Materno, Especialidad, HorarioDisponible) 
                                   VALUES (@Nombre, @Paterno, @Materno, @Especialidad, @HorarioDisponible)";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Nombre", empleado.Nombre);
                        command.Parameters.AddWithValue("@Paterno", empleado.Paterno);
                        command.Parameters.AddWithValue("@Materno", empleado.Materno);
                        command.Parameters.AddWithValue("@Especialidad", empleado.Especialidad ?? "");
                        command.Parameters.AddWithValue("@HorarioDisponible", empleado.HorarioDisponible ?? "");

                        return await command.ExecuteNonQueryAsync() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al agregar empleado: {ex.Message}");
            }
        }

        public async Task<bool> ActualizarAsync(Empleado empleado)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"UPDATE Empleados 
                                   SET Nombre=@Nombre, Paterno=@Paterno, Materno=@Materno, 
                                       Especialidad=@Especialidad, HorarioDisponible=@HorarioDisponible 
                                   WHERE IdEmpleado=@IdEmpleado";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdEmpleado", empleado.IdEmpleado);
                        command.Parameters.AddWithValue("@Nombre", empleado.Nombre);
                        command.Parameters.AddWithValue("@Paterno", empleado.Paterno);
                        command.Parameters.AddWithValue("@Materno", empleado.Materno);
                        command.Parameters.AddWithValue("@Especialidad", empleado.Especialidad ?? "");
                        command.Parameters.AddWithValue("@HorarioDisponible", empleado.HorarioDisponible ?? "");

                        return await command.ExecuteNonQueryAsync() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar empleado: {ex.Message}");
            }
        }

        public async Task<bool> EliminarAsync(int idEmpleado)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Verificar si tiene citas asociadas
                    string verificarQuery = "SELECT COUNT(*) FROM Citas WHERE IdEmpleado = @IdEmpleado";
                    using (var verificarCmd = new SqlCommand(verificarQuery, connection))
                    {
                        verificarCmd.Parameters.AddWithValue("@IdEmpleado", idEmpleado);
                        int citasAsociadas = (int)await verificarCmd.ExecuteScalarAsync();

                        if (citasAsociadas > 0)
                        {
                            throw new Exception("No se puede eliminar el empleado porque tiene citas asignadas.");
                        }
                    }

                    string query = "DELETE FROM Empleados WHERE IdEmpleado = @IdEmpleado";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IdEmpleado", idEmpleado);
                        return await command.ExecuteNonQueryAsync() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar empleado: {ex.Message}");
            }
        }

        public async Task<List<Empleado>> BuscarAsync(string termino)
        {
            var empleados = new List<Empleado>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string query = @"SELECT IdEmpleado, Nombre, Paterno, Materno, Especialidad, HorarioDisponible 
                                   FROM Empleados 
                                   WHERE Nombre LIKE @Termino OR Paterno LIKE @Termino OR Materno LIKE @Termino 
                                      OR Especialidad LIKE @Termino
                                   ORDER BY Nombre";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Termino", $"%{termino}%");

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                empleados.Add(new Empleado
                                {
                                    IdEmpleado = reader.GetInt32(0),
                                    Nombre = reader.GetString(1),
                                    Paterno = reader.GetString(2),
                                    Materno = reader.GetString(3),
                                    Especialidad = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                    HorarioDisponible = reader.IsDBNull(5) ? "" : reader.GetString(5)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar empleados: {ex.Message}");
            }

            return empleados;
        }
    }

    public class Empleado
    {
        public int IdEmpleado { get; set; }
        public string Nombre { get; set; }
        public string Paterno { get; set; }
        public string Materno { get; set; }
        public string Especialidad { get; set; }
        public string HorarioDisponible { get; set; }
    }
}
