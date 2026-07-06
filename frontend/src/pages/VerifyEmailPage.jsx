import { useState, useEffect, useRef } from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import { CheckCircle2, XCircle, Loader2, ArrowRight } from 'lucide-react'
import BackgroundAnimation from '../components/BackgroundAnimation'
import BrandingSide from '../components/BrandingSide'

export default function VerifyEmailPage() {
  const navigate = useNavigate()
  const location = useLocation()
  const [status, setStatus] = useState('loading') // 'loading', 'success', 'error'
  const [message, setMessage] = useState('جاري التحقق من الحساب...')
  const hasFetched = useRef(false)

  useEffect(() => {
    if (hasFetched.current) return
    hasFetched.current = true

    const verifyEmail = async () => {
      const searchParams = new URLSearchParams(location.search)
      const userId = searchParams.get('userId')
      const token = searchParams.get('token')

      if (!userId || !token) {
        setStatus('error')
        setMessage('رابط التحقق غير صالح أو مفقود.')
        return
      }

      try {
        // We add Accept: application/json so the Vite proxy does not bypass it
        const response = await fetch(`/api/authentication/confirm-email?userId=${encodeURIComponent(userId)}&token=${encodeURIComponent(token)}`, {
          method: 'GET',
          headers: {
            'Accept': 'application/json',
            'ngrok-skip-browser-warning': 'true'
          }
        })

        const data = await response.json().catch(() => null)

        if (!response.ok || (data && data.isAuthenticated === false)) {
          setStatus('error')
          setMessage(data?.message || 'رمز التحقق غير صحيح أو منتهي الصلاحية.')
        } else {
          setStatus('success')
          setMessage(data?.message || 'تم تأكيد حسابك بنجاح!')
        }
      } catch (error) {
        setStatus('error')
        setMessage('حدث خطأ أثناء الاتصال بالخادم. حاول مرة أخرى.')
      }
    }

    verifyEmail()
  }, [location])

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
                  <Loader2 size={48} className="spin" style={{ color: 'var(--primary-color)' }} />
                  <h2>جاري التأكيد...</h2>
                  <p style={{ color: 'var(--text-muted)' }}>{message}</p>
                </div>
              )}

              {status === 'success' && (
                <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: '20px' }}>
                  <CheckCircle2 size={64} style={{ color: '#22c55e' }} />
                  <h2 style={{ color: '#22c55e' }}>عملية ناجحة</h2>
                  <p style={{ color: 'var(--text-muted)' }}>{message}</p>
                  <button 
                    className="submit-btn" 
                    style={{ marginTop: '20px', width: 'auto', padding: '0 40px' }}
                    onClick={() => navigate('/auth')}
                  >
                    <span className="btn-text">تسجيل الدخول</span>
                  </button>
                </div>
              )}

              {status === 'error' && (
                <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: '20px' }}>
                  <XCircle size={64} style={{ color: '#ef4444' }} />
                  <h2 style={{ color: '#ef4444' }}>فشل التأكيد</h2>
                  <p style={{ color: 'var(--text-muted)' }}>{message}</p>
                  <button 
                    className="submit-btn" 
                    style={{ marginTop: '20px', width: 'auto', padding: '0 40px', background: 'rgba(255, 255, 255, 0.1)' }}
                    onClick={() => navigate('/auth')}
                  >
                    <span className="btn-text">العودة للرئيسية</span>
                  </button>
                </div>
              )}

            </div>
          </div>
        </div>
      </div>

      <style>{`
        .spin {
          animation: spin 1s linear infinite;
        }
        @keyframes spin {
          from { transform: rotate(0deg); }
          to { transform: rotate(360deg); }
        }
      `}</style>
    </>
  )
}
