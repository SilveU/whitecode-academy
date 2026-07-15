import { useState } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import {
  Mail,
  Lock,
  User,
  Eye,
  EyeOff,
  LogIn,
  UserPlus,
  ArrowLeft,
  Shield,
  Clock
} from 'lucide-react'
import { useAuth } from '../context/AuthContext'
import { authApi } from '../services/api'

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
  { cls: 'weak', text: 'Weak', color: '#ef4444' },
  { cls: 'fair', text: 'Medium', color: '#f59e0b' },
  { cls: 'good', text: 'Good', color: '#22c55e' },
  { cls: 'strong', text: 'Strong', color: '#10b981' },
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
  const { login } = useAuth()
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [remember, setRemember] = useState(false)
  const [loading, setLoading] = useState(false)
  const [shake, setShake] = useState(false)
  const [errorMsg, setErrorMsg] = useState('')
  const [socialNotice, setSocialNotice] = useState('')

  const handleSocialClick = (provider) => {
    setSocialNotice(`${provider} login is awaiting backend endpoint deployment. Please sign in with Email & Password.`)
    setTimeout(() => setSocialNotice(''), 6000)
  }

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
      const result = await login(email, password)
      
      if (!result.success) {
        setShake(true)
        setErrorMsg(result.message || 'Incorrect email or password')
        setTimeout(() => setShake(false), 600)
      } else {
        // Success - redirect to dashboard
        navigate('/dashboard')
      }
    } catch (error) {
      setShake(true)
      setErrorMsg('Server connection error. Please ensure the backend is running.')
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
          <h2>Sign In</h2>
          <p>Enter your credentials to access your account</p>
        </div>
      </div>

      <div className="input-group">
        <label htmlFor="loginEmail">Email Address</label>
        <div className="input-wrapper">
          <span className="input-icon"><Mail size={18} /></span>
          <input
            type="text"
            id="loginEmail"
            placeholder="Email or username"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
          />
        </div>
      </div>

      <div className="input-group">
        <label htmlFor="loginPassword">Password</label>
        <PasswordInput
          id="loginPassword"
          placeholder="Enter your password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
        />
      </div>

      <div className="form-options">
        <label className="checkbox-wrapper">
          <input type="checkbox" checked={remember} onChange={(e) => setRemember(e.target.checked)} />
          <span className="checkmark" />
          <span>Remember me</span>
        </label>
        <span className="forgot-link disabled-link" title="This feature is coming soon">Forgot password?</span>
      </div>

      {errorMsg && <div style={{ color: '#ef4444', fontSize: '0.85rem', textAlign: 'center', marginBottom: '4px' }}>{errorMsg}</div>}

      <button type="submit" className={`submit-btn ${loading ? 'loading' : ''}`} id="loginBtn">
        <span className="btn-text">Sign In</span>
        <span className="btn-loader" />
      </button>

      <div className="divider"><span>or</span></div>

      {socialNotice && (
        <div style={{ padding: '10px 14px', background: 'rgba(245, 158, 11, 0.1)', border: '1px solid rgba(245, 158, 11, 0.3)', borderRadius: '8px', color: '#fbbf24', fontSize: '0.82rem', textAlign: 'center', marginBottom: '12px', lineHeight: 1.5 }}>
          {socialNotice}
        </div>
      )}

      <div className="social-buttons">
        <button type="button" className="social-btn social-btn-disabled" id="googleLogin" title="Click for status" onClick={() => handleSocialClick('Google')}>
          <GoogleIcon /><span>Google</span>
          <span className="coming-soon-badge"><Clock size={10} /> Soon</span>
        </button>
        <button type="button" className="social-btn social-btn-disabled" id="githubLogin" title="Click for status" onClick={() => handleSocialClick('GitHub')}>
          <GitHubIcon /><span>GitHub</span>
          <span className="coming-soon-badge"><Clock size={10} /> Soon</span>
        </button>
      </div>
    </form>
  )
}

