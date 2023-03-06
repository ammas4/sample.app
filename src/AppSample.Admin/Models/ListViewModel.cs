namespace AppSample.Admin.Models;

public class ListViewModel<T> where T : class
{
    public List<T> Items { get; set; }
}