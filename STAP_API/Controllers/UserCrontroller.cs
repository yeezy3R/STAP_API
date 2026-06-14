using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMongoCollection<User> _users;

    public UsersController(MongoService mongo)
    {
        _users = mongo.GetCollection<User>("Users");
    }

    // REGISTO
    [HttpPost("register")]
    public IActionResult Register(User user)
    {
        var exists = _users.Find(u => u.Username == user.Username).FirstOrDefault();
        if (exists != null)
            return BadRequest(new { message = "Username já existe" });

        user.PasswordHash = PasswordHasher.Hash(user.PasswordHash);
        _users.InsertOne(user);

        return Ok(new { message = "Registo feito com sucesso" });
    }

    // LOGIN
    [HttpPost("login")]
    public IActionResult Login(User user, [FromServices] JwtService jwt)
    {
        var dbUser = _users.Find(u => u.Username == user.Username).FirstOrDefault();
        if (dbUser == null)
            return Unauthorized(new { message = "Utilizador não encontrado" });

        var hash = PasswordHasher.Hash(user.PasswordHash);

        if (hash != dbUser.PasswordHash)
            return Unauthorized(new { message = "Password incorreta" });

        var token = jwt.GenerateToken(dbUser.Id, dbUser.Username);

        return Ok(new
        {
            message = "Login OK",
            token = token,
            userId = dbUser.Id
        });
    }

}

