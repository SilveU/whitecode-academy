import { GraduationCap, Award, MessageSquare, Briefcase, Target, Users } from 'lucide-react'

export const FEATURES = [
  {
    icon: <GraduationCap size={24} />,
    title: 'محتوى شامل وحديث',
    desc: 'مواد تعليمية مُصممة على يد خبراء وتُحدّث باستمرار لمواكبة سوق العمل.'
  },
  {
    icon: <Award size={24} />,
    title: 'شهادات معتمدة',
    desc: 'احصل على شهادة إتمام موثقة تعزز من سيرتك الذاتية وفرصك المهنية.'
  },
  {
    icon: <MessageSquare size={24} />,
    title: 'مجتمع تفاعلي',
    desc: 'تواصل مع زملائك والمدربين، وتبادل الخبرات في بيئة تعليمية داعمة.'
  },
  {
    icon: <Briefcase size={24} />,
    title: 'تطبيق عملي',
    desc: 'نركز على التطبيق العملي والمشاريع الحقيقية لضمان اكتساب المهارة.'
  },
  {
    icon: <Target size={24} />,
    title: 'مرونة في التعلم',
    desc: 'تعلم بالسرعة التي تناسبك ومن أي مكان في العالم وفي أي وقت.'
  },
  {
    icon: <Users size={24} />,
    title: 'توجيه مهني',
    desc: 'جلسات إرشادية لمساعدتك في التخطيط لمسارك المهني بنجاح.'
  },
]

export default function FeaturesSection() {
  return (
    <section className="features-section" id="features" dir="rtl">
      <div className="section-inner">
        <div className="section-header">
          <span className="section-tag"><Award size={14} /> لماذا تختارنا</span>
          <h2>ما يميّز White Academy</h2>
          <p>نوفر لك بيئة تعليمية متكاملة تساعدك على تحقيق أهدافك بأفضل الطرق</p>
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
