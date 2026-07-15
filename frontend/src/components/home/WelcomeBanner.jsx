import { Link } from 'react-router-dom'
import { ArrowRight, Search, BookOpen, Users, Star, Award, Flame } from 'lucide-react'

export default function WelcomeBanner() {
  return (
    <section className="welcome-banner" dir="ltr">
      <div className="section-inner">
        <div className="welcome-grid">
          <div className="welcome-main">
            <div className="welcome-tag">
              <Flame size={14} />
              <span>Over 10,000 learners from around the globe</span>
            </div>
            <h1>Upgrade Your Skills, <span className="text-gradient">Shape Your Future</span></h1>
            <p>Your comprehensive platform to master modern skills in technology, programming, management, and design. Hands-on learning paths with practical certifications.</p>
            <div className="welcome-actions">
              <Link to="/auth?tab=register" className="btn-primary-lg">
                <span>Start Learning Free</span>
                <ArrowRight size={16} />
              </Link>
              <a href="#paths" className="btn-ghost-lg">
                <Search size={15} />
                <span>Explore Paths</span>
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
                <div className="mini-stat-label">Learning Paths</div>
              </div>
            </div>
            <div className="mini-stat-card">
              <div className="mini-stat-icon" style={{ color: '#3b82f6', background: 'rgba(59,130,246,0.1)' }}>
                <Users size={20} />
              </div>
              <div>
                <div className="mini-stat-value">10k+</div>
                <div className="mini-stat-label">Enrolled Students</div>
              </div>
            </div>
            <div className="mini-stat-card">
              <div className="mini-stat-icon" style={{ color: '#f59e0b', background: 'rgba(245,158,11,0.1)' }}>
                <Star size={20} />
              </div>
              <div>
                <div className="mini-stat-value">4.9</div>
                <div className="mini-stat-label">Student Rating</div>
              </div>
            </div>
            <div className="mini-stat-card">
              <div className="mini-stat-icon" style={{ color: '#a855f7', background: 'rgba(168,85,247,0.1)' }}>
                <Award size={20} />
              </div>
              <div>
                <div className="mini-stat-value">15k+</div>
                <div className="mini-stat-label">Certificates Issued</div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  )
}
