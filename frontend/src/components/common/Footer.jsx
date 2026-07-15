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
            <div style={{ marginTop: '16px', padding: '12px', background: 'rgba(255,255,255,0.02)', borderRadius: '8px', border: '1px solid var(--border)' }}>
              <p style={{ fontSize: '0.75rem', color: 'var(--text-muted)', margin: 0, lineHeight: 1.6 }}>
                <strong>تنويه:</strong> المحتوى المتوفر على المنصة هو عبارة عن دورات وفيديوهات مدفوعة قمنا بشراؤها وترجمتها للغة العربية، مع إضافة أسئلة (Quizzes) وتكليفات عملية (Assignments) لضمان أقصى استفادة. حقوق الملكية الأصلية تعود لأصحابها.
              </p>
            </div>
          </div>
          <div className="footer-links-group">
            <h4>المنصة</h4>
            <Link to="/paths">المسارات</Link>
            <Link to="/about">المميزات</Link>
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
            <a href="https://x.com" target="_blank" rel="noreferrer">Twitter / X</a>
            <a href="https://linkedin.com" target="_blank" rel="noreferrer">LinkedIn</a>
            <a href="https://instagram.com" target="_blank" rel="noreferrer">Instagram</a>
          </div>
        </div>
        <div className="footer-bottom">
          <p>&copy; 2026 White Academy. جميع الحقوق محفوظة.</p>
        </div>
      </div>
    </footer>
  )
}
