import { Link } from 'react-router-dom'
import { GraduationCap, ArrowLeft, CheckCircle2 } from 'lucide-react'

export default function CTASection() {
  return (
    <section className="cta-section" dir="rtl">
      <div className="section-inner">
        <div className="cta-content">
          <div className="cta-icon-wrap">
            <GraduationCap size={32} />
          </div>
          <h2>جاهز للاستثمار في مستقبلك؟</h2>
          <p>انضم لآلاف المتعلمين وابدأ رحلتك لاكتساب المهارات الأكثر طلباً اليوم.</p>
          <div className="cta-actions">
            <Link to="/auth" className="btn-primary-lg">
              <span>أنشئ حسابك المجاني</span>
              <ArrowLeft size={18} />
            </Link>
          </div>
          <div className="cta-features">
            <span><CheckCircle2 size={15} /> بدون رسوم خفية</span>
            <span><CheckCircle2 size={15} /> وصول مدى الحياة للمحتوى</span>
            <span><CheckCircle2 size={15} /> دعم فني مستمر</span>
          </div>
        </div>
      </div>
    </section>
  )
}
