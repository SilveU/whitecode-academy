import PageLayout from '../components/common/PageLayout'
import { PATHS } from '../components/home/PathsSection'
import { Link } from 'react-router-dom'
import { ArrowRight, Clock } from 'lucide-react'

export default function PathsPage() {
  return (
    <PageLayout 
      title="Learning Tracks" 
      subtitle="Select the track that aligns with your career goals and begin learning today"
    >
      <div className="paths-grid">
        {PATHS.map((path, i) => (
          <div className="path-card" key={i}>
            <div className="path-card-header">
              <div className="path-icon" style={{ color: path.color, background: `${path.color}12` }}>
                {path.icon}
              </div>
              <div className="path-duration">
                <Clock size={13} />
                <span>{path.duration}</span>
              </div>
            </div>
            <h3>{path.title}</h3>
            <p style={{ fontSize: '0.88rem', color: 'var(--text-muted)', marginBottom: '20px', lineHeight: '1.6' }}>
              {path.desc}
            </p>
            <Link to="/auth?tab=register" className="path-link">
              Explore Track <ArrowRight size={14} />
            </Link>
          </div>
        ))}
      </div>
    </PageLayout>
  )
}
