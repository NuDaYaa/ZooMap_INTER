using System;
using System.Collections.Generic;
using System.Windows;
using ZooMap.Models;

// Убедитесь, что используете Microsoft.Data.SqlClient
using Microsoft.Data.SqlClient;

namespace ZooMap.Database
{
    public class DatabaseHelper
    {
        // Используем упрощенную строку подключения
        private string connectionString = "Server=HOME-PC\\SQLEXPRESS;Database=ZooMapDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public User AuthenticateUser(string username, string password)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT Id, Username, Email, Role, CreatedDate FROM Users WHERE Username = @Username AND Password = @Password";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@Password", password);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new User
                                {
                                    Id = reader.GetInt32(0),
                                    Username = reader.GetString(1),
                                    Email = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                    Role = reader.GetString(3),
                                    CreatedDate = reader.GetDateTime(4)
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка аутентификации: {ex.Message}");
            }
            return null;
        }

        public bool RegisterUser(string username, string password, string email, string role = "Visitor")
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO Users (Username, Password, Email, Role) VALUES (@Username, @Password, @Email, @Role)";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@Password", password);
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@Role", role);

                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка регистрации: {ex.Message}");
                return false;
            }
        }

        public List<Enclosure> GetAllEnclosures()
        {
            var enclosures = new List<Enclosure>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT Id, Name, Description, LocationX, LocationY, AnimalType, ImagePath, LastModified FROM Enclosures";

                    using (var command = new SqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            enclosures.Add(new Enclosure
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                LocationX = reader.GetDouble(3),
                                LocationY = reader.GetDouble(4),
                                AnimalType = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                                ImagePath = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                                LastModified = reader.GetDateTime(7)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки вольеров: {ex.Message}");
            }

            return enclosures;
        }

        public Enclosure GetEnclosureByName(string name)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT Id, Name, Description, LocationX, LocationY, AnimalType, ImagePath, LastModified FROM Enclosures WHERE Name = @Name";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Name", name);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Enclosure
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                    LocationX = reader.GetDouble(3),
                                    LocationY = reader.GetDouble(4),
                                    AnimalType = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                                    ImagePath = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                                    LastModified = reader.GetDateTime(7)
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки вольера: {ex.Message}");
            }
            return null;
        }

        public void UpdateEnclosure(Enclosure enclosure)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"UPDATE Enclosures SET 
                                    Name = @Name, 
                                    Description = @Description, 
                                    AnimalType = @AnimalType, 
                                    LastModified = GETDATE() 
                                    WHERE Id = @Id";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", enclosure.Id);
                        command.Parameters.AddWithValue("@Name", enclosure.Name);
                        command.Parameters.AddWithValue("@Description", enclosure.Description);
                        command.Parameters.AddWithValue("@AnimalType", enclosure.AnimalType);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления вольера: {ex.Message}");
            }
        }

        public List<User> GetAllUsers()
        {
            var users = new List<User>();

            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT Id, Username, Email, Role, CreatedDate FROM Users";

                    using (var command = new SqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new User
                            {
                                Id = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                Email = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                Role = reader.GetString(3),
                                CreatedDate = reader.GetDateTime(4)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}");
            }
            return users;
        }
    }
}