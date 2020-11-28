namespace DataAccessLayer.Contexts
{
    public interface IPath
    {
        string GetDatabasePath(string fileName);
    }
}