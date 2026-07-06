import { useState, useEffect, useRef } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { CheckCircle2, XCircle, Loader2 } from 'lucide-react'
import BackgroundAnimation from '../components/BackgroundAnimation'
import BrandingSide from '../components/BrandingSide'

export default function VerifyEmailPage() {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const [status, setStatus] = useState('loading')
  const [message, setMessage] = useState('جاري التحقق من الحساب...')
  const hasFetched = useRef(false)

  useEffect(() => {
    if (hasFetched.current) return
    hasFetched.current = true

    const userId = searchParams.get('userId')
    const token = searchParams.get('token')

    if (!userId || !token) {
      setStatus('error')
      setMessage('رابط التحقق غير صالح أو مفقود.')
      return
    }

    fetch(`/api/authentication/confirm-email?userId=${encodeURIComponent(userId)}&token=${encodeURIComponent(token)}`, {
      headers: {
        'Accept': 'application/json',
        'ngrok-skip-browser-warning': 'true'
      }
    })
      .then(res => res.json())
      .then(data => {
        if (data.isAuthenticated) {
          setStatus('success')
          setMessage(data.message || 'تم تأكيد حسابك بنجاح!')
          setTimeout(() => navigate('/'), 3000)
        } else {
          setStatus('error')
          setMessage(data.message || 'رمز التحقق غير صحيح أو منتهي الصلاحية.')
        }
      })
      .catch(() => {
        setStatus('error')
        setMessage('حدث خطأ أثناء الاتصال بالخادم.')
      })
  }, [searchParams, navigate])

  return (
    <>
      <BackgroundAnimation />
      <div className="container" dir="rtl">
        <BrandingSide />
        <div className="form-side">
          <div className="form-container">
            <div className="auth-form" style={{ textAlign: 'center', padding: '40px 20px' }}>

              {status === 'loading' && (
                <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: '20px' }}>
                  <Loader2 size={48} style={{ color: 'var(--primary-color)', animation: 'spin 1s linear infinite' }} />
                  <h2>{message}</h2>
                </div>
              )}

              {status === 'success' && (
                <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: '16px' }}>
                  <CheckCircle2 size={56} style={{ color: '#22c55e' }} />
                  <h2 style={{ color: '#22c55e' }}>تم التأكيد بنجاح!</h2>
                  <p style={{ color: 'var(--text-muted)' }}>{message}</p>
                  <p style={{ color: 'var(--text-muted)', fontSize: '0.85rem' }}>سيتم تحويلك للصفحة الرئيسية...</p>
                  <button className="submit-btn" onClick={() => navigate('/')}>
                    <span className="btn-text">الصفحة الرئيسية</span>
                  </button>
                </div>
              )}

              {status === 'error' && (
                <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: '16px' }}>
                  <XCircle size={56} style={{ color: '#ef4444' }} />
                  <h2 style={{ color: '#ef4444' }}>فشل التأكيد</h2>
                  <p style={{ color: 'var(--text-muted)' }}>{message}</p>
                  <button className="submit-btn" onClick={() => navigate('/auth')}>
                    <span className="btn-text">تسجيل الدخول</span>
                  </button>
                </div>
              )}

            </div>
          </div>
        </div>
      </div>
      <style>{`@keyframes spin { to { transform: rotate(360deg) } }`}</style>
    </>
  )
}
