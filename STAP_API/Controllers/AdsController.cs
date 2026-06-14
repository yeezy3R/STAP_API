using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

[ApiController]
[Route("api/[controller]")]
public class AdsController : ControllerBase
{
    private readonly IMongoCollection<Ad> _ads;

    public AdsController(MongoService mongo)
    {
        _ads = mongo.GetCollection<Ad>("Ads");
    }

    // CRIAR ANÚNCIO
    [HttpPost]
    public IActionResult Create(Ad ad)
    {
        _ads.InsertOne(ad);
        return Ok(new { message = "Anúncio criado com sucesso", ad });
    }

    // LISTAR TODOS
    [HttpGet]
    public IActionResult GetAll()
    {
        var list = _ads.Find(_ => true)
                       .SortByDescending(a => a.CreatedAt)
                       .ToList();

        return Ok(list);
    }

    // LISTAR POR UTILIZADOR
    [HttpGet("user/{userId}")]
    public IActionResult GetByUser(string userId)
    {
        var list = _ads.Find(a => a.UserId == userId)
                       .SortByDescending(a => a.CreatedAt)
                       .ToList();

        return Ok(list);
    }

    // ➤ FILTROS AVANÇADOS
    [HttpGet("filter")]
    public IActionResult Filter(
        string? title,
        string? category,
        string? location,
        decimal? minPrice,
        decimal? maxPrice
    )
    {
        var filter = Builders<Ad>.Filter.Empty;
        var filters = new List<FilterDefinition<Ad>>();

        if (!string.IsNullOrEmpty(title))
            filters.Add(Builders<Ad>.Filter.Regex(a => a.Title, new MongoDB.Bson.BsonRegularExpression(title, "i")));

        if (!string.IsNullOrEmpty(category))
            filters.Add(Builders<Ad>.Filter.Eq(a => a.Category, category));

        if (!string.IsNullOrEmpty(location))
            filters.Add(Builders<Ad>.Filter.Regex(a => a.Location, new MongoDB.Bson.BsonRegularExpression(location, "i")));

        if (minPrice.HasValue)
            filters.Add(Builders<Ad>.Filter.Gte(a => a.Price, minPrice.Value));

        if (maxPrice.HasValue)
            filters.Add(Builders<Ad>.Filter.Lte(a => a.Price, maxPrice.Value));

        if (filters.Count > 0)
            filter = Builders<Ad>.Filter.And(filters);

        var result = _ads.Find(filter)
                         .SortByDescending(a => a.CreatedAt)
                         .ToList();

        return Ok(result);
    }

    // ➤ FILTRO POR MÚLTIPLAS CATEGORIAS
    [HttpGet("filter-multi")]
    public IActionResult FilterMultipleCategories(
        [FromQuery] List<string>? categories,
        string? title,
        string? location,
        decimal? minPrice,
        decimal? maxPrice
    )
    {
        var filter = Builders<Ad>.Filter.Empty;
        var filters = new List<FilterDefinition<Ad>>();

        // ➤ TÍTULO (texto)
        if (!string.IsNullOrEmpty(title))
            filters.Add(Builders<Ad>.Filter.Regex(a => a.Title, new MongoDB.Bson.BsonRegularExpression(title, "i")));

        // ➤ LOCALIZAÇÃO
        if (!string.IsNullOrEmpty(location))
            filters.Add(Builders<Ad>.Filter.Regex(a => a.Location, new MongoDB.Bson.BsonRegularExpression(location, "i")));

        // ➤ PREÇO MÍNIMO
        if (minPrice.HasValue)
            filters.Add(Builders<Ad>.Filter.Gte(a => a.Price, minPrice.Value));

        // ➤ PREÇO MÁXIMO
        if (maxPrice.HasValue)
            filters.Add(Builders<Ad>.Filter.Lte(a => a.Price, maxPrice.Value));

        // ➤ MÚLTIPLAS CATEGORIAS
        if (categories != null && categories.Count > 0)
            filters.Add(Builders<Ad>.Filter.In(a => a.Category, categories));

        // ➤ SE EXISTEM FILTROS, APLICA
        if (filters.Count > 0)
            filter = Builders<Ad>.Filter.And(filters);

        var result = _ads.Find(filter)
                         .SortByDescending(a => a.CreatedAt)
                         .ToList();

        return Ok(result);
    }

