import { useState, useEffect, useCallback } from 'react'
import { Plus, Search, Edit3, Trash2, GraduationCap, X } from 'lucide-react'
import { instructorApi, departmentApi } from '../../services/api'

export default function InstructorsPage() {
  const [instructors, setInstructors] = useState([])
  const [loading, setLoading] = useState(true)
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)
  const [totalPages, setTotalPages] = useState(1)
  const [alert, setAlert] = useState(null)

  const [modalOpen, setModalOpen] = useState(false)
  const [editingInst, setEditingInst] = useState(null)
  const [form, setForm] = useState({ userId: '', departmentId: '' })
  const [departments, setDepartments] = useState([])
  const [saving, setSaving] = useState(false)

  const loadInstructors = useCallback(async () => {
    setLoading(true)
    try {
      const res = await instructorApi.list({ pageNumber: page, pageSize: 10, wordForSearch: search || undefined })
      if (res.ok) {
        const data = res.data
        if (Array.isArray(data)) {
          setInstructors(data)
          setTotalPages(1)
        } else {
          setInstructors(data?.items || data?.data || [])
          setTotalPages(data?.totalPages || 1)
        }
      }
    } catch {
      // ignore
    } finally {
      setLoading(false)
    }
  }, [page, search])

  useEffect(() => { loadInstructors() }, [loadInstructors])

  const loadDepartments = async () => {
    try {
      const res = await departmentApi.list({ pageSize: 100 })
      if (res.ok) {
        const d = res.data
        setDepartments(Array.isArray(d) ? d : d?.items || d?.data || [])
      }
    } catch {
      // ignore
    }
  }

  const openCreate = async () => {
    setEditingInst(null)
    setForm({ userId: '', departmentId: '' })
    await loadDepartments()
    setModalOpen(true)
  }

  const openEdit = async (inst) => {
    setEditingInst(inst)
    setForm({ userId: inst.userId, departmentId: inst.departmentId || '' })
    await loadDepartments()
    setModalOpen(true)
  }

  const handleSave = async () => {
    setSaving(true)
    try {
      let res
      if (editingInst) {
        res = await instructorApi.update(editingInst.id, { departmentId: form.departmentId || null })
      } else {
        res = await instructorApi.assign({ userId: form.userId, departmentId: form.departmentId || null })
      }

      if (res.ok) {
        setModalOpen(false)
        setAlert({ type: 'success', msg: editingInst ? 'Instructor updated successfully' : 'Instructor assigned successfully' })
        loadInstructors()
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
    if (!window.confirm('Are you sure you want to remove this instructor?')) return
    const res = await instructorApi.delete(id)
    if (res.ok) {
      setAlert({ type: 'success', msg: 'Instructor removed successfully' })
      loadInstructors()
    } else {
      setAlert({ type: 'error', msg: res.message || 'Error occurred during removal' })
    }
    setTimeout(() => setAlert(null), 4000)
  }

  return (
    <div>
      <div className="page-header">
        <div className="page-header-info">
          <h1>Instructors</h1>
          <p>Manage academy instructors and assign new faculty</p>
        </div>
        <div className="page-actions">
          <button className="btn-primary" onClick={openCreate}>
            <Plus size={16} /> Assign Instructor
          </button>
        </div>
      </div>

      {alert && <div className={`dash-alert dash-alert-${alert.type}`}>{alert.msg}</div>}

      <div className="data-table-wrapper">
        <div className="data-table-header">
          <div className="search-input-wrapper">
            <Search size={16} />
            <input placeholder="Search instructors..." value={search} onChange={(e) => { setSearch(e.target.value); setPage(1) }} />
          </div>
        </div>

        {loading ? (
          <div className="loading-container"><div className="spinner" /></div>
        ) : instructors.length === 0 ? (
          <div className="empty-state">
            <div className="empty-state-icon"><GraduationCap size={28} /></div>
            <h3>No instructors found</h3>
            <p>Assign the first instructor to the platform</p>
            <button className="btn-primary" onClick={openCreate}><Plus size={16} /> Assign Instructor</button>
          </div>
        ) : (
          <>
            <table className="data-table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Email</th>
                  <th>Department</th>
                  <th>Assigned Date</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {instructors.map((i) => (
                  <tr key={i.id}>
                    <td style={{ fontWeight: 600, color: 'var(--text-primary)' }}>{i.firstName} {i.lastName}</td>
                    <td style={{ direction: 'ltr', textAlign: 'left' }}>{i.email}</td>
                    <td>
                      {i.departmentName ? (
                        <span className="badge badge-blue">{i.departmentName}</span>
                      ) : (
                        <span style={{ color: 'var(--text-muted)', fontSize: '0.8rem' }}>No Department</span>
                      )}
                    </td>
                    <td>{i.createdAt ? new Date(i.createdAt).toLocaleDateString('en-US') : '—'}</td>
                    <td>
                      <div className="table-actions">
                        <button className="btn-icon" title="Edit" onClick={() => openEdit(i)}><Edit3 size={15} /></button>
                        <button className="btn-icon danger" title="Delete" onClick={() => handleDelete(i.id)}><Trash2 size={15} /></button>
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
              <h2>{editingInst ? 'Edit Instructor' : 'Assign New Instructor'}</h2>
              <button className="dash-modal-close" onClick={() => setModalOpen(false)}><X size={18} /></button>
            </div>
            <div className="dash-modal-body">
              {!editingInst && (
                <div className="dash-field">
                  <label>User ID</label>
                  <input value={form.userId} onChange={(e) => setForm(f => ({ ...f, userId: e.target.value }))} placeholder="Enter registered user ID" style={{ direction: 'ltr', textAlign: 'left' }} />
                </div>
              )}
              <div className="dash-field">
                <label>Department (Optional)</label>
                <select value={form.departmentId} onChange={(e) => setForm(f => ({ ...f, departmentId: e.target.value }))}>
                  <option value="">No Department</option>
                  {departments.map(d => <option key={d.id} value={d.id}>{d.name}</option>)}
                </select>
              </div>
            </div>
            <div className="dash-modal-footer">
              <button className="btn-primary" onClick={handleSave} disabled={saving}>
                {saving ? 'Saving...' : (editingInst ? 'Update' : 'Assign')}
              </button>
              <button className="btn-secondary" onClick={() => setModalOpen(false)}>Cancel</button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
