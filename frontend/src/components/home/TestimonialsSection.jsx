import { MessageSquare, Star } from 'lucide-react'

function UserIcon({ size }) {
  return (
    <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" />
      <circle cx="12" cy="7" r="4" />
    </svg>
  )
}

export const TESTIMONIALS = [
  {
    name: 'سالم عبدالله',
    role: 'مدير تسويق',
    text: 'مسار التسويق الرقمي غيّر نظرتي بالكامل. المحتوى كان عملي جداً وساعدني في مضاعفة مبيعات شركتي في فترة قصيرة.',
    rating: 5
  },
  {
    name: 'منى حسن',
    role: 'مصممة جرافيك',
    text: 'الدورات هنا ليست مجرد تنظير. التطبيق العملي والمشاريع ساعدتني في بناء معرض أعمال قوي حصلت بسببه على وظيفتي الحالية.',
    rating: 5
  },
  {
    name: 'كريم محمود',
    role: 'رائد أعمال',
    text: 'بدأت مشروعين ناشئين بفضل ما تعلمته في مسار إدارة الأعمال. المنصة توفر كل ما تحتاجه للنجاح في سوق العمل اليوم.',
    rating: 5
  },
]

export default function TestimonialsSection() {
  return (
    <section className="testimonials-section" id="testimonials" dir="rtl">
      <div className="section-inner">
        <div className="section-header">
          <span className="section-tag"><MessageSquare size={14} /> آراء المتعلمين</span>
          <h2>قصص نجاح نلهم بها</h2>
          <p>تجارب حقيقية لأشخاص طوّروا مهاراتهم وحققوا أهدافهم معنا</p>
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
