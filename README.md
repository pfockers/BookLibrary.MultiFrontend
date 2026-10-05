# BookLibrary.MultiFrontend

A sample Book Library with one ASP.NET Core backend and three clients:

- `src/BookLibrary.Api` - ASP.NET Core minimal API with an in-memory store
- `src/BookLibrary.React` - React + TypeScript + Vite
- `src/BookLibrary.Wpf` - WPF using MVVM and `HttpClient`
- `src/BookLibrary.Blazor` - Blazor WebAssembly

## Run the API

```powershell
dotnet run --project src/BookLibrary.Api --urls http://localhost:5070
```

The API is available at `http://localhost:5070/api/books`.

## Run React

```powershell
cd src/BookLibrary.React
npm install
npm run dev
```

## Run WPF

With the API running:

```powershell
dotnet run --project src/BookLibrary.Wpf
```

## Run Blazor

With the API running:

```powershell
dotnet run --project src/BookLibrary.Blazor
```

All clients use the same API and support adding and deleting books. The API data is intentionally in-memory and resets when the API restarts.
