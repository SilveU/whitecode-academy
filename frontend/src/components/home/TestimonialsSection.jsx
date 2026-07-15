import { MessageSquare, Star } from 'lucide-react'

function UserIcon({ size }) {
  return (
    <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 0 0 0-4 4v2" />
      <circle cx="12" cy="7" r="4" />
    </svg>
  )
}

export const TESTIMONIALS = [
  {
    name: 'Salem Abdullah',
    role: 'Marketing Manager',
    text: 'The digital marketing track completely changed my approach. The hands-on curriculum helped me double my company sales in a very short period.',
    rating: 5
  },
  {
    name: 'Mona Hassan',
    role: 'Graphic Designer',
    text: 'Courses here go way beyond theory. Real practical assignments allowed me to build a strong portfolio that landed me my current job.',
    rating: 5
  },
  {
    name: 'Karim Mahmoud',
    role: 'Entrepreneur',
    text: 'I successfully launched two tech startups thanks to what I learned in the business management path. Highly recommended for modern careers.',
    rating: 5
  },
]

export default function TestimonialsSection() {
  return (
    <section className="testimonials-section" id="testimonials" dir="ltr">
      <div className="section-inner">
        <div className="section-header">
          <span className="section-tag"><MessageSquare size={14} /> Student Testimonials</span>
          <h2>Inspiring Success Stories</h2>
          <p>Real experiences from ambitious individuals who leveled up their skills and careers with us</p>
        </div>
        <div className="testimonials-grid">
          {TESTIMONIALS.map((t, i) => (
            <div className="testimonial-card" key={i}>
              <div className="testimonial-stars">
                {[...Array(t.rating)].map((_, j) => <Star key={j} size={14} fill="#f59e0b" color="#f59e0b" />)}
              </div>
              <p className="testimonial-text">"{t.text}"</p>
              <div className="testimonial-author">
                <div className="testimonial-avatar">
                  <UserIcon size={18} />
                </div>
                <div>
                  <div className="testimonial-name">{t.name}</div>
                  <div className="testimonial-role">{t.role}</div>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    </section>
  )
}
