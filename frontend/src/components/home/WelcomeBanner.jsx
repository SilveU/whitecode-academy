import { Link } from 'react-router-dom'
import { ArrowLeft, Search, BookOpen, Users, Star, Award, Flame } from 'lucide-react'

export default function WelcomeBanner() {
  return (
    <section className="welcome-banner" dir="rtl">
      <div className="section-inner">
        <div className="welcome-grid">
          <div className="welcome-main">
            <div className="welcome-tag">
              <Flame size={14} />
              <span>أكثر من 10,000 متعلم من جميع أنحاء العالم</span>
            </div>
            <h1>طوّر مهاراتك، <span className="text-gradient">واصنع مستقبلك</span></h1>
            <p>منصتك الشاملة لتعلم المهارات الحديثة في الإدارة، التصميم، التسويق، والتقنية. مسارات عملية بشهادات معتمدة.</p>
            <div className="welcome-actions">
              <Link to="/auth" className="btn-primary-lg">
                <span>ابدأ التعلم مجاناً</span>
                <ArrowLeft size={16} />
              </Link>
              <a href="#paths" className="btn-ghost-lg">
                <Search size={15} />
                <span>تصفح المسارات</span>
              </a>
            </div>
          </div>
          <div className="welcome-stats-cards">
            <div className="mini-stat-card">
              <div className="mini-stat-icon" style={{ color: '#22c55e', background: 'rgba(34,197,94,0.1)' }}>
                <BookOpen size={20} />
              </div>
              <div>
                <div className="mini-stat-value">120+</div>
                <div className="mini-stat-label">مسار تعليمي</div>
              </div>
            </div>
            <div className="mini-stat-card">
              <div className="mini-stat-icon" style={{ color: '#3b82f6', background: 'rgba(59,130,246,0.1)' }}>
                <Users size={20} />
              </div>
              <div>
                <div className="mini-stat-value">10k+</div>
                <div className="mini-stat-label">طالب مسجّل</div>
              </div>
            </div>
            <div className="mini-stat-card">
              <div className="mini-stat-icon" style={{ color: '#f59e0b', background: 'rgba(245,158,11,0.1)' }}>
                <Star size={20} />
              </div>
              <div>
                <div className="mini-stat-value">4.9</div>
                <div className="mini-stat-label">تقييم الطلاب</div>
              </div>
            </div>
            <div className="mini-stat-card">
              <div className="mini-stat-icon" style={{ color: '#a855f7', background: 'rgba(168,85,247,0.1)' }}>
                <Award size={20} />
              </div>
              <div>
                <div className="mini-stat-value">15k+</div>
                <div className="mini-stat-label">شهادة مُصدرة</div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  )
}
