import { GraduationCap, Award, MessageSquare, Briefcase, Target, Users } from 'lucide-react'

export const FEATURES = [
  {
    icon: <GraduationCap size={24} />,
    title: 'Comprehensive & Up-to-Date',
    desc: 'Curriculum designed by industry experts and continuously updated to meet market demands.'
  },
  {
    icon: <Award size={24} />,
    title: 'Verified Certificates',
    desc: 'Earn recognized completion certificates that bolster your CV and career opportunities.'
  },
  {
    icon: <MessageSquare size={24} />,
    title: 'Interactive Community',
    desc: 'Connect with peers and instructors to exchange experiences in a collaborative learning environment.'
  },
  {
    icon: <Briefcase size={24} />,
    title: 'Hands-on Projects',
    desc: 'We emphasize practical execution and real-world projects to ensure mastery of skills.'
  },
  {
    icon: <Target size={24} />,
    title: 'Flexible Learning',
    desc: 'Learn at your own pace, anytime, from anywhere around the world.'
  },
  {
    icon: <Users size={24} />,
    title: 'Career Mentorship',
    desc: 'Dedicated guidance and mentoring sessions to help you navigate and plan your professional path.'
  },
]

export default function FeaturesSection() {
  return (
    <section className="features-section" id="features" dir="ltr">
      <div className="section-inner">
        <div className="section-header">
          <span className="section-tag"><Award size={14} /> Why Choose Us</span>
          <h2>The White Academy Advantage</h2>
          <p>We provide a comprehensive learning ecosystem designed to help you reach your goals effectively</p>
        </div>
        <div className="features-grid">
          {FEATURES.map((f, i) => (
            <div className="feature-card" key={i}>
              <div className="feature-card-icon">{f.icon}</div>
              <h3>{f.title}</h3>
              <p>{f.desc}</p>
            </div>
          ))}
        </div>
      </div>
    </section>
  )
}
