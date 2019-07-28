using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> Get()
        {
            List<string> unames = new List<string>();
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = "GMRMLTV";
            builder.UserID = "sa";
            builder.Password = "GreatMinds110";
            builder.InitialCatalog = "ThadTest";

            using (SqlConnection connection = new SqlConnection(builder.ToString()))
            using (SqlCommand command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandType = System.Data.CommandType.Text;
                command.CommandText = "waitfor delay '00:00:05'; select * from UserData";

                await connection.OpenAsync();

                SqlDataReader reader = await command.ExecuteReaderAsync();

                while(await reader.ReadAsync())
                {
                    string uname = await reader.GetFieldValueAsync<string>(0);
                    unames.Add(uname);
                    Debug.WriteLine(uname);
                }
            }

            return unames;

        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
