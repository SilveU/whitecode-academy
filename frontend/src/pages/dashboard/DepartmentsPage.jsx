import { useState, useEffect, useCallback } from 'react'
import { Plus, Search, Edit3, Trash2, FolderTree, X, Upload } from 'lucide-react'
import { departmentApi } from '../../services/api'

export default function DepartmentsPage() {
  const [departments, setDepartments] = useState([])
  const [loading, setLoading] = useState(true)
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)
  const [totalPages, setTotalPages] = useState(1)
  const [alert, setAlert] = useState(null)

  const [modalOpen, setModalOpen] = useState(false)
  const [editingDept, setEditingDept] = useState(null)
  const [form, setForm] = useState({ name: '', description: '' })
  const [imageFile, setImageFile] = useState(null)
  const [saving, setSaving] = useState(false)

  const loadDepartments = useCallback(async () => {
    setLoading(true)
    try {
      const res = await departmentApi.list({ pageNumber: page, pageSize: 10, wordForSearch: search || undefined })
      if (res.ok) {
        const data = res.data
        if (Array.isArray(data)) {
          setDepartments(data)
          setTotalPages(1)
        } else {
          setDepartments(data?.items || data?.data || [])
          setTotalPages(data?.totalPages || 1)
        }
      }
    } catch {
      // ignore
    } finally {
      setLoading(false)
    }
  }, [page, search])

  useEffect(() => { loadDepartments() }, [loadDepartments])

  const openCreate = () => {
    setEditingDept(null)
    setForm({ name: '', description: '' })
    setImageFile(null)
    setModalOpen(true)
  }

  const openEdit = (dept) => {
    setEditingDept(dept)
    setForm({ name: dept.name, description: dept.description })
    setImageFile(null)
    setModalOpen(true)
  }

  const handleSave = async () => {
    setSaving(true)
    try {
      const fd = new FormData()
      fd.append('Name', form.name)
      fd.append('Description', form.description)
      if (imageFile) fd.append('ImageFile', imageFile)

      let res
      if (editingDept) {
        res = await departmentApi.update(editingDept.id, fd)
      } else {
        res = await departmentApi.create(fd)
      }

      if (res.ok) {
        setModalOpen(false)
        setAlert({ type: 'success', msg: editingDept ? 'Department updated successfully' : 'Department created successfully' })
        loadDepartments()
      } else {
        setAlert({ type: 'error', msg: res.message || 'An error occurred' })
      }
    } catch {
      setAlert({ type: 'error', msg: 'Connection error' })
    } finally {
      setSaving(false)
    }
    setTimeout(() => setAlert(null), 4000)
  }

  const handleDelete = async (id) => {
    if (!window.confirm('Are you sure you want to delete this department?')) return
    const res = await departmentApi.delete(id)
    if (res.ok) {
      setAlert({ type: 'success', msg: 'Department deleted successfully' })
      loadDepartments()
    } else {
      setAlert({ type: 'error', msg: res.message || 'Error occurred during deletion' })
    }
    setTimeout(() => setAlert(null), 4000)
  }

  return (
    <div>
      <div className="page-header">
        <div className="page-header-info">
          <h1>Departments</h1>
          <p>Manage academy departments and categorize courses</p>
        </div>
        <div className="page-actions">
          <button className="btn-primary" onClick={openCreate}>
            <Plus size={16} /> New Department
          </button>
        </div>
      </div>

      {alert && <div className={`dash-alert dash-alert-${alert.type}`}>{alert.msg}</div>}

      <div className="data-table-wrapper">
        <div className="data-table-header">
          <div className="search-input-wrapper">
            <Search size={16} />
            <input placeholder="Search departments..." value={search} onChange={(e) => { setSearch(e.target.value); setPage(1) }} />
          </div>
        </div>

        {loading ? (
          <div className="loading-container"><div className="spinner" /></div>
        ) : departments.length === 0 ? (
          <div className="empty-state">
            <div className="empty-state-icon"><FolderTree size={28} /></div>
            <h3>No departments found</h3>
            <p>Start by creating the first department</p>
            <button className="btn-primary" onClick={openCreate}><Plus size={16} /> Add Department</button>
          </div>
        ) : (
          <>
            <table className="data-table">
              <thead>
                <tr>
                  <th>Department Name</th>
                  <th>Description</th>
                  <th>Image</th>
                  <th>Created Date</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {departments.map((d) => (
                  <tr key={d.id}>
                    <td style={{ fontWeight: 600, color: 'var(--text-primary)' }}>{d.name}</td>
                    <td style={{ maxWidth: '200px', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{d.description}</td>
                    <td>
                      {d.imageUrl ? (
                        <img src={d.imageUrl} alt={d.name} style={{ width: 36, height: 36, borderRadius: 8, objectFit: 'cover', border: '1px solid var(--border)' }} />
                      ) : (
                        <span style={{ color: 'var(--text-muted)', fontSize: '0.8rem' }}>No Image</span>
                      )}
                    </td>
                    <td>{d.createdAt ? new Date(d.createdAt).toLocaleDateString('en-US') : '—'}</td>
                    <td>
                      <div className="table-actions">
                        <button className="btn-icon" title="Edit" onClick={() => openEdit(d)}><Edit3 size={15} /></button>
                        <button className="btn-icon danger" title="Delete" onClick={() => handleDelete(d.id)}><Trash2 size={15} /></button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            <div className="data-table-footer">
              <span>Page {page} of {totalPages}</span>
              <div className="pagination-btns">
                <button className="pagination-btn" disabled={page <= 1} onClick={() => setPage(p => p - 1)}>Previous</button>
                <button className="pagination-btn" disabled={page >= totalPages} onClick={() => setPage(p => p + 1)}>Next</button>
              </div>
            </div>
          </>
        )}
      </div>

      {modalOpen && (
        <div className="dash-modal-overlay" onClick={() => setModalOpen(false)}>
          <div className="dash-modal" onClick={(e) => e.stopPropagation()}>
            <div className="dash-modal-header">
              <h2>{editingDept ? 'Edit Department' : 'New Department'}</h2>
              <button className="dash-modal-close" onClick={() => setModalOpen(false)}><X size={18} /></button>
            </div>
            <div className="dash-modal-body">
              <div className="dash-field">
                <label>Department Name</label>
                <input value={form.name} onChange={(e) => setForm(f => ({ ...f, name: e.target.value }))} placeholder="e.g. Software Engineering" />
              </div>
              <div className="dash-field">
                <label>Description</label>
                <textarea value={form.description} onChange={(e) => setForm(f => ({ ...f, description: e.target.value }))} placeholder="Department description..." />
              </div>
              <div className="dash-field">
                <label>Department Image (Optional)</label>
                <div className="file-input-wrapper">
                  <label className="file-label" htmlFor="dept-image">
                    <Upload size={14} /> Choose Image
                  </label>
                  <input id="dept-image" type="file" accept="image/*" onChange={(e) => setImageFile(e.target.files?.[0] || null)} />
                  {imageFile && <span className="file-name">{imageFile.name}</span>}
                </div>
              </div>
            </div>
            <div className="dash-modal-footer">
              <button className="btn-primary" onClick={handleSave} disabled={saving}>
                {saving ? 'Saving...' : (editingDept ? 'Update' : 'Create')}
              </button>
              <button className="btn-secondary" onClick={() => setModalOpen(false)}>Cancel</button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
