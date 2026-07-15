import { Link } from 'react-router-dom'
import { GraduationCap, ArrowRight, CheckCircle2 } from 'lucide-react'

export default function CTASection() {
  return (
    <section className="cta-section" dir="ltr">
      <div className="section-inner">
        <div className="cta-content">
          <div className="cta-icon-wrap">
            <GraduationCap size={32} />
          </div>
          <h2>Ready to Invest in Your Future?</h2>
          <p>Join thousands of ambitious learners and start building in-demand career skills today.</p>
          <div className="cta-actions">
            <Link to="/auth?tab=register" className="btn-primary-lg">
              <span>Create Free Account</span>
              <ArrowRight size={18} />
            </Link>
          </div>
          <div className="cta-features">
            <span><CheckCircle2 size={15} /> No hidden fees</span>
            <span><CheckCircle2 size={15} /> Lifetime access</span>
            <span><CheckCircle2 size={15} /> Dedicated support</span>
          </div>
        </div>
      </div>
    </section>
  )
}
