import { useState, useEffect, useCallback } from 'react'
import { Plus, Search, Edit3, Trash2, BookOpen, X, Clock, Layers } from 'lucide-react'
import { useAuth } from '../../context/AuthContext'
import { courseApi, departmentApi, instructorApi } from '../../services/api'

export default function CoursesPage() {
  const { isAdmin, isInstructor } = useAuth()
  const canManage = isAdmin || isInstructor

  const [courses, setCourses] = useState([])
  const [loading, setLoading] = useState(true)
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)
  const [totalPages, setTotalPages] = useState(1)
  const [alert, setAlert] = useState(null)

  // Modal state
  const [modalOpen, setModalOpen] = useState(false)
  const [editingCourse, setEditingCourse] = useState(null)
  const [form, setForm] = useState({ name: '', description: '', totalHours: '', totalSections: '', departmentId: '', instructorId: '' })
  const [departments, setDepartments] = useState([])
  const [instructors, setInstructors] = useState([])
  const [saving, setSaving] = useState(false)

  const loadCourses = useCallback(async () => {
    setLoading(true)
    try {
      const res = await courseApi.list({ pageNumber: page, pageSize: 10, wordForSearch: search || undefined })
      if (res.ok) {
        const data = res.data
        if (Array.isArray(data)) {
          setCourses(data)
          setTotalPages(1)
        } else {
          setCourses(data?.items || data?.data || [])
          setTotalPages(data?.totalPages || 1)
        }
      }
    } catch {
      // ignore
    } finally {
      setLoading(false)
    }
  }, [page, search])

  useEffect(() => { loadCourses() }, [loadCourses])

  const openCreate = async () => {
    setEditingCourse(null)
    setForm({ name: '', description: '', totalHours: '', totalSections: '', departmentId: '', instructorId: '' })
    await loadDropdowns()
    setModalOpen(true)
  }

  const openEdit = async (course) => {
    setEditingCourse(course)
    setForm({
      name: course.name,
      description: course.description,
      totalHours: course.totalHours?.toString() || '',
      totalSections: course.totalSections?.toString() || '',
      departmentId: course.departmentId || '',
      instructorId: course.instructorId || '',
    })
    await loadDropdowns()
    setModalOpen(true)
  }

  const loadDropdowns = async () => {
    try {
      const [dRes, iRes] = await Promise.allSettled([
        departmentApi.list({ pageSize: 100 }),
        isAdmin ? instructorApi.list({ pageSize: 100 }) : Promise.resolve({ ok: true, data: [] }),
      ])
      if (dRes.status === 'fulfilled' && dRes.value?.ok) {
        const d = dRes.value.data
        setDepartments(Array.isArray(d) ? d : d?.items || d?.data || [])
      }
      if (iRes.status === 'fulfilled' && iRes.value?.ok) {
        const i = iRes.value.data
        setInstructors(Array.isArray(i) ? i : i?.items || i?.data || [])
      }
    } catch {
      // ignore
    }
  }

  const handleSave = async () => {
    setSaving(true)
    try {
      const body = {
        name: form.name,
        description: form.description,
        totalHours: parseFloat(form.totalHours) || 0,
        totalSections: parseInt(form.totalSections) || 0,
        departmentId: form.departmentId || undefined,
        instructorId: form.instructorId || undefined,
      }

      let res
      if (editingCourse) {
        res = await courseApi.update(editingCourse.id, body)
      } else {
        res = await courseApi.create(body)
      }

      if (res.ok) {
        setModalOpen(false)
        setAlert({ type: 'success', msg: editingCourse ? 'تم تحديث الكورس بنجاح' : 'تم إنشاء الكورس بنجاح' })
        loadCourses()
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
    if (!window.confirm('هل أنت متأكد من حذف هذا الكورس؟')) return
    const res = await courseApi.delete(id)
    if (res.ok) {
      setAlert({ type: 'success', msg: 'تم حذف الكورس بنجاح' })
      loadCourses()
    } else {
      setAlert({ type: 'error', msg: res.message || 'حدث خطأ أثناء الحذف' })
    }
    setTimeout(() => setAlert(null), 4000)
  }

  return (
    <div>
      <div className="page-header">
        <div className="page-header-info">
          <h1>الكورسات</h1>
          <p>إدارة وتصفح جميع الكورسات المتاحة على المنصة</p>
        </div>
        {canManage && (
          <div className="page-actions">
            <button className="btn-primary" onClick={openCreate}>
              <Plus size={16} /> كورس جديد
            </button>
          </div>
        )}
      </div>

      {alert && (
        <div className={`dash-alert dash-alert-${alert.type}`}>{alert.msg}</div>
      )}

      <div className="data-table-wrapper">
        <div className="data-table-header">
          <div className="search-input-wrapper">
            <Search size={16} />
            <input
              placeholder="ابحث عن كورس..."
              value={search}
              onChange={(e) => { setSearch(e.target.value); setPage(1) }}
            />
          </div>
        </div>

        {loading ? (
          <div className="loading-container"><div className="spinner" /></div>
        ) : courses.length === 0 ? (
          <div className="empty-state">
            <div className="empty-state-icon"><BookOpen size={28} /></div>
            <h3>لا توجد كورسات</h3>
            <p>ابدأ بإضافة أول كورس على المنصة</p>
            {canManage && (
              <button className="btn-primary" onClick={openCreate}>
                <Plus size={16} /> إضافة كورس
              </button>
            )}
          </div>
        ) : (
          <>
            <table className="data-table">
              <thead>
                <tr>
                  <th>اسم الكورس</th>
                  <th>الوصف</th>
                  <th>الساعات</th>
                  <th>السكشنات</th>
                  <th>تاريخ الإنشاء</th>
                  {canManage && <th>إجراءات</th>}
                </tr>
              </thead>
              <tbody>
                {courses.map((c) => (
                  <tr key={c.id}>
                    <td style={{ fontWeight: 600, color: 'var(--text-primary)' }}>{c.name}</td>
                    <td style={{ maxWidth: '200px', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{c.description}</td>
                    <td>
                      <span className="badge badge-blue"><Clock size={11} /> {c.totalHours} ساعة</span>
                    </td>
                    <td>
                      <span className="badge badge-purple"><Layers size={11} /> {c.totalSections}</span>
                    </td>
                    <td>{c.createdAt ? new Date(c.createdAt).toLocaleDateString('ar-EG') : '—'}</td>
                    {canManage && (
                      <td>
                        <div className="table-actions">
                          <button className="btn-icon" title="تعديل" onClick={() => openEdit(c)}><Edit3 size={15} /></button>
                          <button className="btn-icon danger" title="حذف" onClick={() => handleDelete(c.id)}><Trash2 size={15} /></button>
                        </div>
                      </td>
                    )}
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

      {/* Create/Edit Modal */}
      {modalOpen && (
        <div className="dash-modal-overlay" onClick={() => setModalOpen(false)}>
          <div className="dash-modal" onClick={(e) => e.stopPropagation()}>
            <div className="dash-modal-header">
              <h2>{editingCourse ? 'تعديل كورس' : 'كورس جديد'}</h2>
              <button className="dash-modal-close" onClick={() => setModalOpen(false)}><X size={18} /></button>
            </div>
            <div className="dash-modal-body">
              <div className="dash-field">
                <label>اسم الكورس</label>
                <input value={form.name} onChange={(e) => setForm(f => ({ ...f, name: e.target.value }))} placeholder="مثال: تطوير واجهات المستخدم" />
              </div>
              <div className="dash-field">
                <label>الوصف</label>
                <textarea value={form.description} onChange={(e) => setForm(f => ({ ...f, description: e.target.value }))} placeholder="وصف مختصر للكورس..." />
              </div>
              <div className="dash-field-row">
                <div className="dash-field">
                  <label>إجمالي الساعات</label>
                  <input type="number" value={form.totalHours} onChange={(e) => setForm(f => ({ ...f, totalHours: e.target.value }))} placeholder="0" />
                </div>
                <div className="dash-field">
                  <label>عدد السكشنات</label>
                  <input type="number" value={form.totalSections} onChange={(e) => setForm(f => ({ ...f, totalSections: e.target.value }))} placeholder="0" />
                </div>
              </div>
              <div className="dash-field">
                <label>القسم</label>
                <select value={form.departmentId} onChange={(e) => setForm(f => ({ ...f, departmentId: e.target.value }))}>
                  <option value="">اختر القسم</option>
                  {departments.map(d => <option key={d.id} value={d.id}>{d.name}</option>)}
                </select>
              </div>
              {isAdmin && (
                <div className="dash-field">
                  <label>المدرب</label>
                  <select value={form.instructorId} onChange={(e) => setForm(f => ({ ...f, instructorId: e.target.value }))}>
                    <option value="">اختر المدرب</option>
                    {instructors.map(i => <option key={i.id} value={i.id}>{i.firstName} {i.lastName}</option>)}
                  </select>
                </div>
              )}
            </div>
            <div className="dash-modal-footer">
              <button className="btn-primary" onClick={handleSave} disabled={saving}>
                {saving ? 'جاري الحفظ...' : (editingCourse ? 'تحديث' : 'إنشاء')}
              </button>
              <button className="btn-secondary" onClick={() => setModalOpen(false)}>إلغاء</button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
