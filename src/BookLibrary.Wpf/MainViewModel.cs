using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace BookLibrary.Wpf;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly HttpClient _httpClient = new() { BaseAddress = new Uri("http://localhost:5211") };
    private Book _newBook = new();
    private string _status = "Loading...";
    private int? _editingId;

    public ObservableCollection<Book> Books { get; } = new();
    public Book NewBook { get => _newBook; set { _newBook = value; OnPropertyChanged(); } }
    public string Status { get => _status; private set { _status = value; OnPropertyChanged(); } }
    public ICommand AddCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand BorrowCommand { get; }

    public MainViewModel()
    {
        AddCommand = new AsyncCommand(AddAsync);
        DeleteCommand = new AsyncCommand<Book>(DeleteAsync);
        EditCommand = new AsyncCommand<Book>(EditAsync);
        BorrowCommand = new AsyncCommand<Book>(BorrowAsync);
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        try { Books.Clear(); foreach (var book in await _httpClient.GetFromJsonAsync<List<Book>>("api/books") ?? []) Books.Add(book); Status = $"{Books.Count} book(s)"; }
        catch (Exception ex) { Status = ex.Message; }
    }

    private async Task AddAsync()
    {
        try
        {
            var response = _editingId is null
                ? await _httpClient.PostAsJsonAsync("api/books", NewBook)
                : await _httpClient.PutAsJsonAsync($"api/books/{_editingId}", NewBook);
            response.EnsureSuccessStatusCode();
            _editingId = null;
            NewBook = new Book();
            await LoadAsync();
        }
        catch (Exception ex) { Status = ex.Message; }
    }

    private Task EditAsync(Book? book)
    {
        if (book is not null)
        {
            _editingId = book.Id;
            NewBook = new Book { Id = book.Id, Title = book.Title, Author = book.Author, Isbn = book.Isbn, Genre = book.Genre, PublishedYear = book.PublishedYear, IsAvailable = book.IsAvailable };
        }
        return Task.CompletedTask;
    }

    private async Task BorrowAsync(Book? book)
    {
        if (book is null || !book.IsAvailable) return;
        try
        {
            (await _httpClient.PostAsJsonAsync($"api/books/{book.Id}/loans", new { borrowerName = Environment.UserName })).EnsureSuccessStatusCode();
            await LoadAsync();
        }
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
