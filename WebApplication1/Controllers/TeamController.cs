using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using WebApplication1;

namespace Hotel.ATR.WebApi.Controllers
{
    //[Route("api/[controller]")]
    [Route("api/team")]
    [ApiController]
    public class TeamController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<TeamController> _logger;

        public static List<Team> teams = new List<Team>();
        public TeamController(ILogger<TeamController> logger, AppDbContext db)
        {
            _logger = logger;
            _db = db;

            teams.Add(new Team("Kathy Luis", "", "Lorem ipsupm dolor sit amet", "Officer"));
            teams.Add(new Team("Them Jonse", "", "Lorem ipsupm dolor sit amet", "Manager"));
            teams.Add(new Team("Marry Gomej", "", "Lorem ipsupm dolor sit amet", "Leader"));
            teams.Add(new Team("Noah Jackson", "", "Lorem ipsupm dolor sit amet", "Officer"));
        }

        [HttpGet]
        [Route("[action]")]
        [Route("/get-all-teams")]
        public IEnumerable<Team> GetAllItems()
        {

            //IEnumerable<Team> data = _db.Teams.ToList();
            //var test = data.Where(w => w.FullName == "");

            _logger.LogWarning("USER TRY TO GET DATA");
            return _db.Teams.ToList();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Post([FromForm] Team team)
        {
            if (team == null)
                //return BadRequest();
                //return BadRequest("Отсутствуют данные!");
                return BadRequest(new { ErrorMessage = "Отсутствуют данные!", IsError = true });

            try
            {
                teams.Add(team);
                return Ok(new { ErrorMessage = "Данные успешно добавлены!", IsError = false });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ErrorMessage = ex.Message, IsError = true });
            }
        }


        [HttpDelete()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return BadRequest(new { ErrorMessage = "FullName указан не корректно", IsError = true });

            var data = teams.FirstOrDefault(f => f.FullName.Equals(fullName));

            if (data != null)
            {
                teams.Remove(data);
                return Ok(new { ErrorMessage = "Данные удалены", IsError = false });
            }
            else
            {
                return NotFound(new { ErrorMessage = "Данные не найдены", IsError = true });
            }

        }

        [HttpPut]
        public IActionResult Put([FromBody] Team team)
        {
            var data = teams.FirstOrDefault(f => f.FullName.Equals(team.FullName));

            if (data != null)
            {
                data.FullName = team.FullName;
                data.PositionName = team.PositionName;
                data.PathImage = team.PathImage;
                data.Description = team.Description;

                return Ok(new { ErrorMessage = "Данные обновлены", IsError = false });
            }
            else
            {
                return NotFound(new { ErrorMessage = "Данные не найдены", IsError = true });
            }
        }

        [HttpPatch]
        public IActionResult Patch(string fullName, [FromBody] JsonPatchDocument<Team> jsonPatch)
        {
            var res = teams.FirstOrDefault(f => f.FullName.Equals(fullName));
            if (res != null)
            {
                jsonPatch.ApplyTo(res);
                return Ok();
            }
            return NotFound();
        }

    }
}