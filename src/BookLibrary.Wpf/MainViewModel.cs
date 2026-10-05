using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace BookLibrary.Wpf;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly HttpClient _httpClient = new() { BaseAddress = new Uri("http://localhost:5070") };
    private Book _newBook = new();
    private string _status = "Loading...";

    public ObservableCollection<Book> Books { get; } = new();
    public Book NewBook { get => _newBook; set { _newBook = value; OnPropertyChanged(); } }
    public string Status { get => _status; private set { _status = value; OnPropertyChanged(); } }
    public ICommand AddCommand { get; }
    public ICommand DeleteCommand { get; }

    public MainViewModel()
    {
        AddCommand = new AsyncCommand(AddAsync);
        DeleteCommand = new AsyncCommand<Book>(DeleteAsync);
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        try { Books.Clear(); foreach (var book in await _httpClient.GetFromJsonAsync<List<Book>>("api/books") ?? []) Books.Add(book); Status = $"{Books.Count} book(s)"; }
        catch (Exception ex) { Status = ex.Message; }
    }

    private async Task AddAsync()
    {
        try { var response = await _httpClient.PostAsJsonAsync("api/books", NewBook); response.EnsureSuccessStatusCode(); NewBook = new Book(); await LoadAsync(); }
        catch (Exception ex) { Status = ex.Message; }
    }

    private async Task DeleteAsync(Book? book)
    {
        if (book is null) return;
        try { (await _httpClient.DeleteAsync($"api/books/{book.Id}")).EnsureSuccessStatusCode(); await LoadAsync(); }
        catch (Exception ex) { Status = ex.Message; }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));
}

public sealed class AsyncCommand(Func<Task> execute) : ICommand
{
    public event EventHandler? CanExecuteChanged { add { } remove { } }
    public bool CanExecute(object? parameter) => true;
    public async void Execute(object? parameter) => await execute();
}

public sealed class AsyncCommand<T>(Func<T?, Task> execute) : ICommand
{
    public event EventHandler? CanExecuteChanged { add { } remove { } }
    public bool CanExecute(object? parameter) => true;
    public async void Execute(object? parameter) => await execute((T?)parameter);
}
