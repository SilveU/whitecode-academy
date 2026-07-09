import { useState, useEffect, useCallback } from 'react'
import { Plus, Search, Edit3, Trash2, Layers, X, Upload, Video, FileText } from 'lucide-react'
import { useAuth } from '../../context/AuthContext'
import { sectionApi, courseApi } from '../../services/api'

const DAYS = ['الأحد', 'الاثنين', 'الثلاثاء', 'الأربعاء', 'الخميس', 'الجمعة', 'السبت']

export default function SectionsPage() {
  const { isAdmin, isInstructor } = useAuth()
  const canManage = isAdmin || isInstructor

  const [sections, setSections] = useState([])
  const [courses, setCourses] = useState([])
  const [selectedCourse, setSelectedCourse] = useState('')
  const [loading, setLoading] = useState(false)
  const [alert, setAlert] = useState(null)

  const [modalOpen, setModalOpen] = useState(false)
  const [editingSection, setEditingSection] = useState(null)
  const [form, setForm] = useState({ name: '', description: '', startAt: '', endAt: '', dayOfWeek: '0', courseId: '' })
  const [videoFile, setVideoFile] = useState(null)
  const [pdfFile, setPdfFile] = useState(null)
  const [saving, setSaving] = useState(false)

  // Load courses for dropdown
  useEffect(() => {
    async function load() {
      const res = await courseApi.list({ pageSize: 100 })
      if (res.ok) {
        const data = res.data
        const list = Array.isArray(data) ? data : data?.items || data?.data || []
        setCourses(list)
      }
    }
    load()
  }, [])

  const loadSections = useCallback(async () => {
    if (!selectedCourse) { setSections([]); return }
    setLoading(true)
    try {
      const res = await sectionApi.listByCourse(selectedCourse)
      if (res.ok) {
        setSections(Array.isArray(res.data) ? res.data : res.data?.items || res.data?.data || [])
      }
    } catch {
      // ignore
    } finally {
      setLoading(false)
    }
  }, [selectedCourse])

  useEffect(() => { loadSections() }, [loadSections])

  const openCreate = () => {
    setEditingSection(null)
    setForm({ name: '', description: '', startAt: '', endAt: '', dayOfWeek: '0', courseId: selectedCourse })
    setVideoFile(null)
    setPdfFile(null)
    setModalOpen(true)
  }

  const openEdit = (sec) => {
    setEditingSection(sec)
    setForm({
      name: sec.name,
      description: sec.description,
      startAt: sec.startAt || '',
      endAt: sec.endAt || '',
      dayOfWeek: sec.dayOfWeek?.toString() || '0',
      courseId: sec.courseId || selectedCourse,
    })
    setVideoFile(null)
    setPdfFile(null)
    setModalOpen(true)
  }

  const handleSave = async () => {
    setSaving(true)
    try {
      const fd = new FormData()
      fd.append('Name', form.name)
      fd.append('Description', form.description)
      fd.append('StartAt', form.startAt)
      fd.append('EndAt', form.endAt)
      fd.append('DayOfWeek', form.dayOfWeek)
      fd.append('CourseId', form.courseId)
      if (videoFile) fd.append('VideoFile', videoFile)
      if (pdfFile) fd.append('PdfFile', pdfFile)

      let res
      if (editingSection) {
        res = await sectionApi.update(editingSection.id, fd)
      } else {
        res = await sectionApi.create(fd)
      }

      if (res.ok) {
        setModalOpen(false)
        setAlert({ type: 'success', msg: editingSection ? 'تم تحديث السكشن بنجاح' : 'تم إنشاء السكشن بنجاح' })
        loadSections()
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
    if (!window.confirm('هل أنت متأكد من حذف هذا السكشن؟')) return
    const res = await sectionApi.delete(id)
    if (res.ok) {
      setAlert({ type: 'success', msg: 'تم حذف السكشن بنجاح' })
      loadSections()
    } else {
      setAlert({ type: 'error', msg: res.message || 'حدث خطأ أثناء الحذف' })
    }
    setTimeout(() => setAlert(null), 4000)
  }

  return (
    <div>
      <div className="page-header">
        <div className="page-header-info">
          <h1>السكشنات</h1>
          <p>إدارة المحتوى التعليمي: فيديوهات وملفات PDF لكل كورس</p>
        </div>
        {canManage && selectedCourse && (
          <div className="page-actions">
            <button className="btn-primary" onClick={openCreate}>
              <Plus size={16} /> سكشن جديد
            </button>
          </div>
        )}
      </div>

      {alert && <div className={`dash-alert dash-alert-${alert.type}`}>{alert.msg}</div>}

      {/* Course selector */}
      <div style={{ marginBottom: '20px' }}>
        <div className="dash-field" style={{ maxWidth: '360px' }}>
          <label>اختر الكورس</label>
          <select value={selectedCourse} onChange={(e) => setSelectedCourse(e.target.value)}>
            <option value="">— اختر كورس لعرض السكشنات —</option>
            {courses.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
          </select>
        </div>
      </div>

      {!selectedCourse ? (
        <div className="data-table-wrapper">
          <div className="empty-state">
            <div className="empty-state-icon"><Layers size={28} /></div>
            <h3>اختر كورس أولاً</h3>
            <p>حدد الكورس من القائمة أعلاه لعرض السكشنات الخاصة به</p>
          </div>
        </div>
      ) : (
        <div className="data-table-wrapper">
          {loading ? (
            <div className="loading-container"><div className="spinner" /></div>
          ) : sections.length === 0 ? (
            <div className="empty-state">
              <div className="empty-state-icon"><Layers size={28} /></div>
              <h3>لا توجد سكشنات</h3>
              <p>ابدأ بإضافة محتوى تعليمي لهذا الكورس</p>
              {canManage && (
                <button className="btn-primary" onClick={openCreate}><Plus size={16} /> إضافة سكشن</button>
              )}
            </div>
          ) : (
            <table className="data-table">
              <thead>
                <tr>
                  <th>الاسم</th>
                  <th>الوصف</th>
                  <th>اليوم</th>
                  <th>الوقت</th>
                  <th>المحتوى</th>
                  {canManage && <th>إجراءات</th>}
                </tr>
              </thead>
              <tbody>
                {sections.map((s) => (
                  <tr key={s.id}>
                    <td style={{ fontWeight: 600, color: 'var(--text-primary)' }}>{s.name}</td>
                    <td style={{ maxWidth: '180px', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{s.description}</td>
                    <td><span className="badge badge-blue">{DAYS[s.dayOfWeek] || '—'}</span></td>
                    <td style={{ fontSize: '0.82rem', direction: 'ltr' }}>{s.startAt} - {s.endAt}</td>
                    <td>
                      <div style={{ display: 'flex', gap: '6px' }}>
                        {s.videoUrl && <span className="badge badge-green"><Video size={11} /> فيديو</span>}
                        {s.pdfUrl && <span className="badge badge-amber"><FileText size={11} /> PDF</span>}
                      </div>
                    </td>
                    {canManage && (
                      <td>
                        <div className="table-actions">
                          <button className="btn-icon" title="تعديل" onClick={() => openEdit(s)}><Edit3 size={15} /></button>
                          <button className="btn-icon danger" title="حذف" onClick={() => handleDelete(s.id)}><Trash2 size={15} /></button>
                        </div>
                      </td>
                    )}
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      )}

      {/* Create/Edit Modal */}
      {modalOpen && (
        <div className="dash-modal-overlay" onClick={() => setModalOpen(false)}>
          <div className="dash-modal" onClick={(e) => e.stopPropagation()}>
            <div className="dash-modal-header">
              <h2>{editingSection ? 'تعديل سكشن' : 'سكشن جديد'}</h2>
              <button className="dash-modal-close" onClick={() => setModalOpen(false)}><X size={18} /></button>
            </div>
            <div className="dash-modal-body">
              <div className="dash-field">
                <label>الاسم</label>
                <input value={form.name} onChange={(e) => setForm(f => ({ ...f, name: e.target.value }))} placeholder="اسم السكشن" />
              </div>
              <div className="dash-field">
                <label>الوصف</label>
                <textarea value={form.description} onChange={(e) => setForm(f => ({ ...f, description: e.target.value }))} placeholder="وصف المحتوى..." />
              </div>
              <div className="dash-field">
                <label>الكورس</label>
                <select value={form.courseId} onChange={(e) => setForm(f => ({ ...f, courseId: e.target.value }))}>
                  <option value="">اختر الكورس</option>
                  {courses.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
                </select>
              </div>
              <div className="dash-field-row">
                <div className="dash-field">
                  <label>وقت البداية</label>
                  <input type="time" value={form.startAt} onChange={(e) => setForm(f => ({ ...f, startAt: e.target.value }))} />
                </div>
                <div className="dash-field">
                  <label>وقت النهاية</label>
                  <input type="time" value={form.endAt} onChange={(e) => setForm(f => ({ ...f, endAt: e.target.value }))} />
                </div>
              </div>
              <div className="dash-field">
                <label>اليوم</label>
                <select value={form.dayOfWeek} onChange={(e) => setForm(f => ({ ...f, dayOfWeek: e.target.value }))}>
                  {DAYS.map((d, i) => <option key={i} value={i}>{d}</option>)}
                </select>
              </div>
              <div className="dash-field">
                <label>ملف الفيديو {!editingSection && '(مطلوب)'}</label>
                <div className="file-input-wrapper">
                  <label className="file-label" htmlFor="sec-video"><Video size={14} /> اختر فيديو</label>
                  <input id="sec-video" type="file" accept="video/*" onChange={(e) => setVideoFile(e.target.files?.[0] || null)} />
                  {videoFile && <span className="file-name">{videoFile.name}</span>}
                </div>
              </div>
              <div className="dash-field">
                <label>ملف PDF (اختياري)</label>
                <div className="file-input-wrapper">
                  <label className="file-label" htmlFor="sec-pdf"><FileText size={14} /> اختر ملف PDF</label>
                  <input id="sec-pdf" type="file" accept=".pdf" onChange={(e) => setPdfFile(e.target.files?.[0] || null)} />
                  {pdfFile && <span className="file-name">{pdfFile.name}</span>}
                </div>
              </div>
            </div>
            <div className="dash-modal-footer">
              <button className="btn-primary" onClick={handleSave} disabled={saving}>
                {saving ? 'جاري الحفظ...' : (editingSection ? 'تحديث' : 'إنشاء')}
              </button>
              <button className="btn-secondary" onClick={() => setModalOpen(false)}>إلغاء</button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
