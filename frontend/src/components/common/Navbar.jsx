import { useState, useEffect } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { Menu, X, LogOut, LayoutDashboard } from 'lucide-react'
import BrandLogo from './BrandLogo'
import { useAuth } from '../../context/AuthContext'

export default function Navbar() {
  const [scrolled, setScrolled] = useState(false)
  const [mobileOpen, setMobileOpen] = useState(false)
  const location = useLocation()
  const navigate = useNavigate()
  const { isAuthenticated, user, logout } = useAuth()

  useEffect(() => {
    const handleScroll = () => setScrolled(window.scrollY > 20)
    window.addEventListener('scroll', handleScroll)
    return () => window.removeEventListener('scroll', handleScroll)
  }, [])

  const isHome = location.pathname === '/'

  const handleLogout = async () => {
    await logout()
    navigate('/')
  }

  return (
    <nav className={`navbar ${scrolled ? 'scrolled' : ''}`} dir="rtl">
      <div className="nav-inner">
        <Link to="/" className="nav-logo">
          <BrandLogo />
          <span className="nav-logo-text">White <strong>Academy</strong></span>
        </Link>

        <div className={`nav-links ${mobileOpen ? 'open' : ''}`}>
          {isHome ? (
            <>
              <a href="#paths" onClick={() => setMobileOpen(false)}>المسارات التعليمية</a>
              <a href="#features" onClick={() => setMobileOpen(false)}>المميزات</a>
              <a href="#testimonials" onClick={() => setMobileOpen(false)}>آراء الطلاب</a>
            </>
          ) : (
            <>
              <Link to="/paths" onClick={() => setMobileOpen(false)}>المسارات التعليمية</Link>
              <Link to="/about" onClick={() => setMobileOpen(false)}>عن الأكاديمية</Link>
              <Link to="/blog" onClick={() => setMobileOpen(false)}>المدونة</Link>
            </>
          )}
          <div className="nav-cta-mobile">
            {isAuthenticated ? (
              <>
                <Link to="/dashboard" className="nav-btn-primary" onClick={() => setMobileOpen(false)}>لوحة التحكم</Link>
                <button onClick={() => { handleLogout(); setMobileOpen(false) }} className="nav-btn-outline" style={{ background: 'none', cursor: 'pointer', fontFamily: "'Cairo', sans-serif" }}>تسجيل الخروج</button>
              </>
            ) : (
              <Link to="/auth" className="nav-btn-primary" onClick={() => setMobileOpen(false)}>ابدأ الآن</Link>
            )}
          </div>
        </div>

        <div className="nav-actions">
          {isAuthenticated ? (
            <>
              <Link to="/dashboard" className="nav-btn-primary" style={{ display: 'inline-flex', alignItems: 'center', gap: '6px' }}>
                <LayoutDashboard size={15} /> لوحة التحكم
              </Link>
              <button onClick={handleLogout} className="nav-btn-outline" style={{ background: 'none', cursor: 'pointer', fontFamily: "'Cairo', sans-serif", display: 'inline-flex', alignItems: 'center', gap: '6px' }}>
                <LogOut size={14} /> خروج
              </button>
            </>
          ) : (
            <>
              <Link to="/auth" className="nav-btn-outline">تسجيل الدخول</Link>
              <Link to="/auth" className="nav-btn-primary">ابدأ مجاناً</Link>
            </>
          )}
        </div>

        <button className="nav-mobile-toggle" onClick={() => setMobileOpen(!mobileOpen)}>
          {mobileOpen ? <X size={24} /> : <Menu size={24} />}
        </button>
      </div>
    </nav>
  )
}
