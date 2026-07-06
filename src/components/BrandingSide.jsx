import { Link } from 'react-router-dom'
import { Code2, Users, Award } from 'lucide-react'

export default function BrandingSide() {
  return (
    <div className="branding-side">
      <div className="brand-content">
        <Link to="/" className="logo-container">
          <div className="logo-icon">
            <svg viewBox="0 0 60 60" fill="none" xmlns="http://www.w3.org/2000/svg">
              <rect x="2" y="2" width="56" height="56" rx="14" stroke="currentColor" strokeWidth="3" />
              <text x="10" y="40" fontFamily="monospace" fontSize="28" fontWeight="bold" fill="currentColor">
                &lt;/&gt;
              </text>
            </svg>
          </div>
          <h1 className="brand-name">
            White Code<br />
            <span>Academy</span>
          </h1>
        </Link>

        <p className="brand-tagline">منصتك لتعلم البرمجة وتطوير المهارات التقنية</p>

        <div className="features-list">
          <div className="feature-item">
            <div className="feature-icon">
              <Code2 size={22} />
            </div>
            <div className="feature-text">
              <h3>دورات تفاعلية</h3>
              <p>محتوى عملي بأحدث التقنيات</p>
            </div>
          </div>

          <div className="feature-item">
            <div className="feature-icon">
              <Users size={22} />
            </div>
            <div className="feature-text">
              <h3>مجتمع المطورين</h3>
              <p>تواصل مع آلاف المبرمجين</p>
            </div>
          </div>

          <div className="feature-item">
            <div className="feature-icon">
              <Award size={22} />
            </div>
            <div className="feature-text">
              <h3>شهادات معتمدة</h3>
              <p>شهادة إتمام لكل دورة</p>
            </div>
          </div>
        </div>
      </div>

      <div className="brand-footer">
        <p>&copy; 2026 White Code Academy. جميع الحقوق محفوظة</p>
      </div>
    </div>
  )
}
