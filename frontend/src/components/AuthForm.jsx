import { useState } from 'react'
import {
  Mail,
  Lock,
  User,
  Eye,
  EyeOff,
  LogIn,
  UserPlus,
  ArrowLeft,
  Shield
} from 'lucide-react'

// Password Strength helper
function getPasswordStrength(password) {
  if (!password) return 0
  let strength = 0
  if (password.length >= 6) strength++
  if (password.length >= 10) strength++
  if (/[A-Z]/.test(password) && /[a-z]/.test(password)) strength++
  if (/[0-9]/.test(password)) strength++
  if (/[^A-Za-z0-9]/.test(password)) strength++
  return Math.min(strength, 4)
}

const STRENGTH_LEVELS = [
  { cls: '', text: '', color: 'var(--text-muted)' },
  { cls: 'weak', text: 'ضعيفة', color: '#ef4444' },
  { cls: 'fair', text: 'متوسطة', color: '#f59e0b' },
  { cls: 'good', text: 'جيدة', color: '#22c55e' },
  { cls: 'strong', text: 'قوية', color: '#10b981' },
]

function PasswordStrengthBar({ password }) {
  const strength = getPasswordStrength(password)
  const level = STRENGTH_LEVELS[strength]
  if (!password) return null

  return (
    <div className="password-strength">
      <div className="strength-bars">
        {[0, 1, 2, 3].map((i) => (
          <div key={i} className={`strength-bar ${i < strength ? level.cls : ''}`} />
        ))}
      </div>
      <span className="strength-text" style={{ color: level.color }}>{level.text}</span>
    </div>
  )
}

function PasswordInput({ id, placeholder, value, onChange, ...rest }) {
  const [visible, setVisible] = useState(false)

  return (
    <div className="input-wrapper">
      <span className="input-icon"><Lock size={18} /></span>
      <input
        type={visible ? 'text' : 'password'}
        id={id}
        placeholder={placeholder}
        value={value}
        onChange={onChange}
        {...rest}
      />
      <button type="button" className="toggle-password" onClick={() => setVisible(!visible)}>
        {visible ? <EyeOff size={18} /> : <Eye size={18} />}
      </button>
    </div>
  )
}

// Google Icon (brand SVG)
const GoogleIcon = () => (
  <svg width="18" height="18" viewBox="0 0 24 24">
    <path d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92a5.06 5.06 0 0 1-2.2 3.32v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.1z" fill="#4285F4" />
    <path d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z" fill="#34A853" />
    <path d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z" fill="#FBBC05" />
    <path d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z" fill="#EA4335" />
  </svg>
)

// GitHub Icon (brand SVG)
const GitHubIcon = () => (
  <svg width="18" height="18" viewBox="0 0 24 24" fill="currentColor">
    <path d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z" />
  </svg>
)

// Login Form
function LoginForm({ onSuccess }) {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [remember, setRemember] = useState(false)
  const [loading, setLoading] = useState(false)
  const [shake, setShake] = useState(false)
  const [errorMsg, setErrorMsg] = useState('')

  const handleSubmit = async (e) => {
    e.preventDefault()
    setErrorMsg('')
    if (!email || !password) {
      setShake(true)
      setTimeout(() => setShake(false), 600)
      return
    }
    setLoading(true)
    
    try {
      const response = await fetch('/api/authentication/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'ngrok-skip-browser-warning': 'true'
        },
        body: JSON.stringify({
          identity: email,
          password: password
        })
      });
      
      const data = await response.json().catch(() => null);
      
      if (!response.ok || (data && data.isAuthenticated === false)) {
        setShake(true)
        setErrorMsg(data?.message || 'البريد الإلكتروني أو كلمة المرور غير صحيحة')
        setTimeout(() => setShake(false), 600)
      } else {
        // Success
        onSuccess('تم تسجيل الدخول بنجاح', data?.message || 'مرحباً بعودتك، جاري تحويلك إلى المنصة')
      }
    } catch (error) {
      setShake(true)
      setErrorMsg('حدث خطأ في الاتصال بالخادم. تأكد من تشغيل الباك اند.')
      setTimeout(() => setShake(false), 600)
    } finally {
      setLoading(false)
    }
  }

  return (
    <form className={`auth-form ${shake ? 'shake' : ''}`} onSubmit={handleSubmit}>
      <div className="form-header">
        <div className="form-header-icon">
          <LogIn size={22} />
        </div>
        <div>
          <h2>تسجيل الدخول</h2>
          <p>أدخل بياناتك للوصول إلى حسابك</p>
        </div>
      </div>

      <div className="input-group">
        <label htmlFor="loginEmail">البريد الإلكتروني</label>
        <div className="input-wrapper">
          <span className="input-icon"><Mail size={18} /></span>
          <input
            type="text"
            id="loginEmail"
            placeholder="البريد الإلكتروني أو اسم المستخدم"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
          />
        </div>
      </div>

      <div className="input-group">
        <label htmlFor="loginPassword">كلمة المرور</label>
        <PasswordInput
          id="loginPassword"
          placeholder="أدخل كلمة المرور"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
        />
      </div>

      <div className="form-options">
        <label className="checkbox-wrapper">
          <input type="checkbox" checked={remember} onChange={(e) => setRemember(e.target.checked)} />
          <span className="checkmark" />
          <span>تذكرني</span>
        </label>
        <a href="#" className="forgot-link">نسيت كلمة المرور؟</a>
      </div>

      {errorMsg && <div style={{ color: '#ef4444', fontSize: '0.85rem', textAlign: 'center', marginBottom: '4px' }}>{errorMsg}</div>}

      <button type="submit" className={`submit-btn ${loading ? 'loading' : ''}`} id="loginBtn">
        <span className="btn-text">تسجيل الدخول</span>
        <span className="btn-loader" />
      </button>

      <div className="divider"><span>أو</span></div>

      <div className="social-buttons">
        <button type="button" className="social-btn" id="googleLogin">
          <GoogleIcon /><span>Google</span>
        </button>
        <button type="button" className="social-btn" id="githubLogin">
          <GitHubIcon /><span>GitHub</span>
        </button>
      </div>
    </form>
  )
}

