namespace PrefabDocumenter.DB
{
    public interface IModel
    {
        string InsertCommand { get; }
        string TableName { get; }
    }
}
