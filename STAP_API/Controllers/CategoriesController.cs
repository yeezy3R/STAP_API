using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IMongoCollection<Category> _categories;

    public CategoriesController(MongoService mongo)
    {
        _categories = mongo.GetCollection<Category>("Categories");
    }

    // ➤ LISTAR TODAS AS CATEGORIAS
    [HttpGet]
    public IActionResult GetAll()
    {
        var list = _categories.Find(_ => true).ToList();
        return Ok(list);
    }

    // ➤ CRIAR CATEGORIA
    [HttpPost]
    public IActionResult Create(Category category)
    {
        _categories.InsertOne(category);
        return Ok(new { message = "Categoria criada", category });
    }

    // ➤ BUSCAR POR ID
    [HttpGet("{id}")]
    public IActionResult GetById(string id)
    {
        var category = _categories.Find(c => c.Id == id).FirstOrDefault();
        if (category == null)
            return NotFound(new { message = "Categoria não encontrada" });

        return Ok(category);
    }

    // ➤ EDITAR CATEGORIA
    [HttpPut("{id}")]
    public IActionResult Update(string id, Category updated)
    {
        var category = _categories.Find(c => c.Id == id).FirstOrDefault();
        if (category == null)
            return NotFound(new { message = "Categoria não existe" });

        updated.Id = id;
        _categories.ReplaceOne(c => c.Id == id, updated);

        return Ok(new { message = "Categoria atualizada", updated });
    }

    // ➤ APAGAR CATEGORIA
    [HttpDelete("{id}")]
    public IActionResult Delete(string id)
    {
        var category = _categories.Find(c => c.Id == id).FirstOrDefault();
        if (category == null)
            return NotFound(new { message = "Categoria não existe" });

        _categories.DeleteOne(c => c.Id == id);

        return Ok(new { message = "Categoria apagada" });
    }
}