// Register Form
function RegisterForm({ onSuccess }) {
  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [agreeTerms, setAgreeTerms] = useState(false)
  const [loading, setLoading] = useState(false)
  const [shake, setShake] = useState(false)
  const [confirmError, setConfirmError] = useState(false)

  const handleSubmit = async (e) => {
    e.preventDefault()
    if (password !== confirmPassword) {
      setShake(true)
      setConfirmError(true)
      setTimeout(() => { setShake(false); setConfirmError(false) }, 3000)
      return
    }
    setLoading(true)

    try {
      const response = await fetch('/api/authentication/register', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'ngrok-skip-browser-warning': 'true'
        },
        body: JSON.stringify({
          firstName,
          lastName,
          email,
          password,
          confirmPassword
        })
      })

      const data = await response.json().catch(() => null)

      if (!response.ok) {
        setShake(true)
        setConfirmError(true) 
        setTimeout(() => { setShake(false); setConfirmError(false) }, 3000)
      } else {
        // Success - tell them to check email
        onSuccess('تم إنشاء حسابك بنجاح', 'يرجى مراجعة بريدك الإلكتروني لتأكيد الحساب قبل تسجيل الدخول.')
      }
    } catch (err) {
      setShake(true)
      setTimeout(() => setShake(false), 3000)
    } finally {
      setLoading(false)
    }
  }

  return (
    <form className={`auth-form ${shake ? 'shake' : ''}`} onSubmit={handleSubmit}>
      <div className="form-header">
        <div className="form-header-icon">
          <UserPlus size={22} />
        </div>
        <div>
          <h2>إنشاء حساب جديد</h2>
          <p>سجّل الآن وابدأ رحلة التعلم</p>
        </div>
      </div>

      <div className="input-row">
        <div className="input-group">
          <label htmlFor="firstName">الاسم الأول</label>
          <div className="input-wrapper">
            <span className="input-icon"><User size={18} /></span>
            <input type="text" id="firstName" placeholder="الاسم الأول" value={firstName} onChange={(e) => setFirstName(e.target.value)} required />
          </div>
        </div>
        <div className="input-group">
          <label htmlFor="lastName">الاسم الأخير</label>
          <div className="input-wrapper">
            <span className="input-icon"><User size={18} /></span>
            <input type="text" id="lastName" placeholder="الاسم الأخير" value={lastName} onChange={(e) => setLastName(e.target.value)} required />
          </div>
        </div>
      </div>

      <div className="input-group">
        <label htmlFor="registerEmail">البريد الإلكتروني</label>
        <div className="input-wrapper">
          <span className="input-icon"><Mail size={18} /></span>
          <input type="email" id="registerEmail" placeholder="example@email.com" value={email} onChange={(e) => setEmail(e.target.value)} required />
        </div>
      </div>

      <div className="input-group">
        <label htmlFor="registerPassword">كلمة المرور</label>
        <PasswordInput
          id="registerPassword"
          placeholder="أنشئ كلمة مرور قوية"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
        />
        <PasswordStrengthBar password={password} />
      </div>

      <div className="input-group">
        <label htmlFor="confirmPassword">تأكيد كلمة المرور</label>
        <div className={`input-wrapper ${confirmError ? 'error' : ''}`}>
          <span className="input-icon"><Shield size={18} /></span>
          <input type="password" id="confirmPassword" placeholder="أعد إدخال كلمة المرور" value={confirmPassword} onChange={(e) => setConfirmPassword(e.target.value)} required />
        </div>
        {confirmError && <span className="error-message">كلمة المرور غير متطابقة</span>}
      </div>

      <label className="checkbox-wrapper terms-checkbox">
        <input type="checkbox" checked={agreeTerms} onChange={(e) => setAgreeTerms(e.target.checked)} required />
        <span className="checkmark" />
        <span>أوافق على <a href="#">الشروط والأحكام</a> و<a href="#">سياسة الخصوصية</a></span>
      </label>

      <button type="submit" className={`submit-btn ${loading ? 'loading' : ''}`} id="registerBtn">
        <span className="btn-text">إنشاء حساب</span>
        <span className="btn-loader" />
      </button>

      <div className="divider"><span>أو</span></div>

      <div className="social-buttons">
        <button type="button" className="social-btn" id="googleRegister">
          <GoogleIcon /><span>Google</span>
        </button>
        <button type="button" className="social-btn" id="githubRegister">
          <GitHubIcon /><span>GitHub</span>
        </button>
      </div>
    </form>
  )
}

export default function AuthForm({ activeTab, setActiveTab, onSuccess }) {
  return (
    <div className="form-container">
      <div className="tab-switcher">
        <button
          className={`tab-btn ${activeTab === 'login' ? 'active' : ''}`}
          onClick={() => setActiveTab('login')}
          type="button"
        >
          تسجيل الدخول
        </button>
        <button
          className={`tab-btn ${activeTab === 'register' ? 'active' : ''}`}
          onClick={() => setActiveTab('register')}
          type="button"
        >
          إنشاء حساب
        </button>
        <div className={`tab-indicator ${activeTab === 'register' ? 'register' : ''}`} />
      </div>

      {activeTab === 'login' ? (
        <LoginForm key="login" onSuccess={onSuccess} />
      ) : (
        <RegisterForm key="register" onSuccess={onSuccess} />
      )}
    </div>
  )
}
