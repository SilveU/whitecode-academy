import { Link } from 'react-router-dom'
import { Award, Briefcase, PenTool } from 'lucide-react'

// Global Logo Icon (Abstract W / Academy Icon)
function BrandLogo() {
  return (
    <svg viewBox="0 0 60 60" fill="none" xmlns="http://www.w3.org/2000/svg">
      <rect x="2" y="2" width="56" height="56" rx="14" stroke="currentColor" strokeWidth="3" />
      <path d="M15 20L25 40L30 30L35 40L45 20" stroke="currentColor" strokeWidth="4" strokeLinecap="round" strokeLinejoin="round"/>
    </svg>
  )
}

export default function BrandingSide() {
  return (
    <div className="branding-side">
      <div className="brand-content">
        <Link to="/" className="logo-container">
          <div className="logo-icon">
            <BrandLogo />
          </div>
          <h1 className="brand-name">
            White<br />
            <span>Academy</span>
          </h1>
        </Link>

        <p className="brand-tagline">منصتك الشاملة لتعلم المهارات وتطوير مستقبلك المهني</p>

        <div className="features-list">
          <div className="feature-item">
            <div className="feature-icon">
              <Briefcase size={22} />
            </div>
            <div className="feature-text">
              <h3>مسارات مهنية</h3>
              <p>محتوى عملي يواكب متطلبات سوق العمل</p>
            </div>
          </div>

          <div className="feature-item">
            <div className="feature-icon">
              <PenTool size={22} />
            </div>
            <div className="feature-text">
              <h3>تطبيق عملي</h3>
              <p>مشاريع حقيقية لتنمية مهاراتك</p>
            </div>
          </div>

          <div className="feature-item">
            <div className="feature-icon">
              <Award size={22} />
            </div>
            <div className="feature-text">
              <h3>شهادات معتمدة</h3>
              <p>شهادة إتمام لكل مسار تعليمي</p>
            </div>
          </div>
        </div>
      </div>

      <div className="brand-footer">
        <p>&copy; 2026 White Academy. جميع الحقوق محفوظة</p>
      </div>
    </div>
  )
}
