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
        setAlert({ type: 'success', msg: editingDept ? 'تم تحديث القسم بنجاح' : 'تم إنشاء القسم بنجاح' })
        loadDepartments()
      } else {
        setAlert({ type: 'error', msg: res.message || 'حدث خطأ' })
      }
    } catch {
      setAlert({ type: 'error', msg: 'حدث خطأ في الاتصال' })
    } finally {
      setSaving(false)
    }
    setTimeout(() => setAlert(null), 4000)
  }

  const handleDelete = async (id) => {
    if (!window.confirm('هل أنت متأكد من حذف هذا القسم؟')) return
    const res = await departmentApi.delete(id)
    if (res.ok) {
      setAlert({ type: 'success', msg: 'تم حذف القسم بنجاح' })
      loadDepartments()
    } else {
      setAlert({ type: 'error', msg: res.message || 'حدث خطأ أثناء الحذف' })
    }
    setTimeout(() => setAlert(null), 4000)
  }

  return (
    <div>
      <div className="page-header">
        <div className="page-header-info">
          <h1>الأقسام</h1>
          <p>إدارة أقسام الأكاديمية وتنظيم الكورسات</p>
        </div>
        <div className="page-actions">
          <button className="btn-primary" onClick={openCreate}>
            <Plus size={16} /> قسم جديد
          </button>
        </div>
      </div>

      {alert && <div className={`dash-alert dash-alert-${alert.type}`}>{alert.msg}</div>}

      <div className="data-table-wrapper">
        <div className="data-table-header">
          <div className="search-input-wrapper">
            <Search size={16} />
            <input placeholder="ابحث عن قسم..." value={search} onChange={(e) => { setSearch(e.target.value); setPage(1) }} />
          </div>
        </div>

        {loading ? (
          <div className="loading-container"><div className="spinner" /></div>
        ) : departments.length === 0 ? (
          <div className="empty-state">
            <div className="empty-state-icon"><FolderTree size={28} /></div>
            <h3>لا توجد أقسام</h3>
            <p>ابدأ بإضافة أول قسم</p>
            <button className="btn-primary" onClick={openCreate}><Plus size={16} /> إضافة قسم</button>
          </div>
        ) : (
          <>
            <table className="data-table">
              <thead>
                <tr>
                  <th>اسم القسم</th>
                  <th>الوصف</th>
                  <th>الصورة</th>
                  <th>تاريخ الإنشاء</th>
                  <th>إجراءات</th>
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
                        <span style={{ color: 'var(--text-muted)', fontSize: '0.8rem' }}>بدون صورة</span>
                      )}
                    </td>
                    <td>{d.createdAt ? new Date(d.createdAt).toLocaleDateString('ar-EG') : '—'}</td>
                    <td>
                      <div className="table-actions">
                        <button className="btn-icon" title="تعديل" onClick={() => openEdit(d)}><Edit3 size={15} /></button>
                        <button className="btn-icon danger" title="حذف" onClick={() => handleDelete(d.id)}><Trash2 size={15} /></button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            <div className="data-table-footer">
              <span>صفحة {page} من {totalPages}</span>
              <div className="pagination-btns">
                <button className="pagination-btn" disabled={page <= 1} onClick={() => setPage(p => p - 1)}>السابق</button>
                <button className="pagination-btn" disabled={page >= totalPages} onClick={() => setPage(p => p + 1)}>التالي</button>
              </div>
            </div>
          </>
        )}
      </div>

      {modalOpen && (
        <div className="dash-modal-overlay" onClick={() => setModalOpen(false)}>
          <div className="dash-modal" onClick={(e) => e.stopPropagation()}>
            <div className="dash-modal-header">
              <h2>{editingDept ? 'تعديل قسم' : 'قسم جديد'}</h2>
              <button className="dash-modal-close" onClick={() => setModalOpen(false)}><X size={18} /></button>
            </div>
            <div className="dash-modal-body">
              <div className="dash-field">
                <label>اسم القسم</label>
                <input value={form.name} onChange={(e) => setForm(f => ({ ...f, name: e.target.value }))} placeholder="مثال: قسم البرمجة" />
              </div>
              <div className="dash-field">
                <label>الوصف</label>
                <textarea value={form.description} onChange={(e) => setForm(f => ({ ...f, description: e.target.value }))} placeholder="وصف القسم..." />
              </div>
              <div className="dash-field">
                <label>صورة القسم (اختياري)</label>
                <div className="file-input-wrapper">
                  <label className="file-label" htmlFor="dept-image">
                    <Upload size={14} /> اختر صورة
                  </label>
                  <input id="dept-image" type="file" accept="image/*" onChange={(e) => setImageFile(e.target.files?.[0] || null)} />
                  {imageFile && <span className="file-name">{imageFile.name}</span>}
                </div>
              </div>
            </div>
            <div className="dash-modal-footer">
              <button className="btn-primary" onClick={handleSave} disabled={saving}>
                {saving ? 'جاري الحفظ...' : (editingDept ? 'تحديث' : 'إنشاء')}
              </button>
              <button className="btn-secondary" onClick={() => setModalOpen(false)}>إلغاء</button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
