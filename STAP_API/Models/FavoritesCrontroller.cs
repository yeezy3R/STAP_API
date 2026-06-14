using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class FavoritesController : ControllerBase
{
    private readonly IMongoCollection<Favorite> _favorites;
    private readonly IMongoCollection<Ad> _ads;

    public FavoritesController(MongoService mongo)
    {
        _favorites = mongo.GetCollection<Favorite>("Favorites");
        _ads = mongo.GetCollection<Ad>("Ads");
    }

    // ➤ ADICIONAR FAVORITO
    [HttpPost]
    public IActionResult AddFavorite(Favorite fav)
    {
        // Evitar duplicados
        var exists = _favorites.Find(f => f.UserId == fav.UserId && f.AdId == fav.AdId).FirstOrDefault();
        if (exists != null)
            return BadRequest(new { message = "Já está nos favoritos" });

        _favorites.InsertOne(fav);
        return Ok(new { message = "Adicionado aos favoritos", fav });
    }

    // ➤ REMOVER FAVORITO
    [HttpDelete("{userId}/{adId}")]
    public IActionResult RemoveFavorite(string userId, string adId)
    {
        _favorites.DeleteOne(f => f.UserId == userId && f.AdId == adId);
        return Ok(new { message = "Removido dos favoritos" });
    }

    // ➤ LISTAR FAVORITOS DO UTILIZADOR (COM ANÚNCIOS)
    [HttpGet("{userId}")]
    public IActionResult GetFavorites(string userId)
    {
        var favs = _favorites.Find(f => f.UserId == userId).ToList();

        var adIds = favs.Select(f => f.AdId).ToList();

        var ads = _ads.Find(a => adIds.Contains(a.Id)).ToList();

        return Ok(ads);
    }
}
