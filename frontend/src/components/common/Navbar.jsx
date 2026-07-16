import { useState, useEffect } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { Menu, X, LogOut, LayoutDashboard, Sun, Moon } from 'lucide-react'
import BrandLogo from './BrandLogo'
import { useAuth } from '../../context/AuthContext'
import { useTheme } from '../../context/ThemeContext'

export default function Navbar() {
  const [scrolled, setScrolled] = useState(false)
  const [mobileOpen, setMobileOpen] = useState(false)
  const location = useLocation()
  const navigate = useNavigate()
  const { isAuthenticated, user, logout } = useAuth()
  const { isDark, toggleTheme } = useTheme()

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
    <nav className={`navbar ${scrolled ? 'scrolled' : ''}`} dir="ltr">
      <div className="nav-inner">
        <Link to="/" className="nav-logo">
          <BrandLogo />
          <span className="nav-logo-text">White <strong>Academy</strong></span>
        </Link>

        <div className={`nav-links ${mobileOpen ? 'open' : ''}`}>
          {isHome ? (
            <>
              <a href="#paths" onClick={() => setMobileOpen(false)}>Learning Paths</a>
              <a href="#features" onClick={() => setMobileOpen(false)}>Features</a>
              <a href="#testimonials" onClick={() => setMobileOpen(false)}>Testimonials</a>
            </>
          ) : (
            <>
              <Link to="/paths" onClick={() => setMobileOpen(false)}>Learning Paths</Link>
              <Link to="/about" onClick={() => setMobileOpen(false)}>About Us</Link>
              <Link to="/blog" onClick={() => setMobileOpen(false)}>Blog</Link>
            </>
          )}
          <div className="nav-cta-mobile">
            {isAuthenticated ? (
              <>
                <Link to="/dashboard" className="nav-btn-primary" onClick={() => setMobileOpen(false)}>Dashboard</Link>
                <button onClick={() => { handleLogout(); setMobileOpen(false) }} className="nav-btn-outline" style={{ background: 'none', cursor: 'pointer' }}>Logout</button>
              </>
            ) : (
              <Link to="/auth?tab=register" className="nav-btn-primary" onClick={() => setMobileOpen(false)}>Get Started</Link>
            )}
          </div>
        </div>

        <div className="nav-actions">
          <button className="theme-toggle" onClick={toggleTheme} title="Toggle Theme" style={{ marginRight: '8px' }}>
            {isDark ? <Sun size={18} /> : <Moon size={18} />}
          </button>
          
          {isAuthenticated ? (
            <>
              <Link to="/dashboard" className="nav-btn-primary" style={{ display: 'inline-flex', alignItems: 'center', gap: '6px' }}>
                <LayoutDashboard size={15} /> Dashboard
              </Link>
              <button onClick={handleLogout} className="nav-btn-outline" style={{ background: 'none', cursor: 'pointer', display: 'inline-flex', alignItems: 'center', gap: '6px' }}>
                <LogOut size={14} /> Logout
              </button>
            </>
          ) : (
            <>
              <Link to="/auth?tab=login" className="nav-btn-outline">Log In</Link>
              <Link to="/auth?tab=register" className="nav-btn-primary">Start Free</Link>
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