    // ➤ FILTROS + PAGINAÇÃO
    [HttpGet("paged")]
    public IActionResult GetPaged(
        int page = 1,
        int pageSize = 10,
        string? title = null,
        string? category = null,
        string? location = null,
        decimal? minPrice = null,
        decimal? maxPrice = null
    )
    {
        var filter = Builders<Ad>.Filter.Empty;
        var filters = new List<FilterDefinition<Ad>>();

        if (!string.IsNullOrEmpty(title))
            filters.Add(Builders<Ad>.Filter.Regex(a => a.Title, new MongoDB.Bson.BsonRegularExpression(title, "i")));

        if (!string.IsNullOrEmpty(category))
            filters.Add(Builders<Ad>.Filter.Eq(a => a.Category, category));

        if (!string.IsNullOrEmpty(location))
            filters.Add(Builders<Ad>.Filter.Regex(a => a.Location, new MongoDB.Bson.BsonRegularExpression(location, "i")));

        if (minPrice.HasValue)
            filters.Add(Builders<Ad>.Filter.Gte(a => a.Price, minPrice.Value));

        if (maxPrice.HasValue)
            filters.Add(Builders<Ad>.Filter.Lte(a => a.Price, maxPrice.Value));

        if (filters.Count > 0)
            filter = Builders<Ad>.Filter.And(filters);

        // ➤ PAGINAÇÃO
        int skip = (page - 1) * pageSize;

        var result = _ads.Find(filter)
                         .SortByDescending(a => a.CreatedAt)
                         .Skip(skip)
                         .Limit(pageSize)
                         .ToList();

        // ➤ TOTAL DE REGISTOS (para saber quantas páginas existem)
        long total = _ads.CountDocuments(filter);

        return Ok(new
        {
            page,
            pageSize,
            total,
            totalPages = (int)Math.Ceiling((double)total / pageSize),
            data = result
        });
    }
    // ➤ FILTROS + MÚLTIPLAS CATEGORIAS + PAGINAÇÃO + ORDENAÇÃO
    [HttpGet("filter-multi-paged-sorted")]
    public IActionResult FilterMultiPagedSorted(
        [FromQuery] List<string>? categories,
        string? title,
        string? location,
        decimal? minPrice,
        decimal? maxPrice,
        string? sort = "newest",
        int page = 1,
        int pageSize = 10
    )
    {
        var filter = Builders<Ad>.Filter.Empty;
        var filters = new List<FilterDefinition<Ad>>();

        // ➤ TÍTULO
        if (!string.IsNullOrEmpty(title))
            filters.Add(Builders<Ad>.Filter.Regex(a => a.Title,
                new MongoDB.Bson.BsonRegularExpression(title, "i")));

        // ➤ LOCALIZAÇÃO
        if (!string.IsNullOrEmpty(location))
            filters.Add(Builders<Ad>.Filter.Regex(a => a.Location,
                new MongoDB.Bson.BsonRegularExpression(location, "i")));

        // ➤ PREÇO MÍNIMO
        if (minPrice.HasValue)
            filters.Add(Builders<Ad>.Filter.Gte(a => a.Price, minPrice.Value));

        // ➤ PREÇO MÁXIMO
        if (maxPrice.HasValue)
            filters.Add(Builders<Ad>.Filter.Lte(a => a.Price, maxPrice.Value));

        // ➤ MÚLTIPLAS CATEGORIAS
        if (categories != null && categories.Count > 0)
            filters.Add(Builders<Ad>.Filter.In(a => a.Category, categories));

        // ➤ APLICAR FILTROS
        if (filters.Count > 0)
            filter = Builders<Ad>.Filter.And(filters);

        // ➤ PAGINAÇÃO
        int skip = (page - 1) * pageSize;

        // ➤ ORDENAÇÃO
        SortDefinition<Ad> sortDefinition = sort switch
        {
            "price_asc" => Builders<Ad>.Sort.Ascending(a => a.Price),
            "price_desc" => Builders<Ad>.Sort.Descending(a => a.Price),
            "oldest" => Builders<Ad>.Sort.Ascending(a => a.CreatedAt),
            _ => Builders<Ad>.Sort.Descending(a => a.CreatedAt) // newest (default)
        };

        var data = _ads.Find(filter)
                       .Sort(sortDefinition)
                       .Skip(skip)
                       .Limit(pageSize)
                       .ToList();

        long total = _ads.CountDocuments(filter);

        return Ok(new
        {
            page,
            pageSize,
            total,
            totalPages = (int)Math.Ceiling((double)total / pageSize),
            data
        });
    }


