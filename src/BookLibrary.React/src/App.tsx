import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import './App.css'

type Book = {
  id: number
  title: string
  author: string
  isbn: string
  genre: string
  publishedYear: number
  isAvailable: boolean
}

type BookForm = Omit<Book, 'id'>
const apiUrl = 'http://localhost:5211/api/books'
const emptyForm: BookForm = { title: '', author: '', isbn: '', genre: '', publishedYear: new Date().getFullYear(), isAvailable: true }

function App() {
  const [books, setBooks] = useState<Book[]>([])
  const [form, setForm] = useState<BookForm>(emptyForm)
  const [error, setError] = useState('')

  const loadBooks = async () => {
    const response = await fetch(apiUrl)
    if (!response.ok) throw new Error('Could not load books.')
    setBooks(await response.json())
  }

  useEffect(() => { loadBooks().catch((reason: Error) => setError(reason.message)) }, [])

  const addBook = async (event: FormEvent) => {
    event.preventDefault()
    setError('')
    const response = await fetch(apiUrl, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(form) })
    if (!response.ok) { setError(await response.text()); return }
    setForm(emptyForm)
    await loadBooks()
  }

  const deleteBook = async (id: number) => {
    await fetch(`${apiUrl}/${id}`, { method: 'DELETE' })
    await loadBooks()
  }

  return <main>
    <h1>Book Library</h1>
    <form onSubmit={addBook} className="book-form">
      {(['title', 'author', 'isbn', 'genre'] as const).map(field => <label key={field}>{field[0].toUpperCase() + field.slice(1)}<input required={field === 'title' || field === 'author'} value={form[field]} onChange={e => setForm({ ...form, [field]: e.target.value })} /></label>)}
      <label>Published year<input type="number" value={form.publishedYear} onChange={e => setForm({ ...form, publishedYear: Number(e.target.value) })} /></label>
      <label className="checkbox"><input type="checkbox" checked={form.isAvailable} onChange={e => setForm({ ...form, isAvailable: e.target.checked })} /> Available</label>
      <button type="submit">Add book</button>
    </form>
    {error && <p className="error">{error}</p>}
    <table><thead><tr><th>Title</th><th>Author</th><th>ISBN</th><th>Genre</th><th>Year</th><th>Available</th><th /></tr></thead><tbody>{books.map(book => <tr key={book.id}><td>{book.title}</td><td>{book.author}</td><td>{book.isbn}</td><td>{book.genre}</td><td>{book.publishedYear}</td><td>{book.isAvailable ? 'Yes' : 'No'}</td><td><button onClick={() => deleteBook(book.id)}>Delete</button></td></tr>)}</tbody></table>
  </main>
}

export default App
