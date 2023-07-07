using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Threading.Tasks;
using System;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.RegularExpressions;

var client = new MongoClient("mongodb://localhost:27017");
var database = client.GetDatabase("Zadanie");
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseStaticFiles();
// ładowanie stron
app.MapGet("/", async (context) =>
{
    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html");
    var fileContent = await File.ReadAllTextAsync(filePath);
    var collection = database.GetCollection<BsonDocument>("hoteles");
    var filter = new BsonDocument(); 
    var result = collection.Find(filter).ToList(); 
    var html = string.Empty;
    foreach (var doc in result)
    {
        var slug = doc.GetValue("slug").AsString;
        var name = doc.GetValue("name").AsString;
        var place = doc.GetValue("place").AsString;
        var price = doc.GetValue("price").AsString;
        var rate = doc.GetValue("rate").AsString;

       
        html += $@"
        <a href='/{slug}'>
          <div class='hotel-item'>
            <div class='hotel-img'>
              <img src='https://cf.bstatic.com/xdata/images/hotel/max1024x768/339008917.jpg?k=cf204230bfee436cc523d115686834fd8be0de655b0dfb003ce9f140665820c2&o=&hp=1' alt='Zdjęcie hotelu' />
            </div>
            <div class='hotel-info'>
              <div class='hotel-name'>{name}</div>
              <div class='hotel-rate'>{rate}</div>
              <div class='hotel-label'>
                <div>Miejscowość:</div>
                <div class='hotel-place'>{place}</div>
              </div>
              <div class='hotel-label'>
                <div>Cena za dobe:</div>
                <div><span class='hotel-price'>{price}</span> zł</div>
              </div>
            </div>
          </div>
        </a>
    ";
    }
    fileContent = fileContent.Replace("{html}", html);
    await context.Response.WriteAsync(fileContent);
});

app.MapGet("/logowanie", async (context) =>
{
    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logowanie.html");
    var fileContent = await File.ReadAllTextAsync(filePath);
    await context.Response.WriteAsync(fileContent);
});
app.MapGet("/error", async (context) =>
{
    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "error.html");
    var fileContent = await File.ReadAllTextAsync(filePath);
    await context.Response.WriteAsync(fileContent);
});
app.MapGet("/rejestracja", async (context) =>
{
    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "rejestracja.html");
    var fileContent = await File.ReadAllTextAsync(filePath);
    await context.Response.WriteAsync(fileContent);
});
app.MapGet("/profil-dodawanie", async (context) =>
{
    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "profil-dodawanie.html");
    var fileContent = await File.ReadAllTextAsync(filePath);
    var profil_id = context.Request.Query["id"].ToString();
    fileContent = fileContent.Replace("{id}", profil_id);
    await context.Response.WriteAsync(fileContent);
});
app.MapGet("/usun", async (context) =>
{
    var slug = context.Request.Query["slug"].ToString();
    var id = context.Request.Query["id"].ToString();
    var collection = database.GetCollection<BsonDocument>("hoteles");
    var filter = Builders<BsonDocument>.Filter.Eq("slug", slug);
    var result = await collection.DeleteOneAsync(filter);
    
    if (result.DeletedCount > 0)
    {
        context.Response.Redirect("/profil?id=" + id);
    }
});