// Register Form
function RegisterForm({ onSuccess }) {
  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [userName, setUserName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [agreeTerms, setAgreeTerms] = useState(false)
  const [loading, setLoading] = useState(false)
  const [shake, setShake] = useState(false)
  const [confirmError, setConfirmError] = useState(false)
  const [apiError, setApiError] = useState('')
  const [socialNotice, setSocialNotice] = useState('')

  const handleSocialClick = (provider) => {
    setSocialNotice(`${provider} registration is awaiting backend endpoint deployment. Please register with Email & Password.`)
    setTimeout(() => setSocialNotice(''), 6000)
  }

  const navigate = useNavigate()

  const handleSubmit = async (e) => {
    e.preventDefault()
    setApiError('')
    if (password !== confirmPassword) {
      setShake(true)
      setConfirmError(true)
      setTimeout(() => { setShake(false); setConfirmError(false) }, 3000)
      return
    }
    setLoading(true)

    try {
      const res = await authApi.register({
        firstName,
        lastName,
        userName,
        email,
        password,
        confirmPassword
      })

      if (!res.ok) {
        setShake(true)
        setApiError(res.message || 'An error occurred during registration. Please try again.')
        setTimeout(() => setShake(false), 600)
      } else {
        // Success - redirect to pending verification page (client-side)
        navigate('/pending-verification')
      }
    } catch (err) {
      setShake(true)
      setApiError('Server connection error. Please ensure the backend is running.')
      setTimeout(() => setShake(false), 600)
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
          <h2>Create New Account</h2>
          <p>Register now and begin your learning journey</p>
        </div>
      </div>

      <div className="input-row">
        <div className="input-group">
          <label htmlFor="firstName">First Name</label>
          <div className="input-wrapper">
            <span className="input-icon"><User size={18} /></span>
            <input type="text" id="firstName" placeholder="First name" value={firstName} onChange={(e) => setFirstName(e.target.value)} required />
          </div>
        </div>
        <div className="input-group">
          <label htmlFor="lastName">Last Name</label>
          <div className="input-wrapper">
            <span className="input-icon"><User size={18} /></span>
            <input type="text" id="lastName" placeholder="Last name" value={lastName} onChange={(e) => setLastName(e.target.value)} required />
          </div>
        </div>
      </div>

      <div className="input-group">
        <label htmlFor="userName">Username</label>
        <div className="input-wrapper">
          <span className="input-icon"><User size={18} /></span>
          <input type="text" id="userName" placeholder="e.g. youssef_99" value={userName} onChange={(e) => setUserName(e.target.value)} maxLength={30} required />
        </div>
      </div>

      <div className="input-group">
        <label htmlFor="registerEmail">Email Address</label>
        <div className="input-wrapper">
          <span className="input-icon"><Mail size={18} /></span>
          <input type="email" id="registerEmail" placeholder="example@email.com" value={email} onChange={(e) => setEmail(e.target.value)} required />
        </div>
      </div>

      <div className="input-group">
        <label htmlFor="registerPassword">Password</label>
        <PasswordInput
          id="registerPassword"
          placeholder="Create a strong password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
        />
        <PasswordStrengthBar password={password} />
      </div>

      <div className="input-group">
        <label htmlFor="confirmPassword">Confirm Password</label>
        <PasswordInput
          id="confirmPassword"
          placeholder="Re-enter your password"
          value={confirmPassword}
          onChange={(e) => setConfirmPassword(e.target.value)}
          required
        />
        {confirmError && <span className="error-message">Passwords do not match</span>}
      </div>

      {apiError && <div style={{ color: '#ef4444', fontSize: '0.85rem', textAlign: 'center', marginBottom: '4px', padding: '8px', background: 'rgba(239,68,68,0.1)', borderRadius: '8px' }}>{apiError}</div>}

      <label className="checkbox-wrapper terms-checkbox">
        <input type="checkbox" checked={agreeTerms} onChange={(e) => setAgreeTerms(e.target.checked)} required />
        <span className="checkmark" />
        <span>I agree to the <Link to="/terms" target="_blank">Terms & Conditions</Link> and <Link to="/terms" target="_blank">Privacy Policy</Link></span>
      </label>

      <button type="submit" className={`submit-btn ${loading ? 'loading' : ''}`} id="registerBtn">
        <span className="btn-text">Create Account</span>
        <span className="btn-loader" />
      </button>

      <div className="divider"><span>or</span></div>

      {socialNotice && (
        <div style={{ padding: '10px 14px', background: 'rgba(245, 158, 11, 0.1)', border: '1px solid rgba(245, 158, 11, 0.3)', borderRadius: '8px', color: '#fbbf24', fontSize: '0.82rem', textAlign: 'center', marginBottom: '12px', lineHeight: 1.5 }}>
          {socialNotice}
        </div>
      )}

      <div className="social-buttons">
        <button type="button" className="social-btn social-btn-disabled" id="googleRegister" title="Click for status" onClick={() => handleSocialClick('Google')}>
          <GoogleIcon /><span>Google</span>
          <span className="coming-soon-badge"><Clock size={10} /> Soon</span>
        </button>
        <button type="button" className="social-btn social-btn-disabled" id="githubRegister" title="Click for status" onClick={() => handleSocialClick('GitHub')}>
          <GitHubIcon /><span>GitHub</span>
          <span className="coming-soon-badge"><Clock size={10} /> Soon</span>
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
          Sign In
        </button>
        <button
          className={`tab-btn ${activeTab === 'register' ? 'active' : ''}`}
          onClick={() => setActiveTab('register')}
          type="button"
        >
          Create Account
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
