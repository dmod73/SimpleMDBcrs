namespace Smdb.Core.Movies;

// Modelo principal de película
public class Movie
{
    // Propiedades básicas del dominio
    public int Id { get; set; }
    public string Title { get; set; }
    public int Year { get; set; }
    public string Description { get; set; }

    // Constructor principal usado para crear instancias completas
    public Movie(int id, string title, int year, string description)
    {
        Id = id;
        Title = title;
        Year = year;
        Description = description;
    }

    // Representación en texto (útil para debugging)
    public override string ToString()
    {
        return $"Movie[Id={Id}, Title={Title}, Year={Year}, Description={Description}]";
    }
}
