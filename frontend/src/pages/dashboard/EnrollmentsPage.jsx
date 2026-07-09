import { useState, useEffect } from 'react'
import { ClipboardList, Search, Trash2, Plus } from 'lucide-react'
import { useAuth } from '../../context/AuthContext'
import { enrollmentApi, courseApi } from '../../services/api'

export default function EnrollmentsPage() {
  const { isAdmin, isInstructor, isStudent } = useAuth()
  const [enrollments, setEnrollments] = useState([])
  const [courses, setCourses] = useState([])
  const [selectedCourse, setSelectedCourse] = useState('')
  const [studentIdInput, setStudentIdInput] = useState('')
  const [enrollCourseId, setEnrollCourseId] = useState('')
  const [loading, setLoading] = useState(false)
  const [alert, setAlert] = useState(null)

  // Load courses
  useEffect(() => {
    async function load() {
      const res = await courseApi.list({ pageSize: 100 })
      if (res.ok) {
        const d = res.data
        setCourses(Array.isArray(d) ? d : d?.items || d?.data || [])
      }
    }
    load()
  }, [])

  // Load enrollments
  const loadByCourse = async (courseId) => {
    if (!courseId) { setEnrollments([]); return }
    setLoading(true)
    try {
      const res = await enrollmentApi.byCourse(courseId)
      if (res.ok) {
        setEnrollments(Array.isArray(res.data) ? res.data : res.data?.items || res.data?.data || [])
      }
    } catch {
      // ignore
    } finally {
      setLoading(false)
    }
  }

  const loadByStudent = async () => {
    if (!studentIdInput.trim()) return
    setLoading(true)
    try {
      const res = await enrollmentApi.byStudent(studentIdInput.trim())
      if (res.ok) {
        setEnrollments(Array.isArray(res.data) ? res.data : res.data?.items || res.data?.data || [])
      } else {
        setAlert({ type: 'error', msg: res.message || 'لم يتم العثور على تسجيلات' })
      }
    } catch {
      setAlert({ type: 'error', msg: 'حدث خطأ في الاتصال' })
    } finally {
      setLoading(false)
    }
    setTimeout(() => setAlert(null), 4000)
  }

  const handleEnroll = async () => {
    if (!enrollCourseId) return
    setLoading(true)
    try {
      const res = await enrollmentApi.enroll(enrollCourseId)
      if (res.ok) {
        setAlert({ type: 'success', msg: 'تم الالتحاق بالكورس بنجاح!' })
      } else {
        setAlert({ type: 'error', msg: res.message || 'حدث خطأ أثناء الالتحاق' })
      }
    } catch {
      setAlert({ type: 'error', msg: 'حدث خطأ في الاتصال' })
    } finally {
      setLoading(false)
    }
    setTimeout(() => setAlert(null), 4000)
  }

  const handleUnenroll = async (studentId, courseId) => {
    if (!window.confirm('هل أنت متأكد من إلغاء التسجيل؟')) return
    const res = await enrollmentApi.unenroll(studentId, courseId)
    if (res.ok) {
      setAlert({ type: 'success', msg: 'تم إلغاء التسجيل بنجاح' })
      setEnrollments(prev => prev.filter(e => !(e.studentId === studentId && e.courseId === courseId)))
    } else {
      setAlert({ type: 'error', msg: res.message || 'حدث خطأ' })
    }
    setTimeout(() => setAlert(null), 4000)
  }

  return (
    <div>
      <div className="page-header">
        <div className="page-header-info">
          <h1>التسجيلات</h1>
          <p>عرض وإدارة تسجيلات الطلاب في الكورسات</p>
        </div>
      </div>

      {alert && <div className={`dash-alert dash-alert-${alert.type}`}>{alert.msg}</div>}

      <div style={{ display: 'grid', gap: '20px', gridTemplateColumns: 'repeat(auto-fill, minmax(340px, 1fr))', marginBottom: '24px' }}>
        {/* Enroll (User role) */}
        {isStudent && (
          <div className="data-table-wrapper" style={{ padding: '24px' }}>
            <h3 style={{ fontSize: '0.95rem', fontWeight: 700, color: 'var(--text-primary)', marginBottom: '14px', display: 'flex', alignItems: 'center', gap: '8px' }}>
              <Plus size={16} style={{ color: 'var(--green-400)' }} /> التحق بكورس
            </h3>
            <div className="dash-field" style={{ marginBottom: '12px' }}>
              <select value={enrollCourseId} onChange={(e) => setEnrollCourseId(e.target.value)}>
                <option value="">اختر الكورس</option>
                {courses.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
              </select>
            </div>
            <button className="btn-primary" onClick={handleEnroll} disabled={!enrollCourseId || loading} style={{ width: '100%' }}>
              التحق الآن
            </button>
          </div>
        )}

        {/* Search by student ID */}
        <div className="data-table-wrapper" style={{ padding: '24px' }}>
          <h3 style={{ fontSize: '0.95rem', fontWeight: 700, color: 'var(--text-primary)', marginBottom: '14px', display: 'flex', alignItems: 'center', gap: '8px' }}>
            <Search size={16} style={{ color: '#60a5fa' }} /> بحث بمعرف الطالب
          </h3>
          <div className="dash-field" style={{ marginBottom: '12px' }}>
            <input placeholder="Student ID" value={studentIdInput} onChange={(e) => setStudentIdInput(e.target.value)} style={{ direction: 'ltr', textAlign: 'right' }} />
          </div>
          <button className="btn-secondary" onClick={loadByStudent} disabled={loading} style={{ width: '100%' }}>
            عرض التسجيلات
          </button>
        </div>
      </div>

      {/* Browse by course */}
      {(isAdmin || isInstructor) && (
        <div style={{ marginBottom: '20px' }}>
          <div className="dash-field" style={{ maxWidth: '360px' }}>
            <label>عرض التسجيلات حسب الكورس</label>
            <select value={selectedCourse} onChange={(e) => { setSelectedCourse(e.target.value); loadByCourse(e.target.value) }}>
              <option value="">— اختر كورس —</option>
              {courses.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
            </select>
          </div>
        </div>
      )}

      {/* Enrollments table */}
      <div className="data-table-wrapper">
        {loading ? (
          <div className="loading-container"><div className="spinner" /></div>
        ) : enrollments.length === 0 ? (
          <div className="empty-state">
            <div className="empty-state-icon"><ClipboardList size={28} /></div>
            <h3>لا توجد تسجيلات</h3>
            <p>اختر كورس أو ابحث بمعرف طالب لعرض التسجيلات</p>
          </div>
        ) : (
          <table className="data-table">
            <thead>
              <tr>
                <th>اسم الكورس</th>
                <th>معرف الطالب</th>
                <th>تاريخ التسجيل</th>
                {isAdmin && <th>إجراءات</th>}
              </tr>
            </thead>
            <tbody>
              {enrollments.map((e) => (
                <tr key={e.id}>
                  <td style={{ fontWeight: 600, color: 'var(--text-primary)' }}>{e.courseName || '—'}</td>
                  <td style={{ direction: 'ltr', textAlign: 'right', fontSize: '0.82rem' }}>{e.studentId}</td>
                  <td>{e.createdAt ? new Date(e.createdAt).toLocaleDateString('ar-EG') : '—'}</td>
                  {isAdmin && (
                    <td>
                      <button className="btn-icon danger" title="إلغاء التسجيل" onClick={() => handleUnenroll(e.studentId, e.courseId)}>
                        <Trash2 size={15} />
                      </button>
                    </td>
                  )}
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  )
}
