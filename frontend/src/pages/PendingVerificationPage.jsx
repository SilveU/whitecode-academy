import { useNavigate } from 'react-router-dom'
import { Mail, ArrowRight } from 'lucide-react'
import BackgroundAnimation from '../components/BackgroundAnimation'
import BrandingSide from '../components/BrandingSide'

export default function PendingVerificationPage() {
  const navigate = useNavigate()

  return (
    <>
      <BackgroundAnimation />
      <div className="container" dir="rtl">
        <BrandingSide />
        <div className="form-side">
          <div className="form-container">
            <div className="auth-form" style={{ textAlign: 'center', padding: '40px 20px' }}>

              <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: '20px' }}>
                <div style={{
                  width: '80px', height: '80px', borderRadius: '50%',
                  background: 'rgba(99, 102, 241, 0.15)',
                  display: 'flex', alignItems: 'center', justifyContent: 'center'
                }}>
                  <Mail size={40} style={{ color: 'var(--primary-color, #6366f1)' }} />
                </div>

                <h2 style={{ margin: 0 }}>تحقق من بريدك الإلكتروني</h2>

                <p style={{ color: 'var(--text-muted)', lineHeight: 1.8, maxWidth: '400px' }}>
                  تم إرسال رابط التحقق إلى بريدك الإلكتروني.
                  <br />
                  يرجى فتح الإيميل والضغط على الرابط لتفعيل حسابك.
                </p>

                <div style={{
                  padding: '16px', borderRadius: '12px',
                  background: 'rgba(255,255,255,0.05)',
                  border: '1px solid rgba(255,255,255,0.1)',
                  width: '100%', maxWidth: '400px'
                }}>
                  <p style={{ color: 'var(--text-muted)', fontSize: '0.85rem', margin: 0 }}>
                    💡 لم تجد الإيميل؟ تحقق من مجلد الـ Spam
                  </p>
                </div>

                <button
                  className="submit-btn"
                  style={{ marginTop: '10px' }}
                  onClick={() => navigate('/auth')}
                >
                  <span className="btn-text" style={{ display: 'flex', alignItems: 'center', gap: '8px', justifyContent: 'center' }}>
                    <ArrowRight size={18} />
                    العودة لتسجيل الدخول
                  </span>
                </button>
              </div>

            </div>
          </div>
        </div>
      </div>
    </>
  )
}
