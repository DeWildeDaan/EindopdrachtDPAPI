using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using DPEindopdrachtAPI.Models;
using System.Data.SqlClient;

namespace DPEindopdrachtAPI
{
    public static class Function1
    {
        [FunctionName("GetComments")]
        public static async Task<IActionResult> GetComments(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/comments/chargers/{id}")] HttpRequest req, int id,
            ILogger log)
        {
            try
            {
                string connectionstring = Environment.GetEnvironmentVariable("SQLSERVER");

                List<Comment> comments = new List<Comment>();

                using (SqlConnection sqlConnection = new SqlConnection(connectionstring))
                {
                    await sqlConnection.OpenAsync();
                    using (SqlCommand sqlCommand = new SqlCommand())
                    {
                        sqlCommand.Connection = sqlConnection;
                        sqlCommand.CommandText = "SELECT * FROM comments WHERE ChargerId = @chargerid";
                        sqlCommand.Parameters.AddWithValue("@chargerid", id);

                        var sqlDataReader = await sqlCommand.ExecuteReaderAsync();
                        while (await sqlDataReader.ReadAsync())
                        {
                            comments.Add(new Comment()
                            {
                                CommentId = Guid.Parse(sqlDataReader["CommentId"].ToString()),
                                ChargerId = int.Parse(sqlDataReader["ChargerId"].ToString()),
                                UserName = sqlDataReader["UserName"].ToString(),
                                Email = sqlDataReader["Email"].ToString(),
                                CommentText = sqlDataReader["Comment"].ToString(),
                                DateAndTime = DateTime.Parse(sqlDataReader["DateAndTime"].ToString())
                            });
                        }
                    }
                }

                return new OkObjectResult(comments);
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                return new StatusCodeResult(500);


            }
        }

        [FunctionName("PostComment")]
        public static async Task<IActionResult> PostComment(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/comments/chargers/{id}")] HttpRequest req, int id,
            ILogger log)
        {
            try
            {
                var json = await new StreamReader(req.Body).ReadToEndAsync();
                var comment = JsonConvert.DeserializeObject<Comment>(json);

                string connectionstring = Environment.GetEnvironmentVariable("SQLSERVER");

                Guid guid = Guid.NewGuid();
                comment.ChargerId = id;
                comment.CommentId = guid;
                comment.DateAndTime = DateTime.Now;


                using (SqlConnection sqlConnection = new SqlConnection(connectionstring))
                {
                    await sqlConnection.OpenAsync();
                    using (SqlCommand sqlCommand = new SqlCommand())
                    {
                        sqlCommand.Connection = sqlConnection;
                        sqlCommand.CommandText = "INSERT INTO comments VALUES(@CommentId, @ChargerId, @UserName, @Email, @Comment, @DateAndTime)";
                        sqlCommand.Parameters.AddWithValue("@CommentId", comment.CommentId);
                        sqlCommand.Parameters.AddWithValue("@ChargerId", comment.ChargerId);
                        sqlCommand.Parameters.AddWithValue("@UserName", comment.UserName);
                        sqlCommand.Parameters.AddWithValue("@Email", comment.Email);
                        sqlCommand.Parameters.AddWithValue("@Comment", comment.CommentText);
                        sqlCommand.Parameters.AddWithValue("@DateAndTime", comment.DateAndTime);

                        await sqlCommand.ExecuteNonQueryAsync();
                    }
                }

                return new OkObjectResult(comment);
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                return new StatusCodeResult(500);
                throw;
            }
        }

        [FunctionName("PutComment")]
        public static async Task<IActionResult> PutComment(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "v1/comments/{id}")] HttpRequest req, string id,
            ILogger log)
        {
            try
            {
                var json = await new StreamReader(req.Body).ReadToEndAsync();
                var comment = JsonConvert.DeserializeObject<Comment>(json);
                comment.CommentId = Guid.Parse(id);

                string connectionstring = Environment.GetEnvironmentVariable("SQLSERVER");

                using (SqlConnection sqlConnection = new SqlConnection(connectionstring))
                {
                    await sqlConnection.OpenAsync();
                    using (SqlCommand sqlCommand = new SqlCommand())
                    {
                        sqlCommand.Connection = sqlConnection;
                        sqlCommand.CommandText = "UPDATE comments SET Comment = @Comment WHERE CommentId = @CommentId";
                        sqlCommand.Parameters.AddWithValue("@CommentId", comment.CommentId);
                        sqlCommand.Parameters.AddWithValue("@Comment", comment.CommentText);

                        await sqlCommand.ExecuteNonQueryAsync();
                    }
                }

                return new OkObjectResult(comment);
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                return new StatusCodeResult(500);
                throw;
            }
        }

        [FunctionName("DelComment")]
        public static async Task<IActionResult> DelComment(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "v1/comments/{id}")] HttpRequest req, string id,
            ILogger log)
        {
            try
            {
                string connectionstring = Environment.GetEnvironmentVariable("SQLSERVER");

                using (SqlConnection sqlConnection = new SqlConnection(connectionstring))
                {
                    await sqlConnection.OpenAsync();
                    using (SqlCommand sqlCommand = new SqlCommand())
                    {
                        sqlCommand.Connection = sqlConnection;
                        sqlCommand.CommandText = "DELETE FROM comments WHERE CommentId = @CommentId";
                        sqlCommand.Parameters.AddWithValue("@CommentId", id);
                        await sqlCommand.ExecuteNonQueryAsync();
                    }
                }

                return new OkObjectResult("");
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                return new StatusCodeResult(500);
                throw;
            }
        }
    }
}
