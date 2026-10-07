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
  const [search, setSearch] = useState('')
  const [editingId, setEditingId] = useState<number | null>(null)
  const [loading, setLoading] = useState(false)

  const loadBooks = async (query = search) => {
    setLoading(true)
    const response = await fetch(`${apiUrl}?search=${encodeURIComponent(query)}`)
    if (!response.ok) throw new Error('Could not load books.')
    setBooks(await response.json())
    setLoading(false)
  }

  useEffect(() => { loadBooks().catch((reason: Error) => { setLoading(false); setError(reason.message) }) }, [search])

  const addBook = async (event: FormEvent) => {
    event.preventDefault()
    setError('')
    const url = editingId === null ? apiUrl : `${apiUrl}/${editingId}`
    const response = await fetch(url, { method: editingId === null ? 'POST' : 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(form) })
    if (!response.ok) { setError(await response.text()); return }
    setForm(emptyForm)
    setEditingId(null)
    await loadBooks()
  }

  const editBook = (book: Book) => { setEditingId(book.id); setForm({ ...book }); setError('') }

  const borrowBook = async (id: number) => {
    const borrowerName = window.prompt('Borrower name')
    if (!borrowerName) return
    const response = await fetch(`${apiUrl}/${id}/loans`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ borrowerName }) })
    if (!response.ok) { setError(await response.text()); return }
    await loadBooks()
  }

  const deleteBook = async (id: number) => {
    await fetch(`${apiUrl}/${id}`, { method: 'DELETE' })
    await loadBooks()
  }

  return <main className="app-shell">
    <header className="app-header">
      <a className="brand" href="/" aria-label="Bookshelf home"><span className="brand-mark">B</span><span>BOOKSHELF</span></a>
      <span className="header-note">A little room for every story</span>
    </header>

    <section className="intro">
      <div>
        <p className="eyebrow">PERSONAL COLLECTION</p>
        <h1>Your library, <span>well kept.</span></h1>
        <p className="intro-copy">Keep your favorite reads close, and make space for the next one.</p>
      </div>
      <div className="collection-count"><strong>{books.length}</strong><span>books in<br />your collection</span></div>
    </section>

    <section className="panel add-panel" aria-labelledby="add-heading">
      <div className="panel-heading">
         <div><p className="eyebrow">GROW YOUR SHELF</p><h2 id="add-heading">{editingId === null ? 'Add a book' : 'Edit a book'}</h2></div>
        <span className="step-mark" aria-hidden="true">01</span>
      </div>
      <form onSubmit={addBook} className="book-form">
        {(['title', 'author', 'isbn', 'genre'] as const).map(field => <label className="field" key={field}>
          <span>{field === 'isbn' ? 'ISBN' : field[0].toUpperCase() + field.slice(1)}</span>
          <input required={field === 'title' || field === 'author'} value={form[field]} onChange={e => setForm({ ...form, [field]: e.target.value })} placeholder={field === 'title' ? 'e.g. The Secret Garden' : field === 'author' ? 'Author name' : field === 'isbn' ? '978-…' : 'e.g. Fiction'} />
        </label>)}
        <label className="field year-field"><span>Published year</span><input type="number" value={form.publishedYear} onChange={e => setForm({ ...form, publishedYear: Number(e.target.value) })} /></label>
        <label className="availability-field"><input type="checkbox" checked={form.isAvailable} onChange={e => setForm({ ...form, isAvailable: e.target.checked })} /><span>Available to borrow</span></label>
         <button className="primary-button" type="submit"><span aria-hidden="true">+</span> {editingId === null ? 'Add book' : 'Save changes'}</button>
         {editingId !== null && <button type="button" onClick={() => { setEditingId(null); setForm(emptyForm) }}>Cancel</button>}
      </form>
    </section>

    {error && <p className="error-message" role="alert">{error}</p>}

    <section className="collection-section" aria-labelledby="collection-heading">
      <div className="collection-heading">
        <div><p className="eyebrow">THE GOOD READS</p><h2 id="collection-heading">Your collection</h2></div>
        <span className="result-count">{books.length} {books.length === 1 ? 'BOOK' : 'BOOKS'}</span>
      </div>
      <label className="field"><span>Search</span><input value={search} onChange={e => setSearch(e.target.value)} placeholder="Title or author" /></label>
      {loading && <p>Loading...</p>}
      <div className="table-card">
        <div className="table-scroll"><table>
          <thead><tr><th>Title</th><th>Author</th><th>ISBN</th><th>Genre</th><th>Year</th><th>Availability</th><th><span className="visually-hidden">Actions</span></th></tr></thead>
          <tbody>{books.length === 0 ? <tr><td className="empty-state" colSpan={7}>Your shelf is waiting. Add your first book above.</td></tr> : books.map(book => <tr key={book.id}>
            <td className="book-title">{book.title}</td><td>{book.author}</td><td className="isbn-cell">{book.isbn}</td><td><span className="genre-tag">{book.genre}</span></td><td>{book.publishedYear}</td>
            <td><span className={`availability ${book.isAvailable ? 'is-available' : 'is-unavailable'}`}><span />{book.isAvailable ? 'Available' : 'On loan'}</span></td>
             <td><button onClick={() => editBook(book)}>Edit</button> <button disabled={!book.isAvailable} onClick={() => borrowBook(book.id)}>Borrow</button> <button className="delete-button" aria-label={`Delete ${book.title}`} onClick={() => deleteBook(book.id)}>Remove</button></td>
          </tr>)}</tbody>
        </table></div>
      </div>
    </section>
    <footer className="app-footer"><span>Made for the love of reading.</span><span>BOOKSHELF · YOUR PERSONAL LIBRARY</span></footer>
  </main>
}

export default App