    // ➤ FILTROS + MÚLTIPLAS CATEGORIAS + PAGINAÇÃO
    [HttpGet("filter-multi-paged")]
    public IActionResult FilterMultiPaged(
        [FromQuery] List<string>? categories,
        string? title,
        string? location,
        decimal? minPrice,
        decimal? maxPrice,
        int page = 1,
        int pageSize = 10
    )
    {
        var filter = Builders<Ad>.Filter.Empty;
        var filters = new List<FilterDefinition<Ad>>();

        // ➤ TÍTULO
        if (!string.IsNullOrEmpty(title))
            filters.Add(Builders<Ad>.Filter.Regex(a => a.Title,
                new MongoDB.Bson.BsonRegularExpression(title, "i")));

        // ➤ LOCALIZAÇÃO
        if (!string.IsNullOrEmpty(location))
            filters.Add(Builders<Ad>.Filter.Regex(a => a.Location,
                new MongoDB.Bson.BsonRegularExpression(location, "i")));

        // ➤ PREÇO MÍNIMO
        if (minPrice.HasValue)
            filters.Add(Builders<Ad>.Filter.Gte(a => a.Price, minPrice.Value));

        // ➤ PREÇO MÁXIMO
        if (maxPrice.HasValue)
            filters.Add(Builders<Ad>.Filter.Lte(a => a.Price, maxPrice.Value));

        // ➤ MÚLTIPLAS CATEGORIAS
        if (categories != null && categories.Count > 0)
            filters.Add(Builders<Ad>.Filter.In(a => a.Category, categories));

        // ➤ APLICAR FILTROS
        if (filters.Count > 0)
            filter = Builders<Ad>.Filter.And(filters);

        // ➤ PAGINAÇÃO
        int skip = (page - 1) * pageSize;

        var data = _ads.Find(filter)
                       .SortByDescending(a => a.CreatedAt)
                       .Skip(skip)
                       .Limit(pageSize)
                       .ToList();

        long total = _ads.CountDocuments(filter);

        return Ok(new
        {
            page,
            pageSize,
            total,
            totalPages = (int)Math.Ceiling((double)total / pageSize),
            data
        });
    }




    // BUSCAR POR ID
    [HttpGet("{id}")]
    public IActionResult GetById(string id)
    {
        var ad = _ads.Find(a => a.Id == id).FirstOrDefault();
        if (ad == null)
            return NotFound(new { message = "Anúncio não encontrado" });

        return Ok(ad);
    }

    // EDITAR
    [HttpPut("{id}")]
    public IActionResult Update(string id, Ad updated)
    {
        var ad = _ads.Find(a => a.Id == id).FirstOrDefault();
        if (ad == null)
            return NotFound(new { message = "Anúncio não existe" });

        updated.Id = id;
        _ads.ReplaceOne(a => a.Id == id, updated);

        return Ok(new { message = "Anúncio atualizado", updated });
    }

    // APAGAR
    [HttpDelete("{id}")]
    public IActionResult Delete(string id)
    {
        var ad = _ads.Find(a => a.Id == id).FirstOrDefault();
        if (ad == null)
            return NotFound(new { message = "Anúncio não existe" });

        _ads.DeleteOne(a => a.Id == id);

        return Ok(new { message = "Anúncio apagado" });
    }

}
