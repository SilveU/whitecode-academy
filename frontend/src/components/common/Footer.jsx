import { Link } from 'react-router-dom'
import BrandLogo from './BrandLogo'

export default function Footer() {
  return (
    <footer className="site-footer" dir="rtl">
      <div className="section-inner">
        <div className="footer-grid">
          <div className="footer-brand">
            <div className="footer-logo">
              <BrandLogo />
              <span className="nav-logo-text">White <strong>Academy</strong></span>
            </div>
            <p>منصة تعليمية رائدة تهدف إلى تمكين الأفراد بالمهارات والمعارف اللازمة للنجاح في العصر الحديث.</p>
          </div>
          <div className="footer-links-group">
            <h4>المنصة</h4>
            <Link to="/paths">المسارات</Link>
            <Link to="/#features">المميزات</Link>
            <Link to="/blog">المدونة</Link>
            <Link to="/about">عن الأكاديمية</Link>
          </div>
          <div className="footer-links-group">
            <h4>الدعم</h4>
            <Link to="/help">مركز المساعدة</Link>
            <Link to="/faq">الأسئلة الشائعة</Link>
            <Link to="/contact">تواصل معنا</Link>
            <Link to="/terms">الشروط والأحكام</Link>
          </div>
          <div className="footer-links-group">
            <h4>تواصل معنا</h4>
            <a href="mailto:hello@whiteacademy.com">hello@whiteacademy.com</a>
            <a href="#" target="_blank" rel="noreferrer">Twitter / X</a>
            <a href="#" target="_blank" rel="noreferrer">LinkedIn</a>
            <a href="#" target="_blank" rel="noreferrer">Instagram</a>
          </div>
        </div>
        <div className="footer-bottom">
          <p>&copy; 2026 White Academy. جميع الحقوق محفوظة.</p>
        </div>
      </div>
    </footer>
  )
}
