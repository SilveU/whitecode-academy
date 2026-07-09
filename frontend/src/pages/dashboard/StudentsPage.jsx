import { useState } from 'react'
import { Users, UserPlus, Trash2 } from 'lucide-react'
import { useAuth } from '../../context/AuthContext'
import { studentApi } from '../../services/api'

export default function StudentsPage() {
  const { isAdmin, isStudent } = useAuth()
  const [alert, setAlert] = useState(null)
  const [loading, setLoading] = useState(false)
  const [deleteId, setDeleteId] = useState('')

  const handleSelfRegister = async () => {
    setLoading(true)
    try {
      const res = await studentApi.register()
      if (res.ok) {
        setAlert({ type: 'success', msg: 'تم تسجيلك كطالب بنجاح! يمكنك الآن الالتحاق بالكورسات.' })
      } else {
        setAlert({ type: 'error', msg: res.message || 'حدث خطأ أثناء التسجيل' })
      }
    } catch {
      setAlert({ type: 'error', msg: 'حدث خطأ في الاتصال' })
    } finally {
      setLoading(false)
    }
    setTimeout(() => setAlert(null), 5000)
  }

  const handleDeleteStudent = async () => {
    if (!deleteId.trim()) return
    if (!window.confirm('هل أنت متأكد من حذف هذا الطالب؟')) return
    const res = await studentApi.delete(deleteId.trim())
    if (res.ok) {
      setAlert({ type: 'success', msg: 'تم حذف الطالب بنجاح' })
      setDeleteId('')
    } else {
      setAlert({ type: 'error', msg: res.message || 'حدث خطأ أثناء الحذف' })
    }
    setTimeout(() => setAlert(null), 4000)
  }

  return (
    <div>
      <div className="page-header">
        <div className="page-header-info">
          <h1>الطلاب</h1>
          <p>إدارة الطلاب المسجلين في الأكاديمية</p>
        </div>
      </div>

      {alert && <div className={`dash-alert dash-alert-${alert.type}`}>{alert.msg}</div>}

      <div style={{ display: 'grid', gap: '20px', gridTemplateColumns: 'repeat(auto-fill, minmax(340px, 1fr))' }}>
        {/* Student self-register */}
        {isStudent && (
          <div className="data-table-wrapper" style={{ padding: '28px' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '12px', marginBottom: '14px' }}>
              <div style={{ width: 44, height: 44, borderRadius: 12, background: 'rgba(34, 197, 94, 0.1)', display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'var(--green-400)' }}>
                <UserPlus size={20} />
              </div>
              <div>
                <h3 style={{ fontSize: '1rem', fontWeight: 700, color: 'var(--text-primary)' }}>التسجيل كطالب</h3>
                <p style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>سجّل نفسك كطالب للالتحاق بالكورسات</p>
              </div>
            </div>
            <button className="btn-primary" onClick={handleSelfRegister} disabled={loading} style={{ width: '100%' }}>
              <UserPlus size={16} /> {loading ? 'جاري التسجيل...' : 'سجّل كطالب الآن'}
            </button>
          </div>
        )}

        {/* Admin delete student */}
        {isAdmin && (
          <div className="data-table-wrapper" style={{ padding: '28px' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: '12px', marginBottom: '14px' }}>
              <div style={{ width: 44, height: 44, borderRadius: 12, background: 'rgba(239, 68, 68, 0.1)', display: 'flex', alignItems: 'center', justifyContent: 'center', color: '#ef4444' }}>
                <Trash2 size={20} />
              </div>
              <div>
                <h3 style={{ fontSize: '1rem', fontWeight: 700, color: 'var(--text-primary)' }}>حذف طالب</h3>
                <p style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>أدخل معرف الطالب لحذفه من المنصة</p>
              </div>
            </div>
            <div className="dash-field" style={{ marginBottom: '12px' }}>
              <input
                placeholder="Student ID"
                value={deleteId}
                onChange={(e) => setDeleteId(e.target.value)}
                style={{ direction: 'ltr', textAlign: 'right' }}
              />
            </div>
            <button className="btn-danger" onClick={handleDeleteStudent} style={{ width: '100%', justifyContent: 'center', padding: '10px' }}>
              <Trash2 size={16} /> حذف الطالب
            </button>
          </div>
        )}
      </div>

      {/* Info card */}
      <div className="data-table-wrapper" style={{ padding: '24px', marginTop: '20px' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
          <Users size={18} style={{ color: 'var(--green-400)' }} />
          <h3 style={{ fontSize: '1rem', fontWeight: 700, color: 'var(--text-primary)' }}>معلومات</h3>
        </div>
        <p style={{ fontSize: '0.85rem', color: 'var(--text-muted)', marginTop: '10px', lineHeight: 1.7 }}>
          يمكن للمستخدمين المسجلين تسجيل أنفسهم كطلاب، ثم الالتحاق بالكورسات المتاحة.
          المسؤول يمكنه حذف أي طالب من المنصة.
        </p>
      </div>
    </div>
  )
}
