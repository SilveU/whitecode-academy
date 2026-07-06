import { useState, useEffect } from 'react'
import { Link, useLocation } from 'react-router-dom'
import { Menu, X } from 'lucide-react'
import BrandLogo from './BrandLogo'

export default function Navbar() {
  const [scrolled, setScrolled] = useState(false)
  const [mobileOpen, setMobileOpen] = useState(false)
  const location = useLocation()

  useEffect(() => {
    const handleScroll = () => setScrolled(window.scrollY > 20)
    window.addEventListener('scroll', handleScroll)
    return () => window.removeEventListener('scroll', handleScroll)
  }, [])

  const isHome = location.pathname === '/'

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
            <Link to="/auth" className="nav-btn-primary" onClick={() => setMobileOpen(false)}>ابدأ الآن</Link>
          </div>
        </div>

        <div className="nav-actions">
          <Link to="/auth" className="nav-btn-outline">تسجيل الدخول</Link>
          <Link to="/auth" className="nav-btn-primary">ابدأ مجاناً</Link>
        </div>

        <button className="nav-mobile-toggle" onClick={() => setMobileOpen(!mobileOpen)}>
          {mobileOpen ? <X size={24} /> : <Menu size={24} />}
        </button>
      </div>
    </nav>
  )
}