app.MapGet("/profil", async (context) =>
{
    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "profil.html");
    var fileContent = await File.ReadAllTextAsync(filePath);
    var profil_id = context.Request.Query["id"].ToString();
    var collection = database.GetCollection<BsonDocument>("users");
    var filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(profil_id));
    var result = collection.Find(filter).FirstOrDefault();

    fileContent = fileContent.Replace("{id}", result["_id"].ToString());
    fileContent = fileContent.Replace("{lastname}", result["lastname"].ToString());
    fileContent = fileContent.Replace("{name}", result["name"].ToString());
    fileContent = fileContent.Replace("{login}", result["login"].ToString());


    var collection2 = database.GetCollection<BsonDocument>("hoteles");
    var filter2 = Builders<BsonDocument>.Filter.Eq("profil_id", profil_id);
    var result2 = collection2.Find(filter2).ToList();
    var html = string.Empty;

    foreach (var doc in result2)
    {
        var slug = doc.GetValue("slug").AsString;
        var name = doc.GetValue("name").AsString;
        var place = doc.GetValue("place").AsString;
        var price = doc.GetValue("price").AsString;
        var rate = doc.GetValue("rate").AsString;

        html += $@"
  <div class='p-hotel-item'>
    <div class='item-box-img'>
      <img src='https://cf.bstatic.com/xdata/images/hotel/max1024x768/339008917.jpg?k=cf204230bfee436cc523d115686834fd8be0de655b0dfb003ce9f140665820c2&o=&hp=1' alt='' />
    </div>
    <div class='item-box-info'>
      <div class='item-info-title'>
        <a href=''><h2 class='p-titles'>{name}</h2></a>
      </div>
      <div class='item-info-data'>
        <div class='item-info-group'>
          <div class='item-label'>Miejscowość:</div>
          <p id='place'>{place}</p>
        </div>
        <div class='item-info-group'>
          <div class='item-label'>Cena za dobe:</div>
          <p id='price'>{price}<span> zł</span></p>
        </div>
        <div class='item-info-group'>
          <div class='item-label'>Ocena:</div>
          <p id='rate'>{rate}</p>
        </div>
      </div>
    </div>
    <div class='item-box-buttons'>
      <a href='/usun?id={profil_id}&slug={slug}'><button class='btn btn-delete'>Usuń hotel</button></a>
    </div>
  </div>
";


    }
    fileContent = fileContent.Replace("{html}", html);
    await context.Response.WriteAsync(fileContent);
});
app.MapGet("/{nazwa}", async (context) =>
{
    var nazwa = context.Request.RouteValues["nazwa"]?.ToString();
    var collection = database.GetCollection<BsonDocument>("hoteles");
    var filter = Builders<BsonDocument>.Filter.Eq("slug", nazwa);
    var result = collection.Find(filter).FirstOrDefault();

    if (result != null)
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "wpis.html");
        var fileContent = await File.ReadAllTextAsync(filePath);
        fileContent = fileContent.Replace("{map}", result["map"].ToString());
        fileContent = fileContent.Replace("{name}", result["name"].ToString());
        fileContent = fileContent.Replace("{place}", result["place"].ToString());
        fileContent = fileContent.Replace("{price}", result["price"].ToString());
        fileContent = fileContent.Replace("{rate}", result["rate"].ToString());
        fileContent = fileContent.Replace("{desc}", result["desc"].ToString());

        await context.Response.WriteAsync(fileContent);
    }
    else
    {
        if (nazwa == "/")
        {
            context.Response.Redirect("/");
        }
        else if (nazwa == "/logowanie")
        {
            context.Response.Redirect("/logowanie");
        }
        else if (nazwa == "/rejestracja")
        {
            context.Response.Redirect("/rejestracja");
        }
        else if (nazwa == "/profil")
        {
            context.Response.Redirect("/profil");
        }
         else if (nazwa == "/usun")
        {
            context.Response.Redirect("/usun");
        }
        else if (nazwa == "/profil-dodawanie")
        {
            context.Response.Redirect("/profil-dodawanie");
        }
        else
        {
            context.Response.Redirect("/error");
        }
    }
});
// odbieranie danych z stron
app.MapPost("/logowanie", async (context) =>
{
    var form = await context.Request.ReadFormAsync();
    var username = form["name"].ToString();
    var password = form["password"].ToString();
    var collection = database.GetCollection<BsonDocument>("users");
    var document = collection.Find(new BsonDocument()).FirstOrDefault();
    var filter = Builders<BsonDocument>.Filter.Eq("login", username) & Builders<BsonDocument>.Filter.Eq("password", password);
    var result = collection.Find(filter).FirstOrDefault();
    if (result != null)
    {
        context.Response.Redirect($"/profil?id={result["_id"]}");
    }
    else
    {
        context.Response.Redirect("/error");
    }
});

app.MapPost("/rejestracja", async (context) =>
{
    var collection = database.GetCollection<BsonDocument>("users");
    var form = await context.Request.ReadFormAsync();
    var username = form["user"].ToString();
    var password = form["password"].ToString();
    var name = form["name"].ToString();
    var lastname = form["lastName"].ToString();

    var document = new BsonDocument
{
    { "login", username },
    { "password", password },
    { "name", name },
    { "lastname", lastname },
    { "rights", 2 }
};
    collection.InsertOne(document);
    context.Response.Redirect("/logowanie");
});
app.MapPost("/profil-dodawanie", async (context) =>
{
    var collection = database.GetCollection<BsonDocument>("hoteles");
    var form = await context.Request.ReadFormAsync();
    var profil_id = context.Request.Query["id"].ToString(); 
    var name = form["name"].ToString();
    var slug = Regex.Replace(name, @"[^a-zA-Z0-9\s-]", "", RegexOptions.Compiled);
    slug = Regex.Replace(slug, @"\s+", "-").ToLower();
    var place = form["place"].ToString();
    var price = form["price"].ToString();
    var rate = form["rate"].ToString();
    var faci = form["faci"].ToString();
    var desc = form["desc"].ToString();
    var map = form["map"].ToString();

    var document = new BsonDocument
{
    { "name", name },
    { "slug", slug },
    { "place", place },
    { "price", price },
    { "rate", rate },
    { "faci", faci },
    { "desc", desc },
    { "map", map },
    { "profil_id", profil_id }
};
    collection.InsertOne(document);
    context.Response.Redirect("/profil?id=" + profil_id);
});
app.Run();
