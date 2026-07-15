import { Link } from 'react-router-dom'
import { Briefcase, PenTool, TrendingUp, Monitor, Globe, Target, Clock, ArrowRight } from 'lucide-react'

export const PATHS = [
  {
    title: 'Business & Project Management',
    desc: 'Learn management strategies, financial planning, and successful team leadership.',
    duration: '4 Months',
    icon: <Briefcase size={22} />,
    color: '#3b82f6'
  },
  {
    title: 'Graphic Design & UI/UX',
    desc: 'Master industry-standard design tools and create compelling, user-friendly digital interfaces.',
    duration: '6 Months',
    icon: <PenTool size={22} />,
    color: '#a855f7'
  },
  {
    title: 'Digital Marketing & Sales',
    desc: 'Modern marketing strategies, advertising campaign management, and data analytics.',
    duration: '3 Months',
    icon: <TrendingUp size={22} />,
    color: '#f59e0b'
  },
  {
    title: 'Software Development & Tech',
    desc: 'Core computer science fundamentals, web development, and AI concepts from scratch.',
    duration: '8 Months',
    icon: <Monitor size={22} />,
    color: '#22c55e'
  },
  {
    title: 'Languages & Professional Communication',
    desc: 'Boost your business English fluency and workplace communication skills.',
    duration: '5 Months',
    icon: <Globe size={22} />,
    color: '#ec4899'
  },
  {
    title: 'Personal Development & Productivity',
    desc: 'Time management, critical thinking, and leadership essentials for personal growth.',
    duration: '2 Months',
    icon: <Target size={22} />,
    color: '#06b6d4'
  },
]

export default function PathsSection() {
  return (
    <section className="paths-section" id="paths" dir="ltr" style={{ marginTop: '40px' }}>
      <div className="section-inner">
        <div className="section-header">
          <span className="section-tag"><Target size={14} /> Learning Tracks</span>
          <h2>Explore Our Learning Paths</h2>
          <p>Choose the domain that matches your career aspirations and start building future-proof skills</p>
        </div>
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
                Explore Path <ArrowRight size={14} />
              </Link>
            </div>
          ))}
        </div>
      </div>
    </section>
  )
}
