namespace Databases.App.Models;
public class Config
{
    public Config()
    {
        this.Databases = new List<Database>();
    }
    public List<Database> Databases { get; set; }
}