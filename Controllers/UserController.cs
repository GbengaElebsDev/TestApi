using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using TestApi.DTOs;
using Dapper;
using MySql.Data.MySqlClient;
using System.Data;
using Microsoft.Extensions.Logging;

namespace TestApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string connString;
        private readonly ILogger<UserController> _logger;

        public UserController(IConfiguration configuration, ILogger<UserController> logger)
        {
            _configuration = configuration;
            _logger = logger;
            var host = _configuration["DBHOST"] ?? "localhost";
            var port = _configuration["DBPORT"] ?? "3306";
            var password = _configuration["MYSQL_PASSWORD"] ?? _configuration.GetConnectionString("MYSQL_PASSWORD");
            var userid = _configuration["MYSQL_USER"] ?? _configuration.GetConnectionString("MYSQL_USER");
            var usersDataBase = _configuration["MYSQL_DATABASE"] ?? _configuration.GetConnectionString("MYSQL_DATABASE");

            connString = $"server={host}; userid={userid};pwd={password};port={port};database={usersDataBase}";
        }
        [HttpGet("GetAllUsers")]
        public async Task<ActionResult<List<UsersDto>>> GetAllUsers()
        {
            var users = new List<UsersDto>();
            try
            {
                string query = @"SELECT * FROM Users";
                using (var connection = new MySqlConnection(connString))
                {
                    var result = await connection.QueryAsync<UsersDto>(query, CommandType.Text);
                    users = result.ToList();
                }
                if (users.Count > 0)
                {
                    return Ok(users);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Unable To Process Request"+ ex.ToString());
            }
        }

        [HttpPost("AddNewUser")]
        public async Task<ActionResult<UsersDto>> AddNewUser(UsersDto user)
        {
            var newUser = new UsersDto();
            try
            {
                string query = @"INSERT INTO Users (UserName,Hobbies,Location) VALUES (@UserName,@Hobbies,@Location)";
                var param = new DynamicParameters();
                param.Add("@UserName", user.UserName);
                param.Add("@Hobbies", user.Hobbies);
                param.Add("@Location", user.Location);
                using (var connection = new MySqlConnection(connString))
                {
                    var result = await connection.ExecuteAsync(query, param, null, null, CommandType.Text);
                    if (result > 0)
                    {
                        newUser = user;
                    }
                }
                if (newUser != null)
                {
                    return Ok(newUser);
                }
                else
                {
                    return BadRequest("Unable To  User");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Unable To Process Request" + ex.ToString());
            }
        }
    }
}