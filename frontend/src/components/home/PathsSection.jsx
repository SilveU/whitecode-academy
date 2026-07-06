import { Link } from 'react-router-dom'
import { Briefcase, PenTool, TrendingUp, Monitor, Globe, Target, Clock, ArrowLeft } from 'lucide-react'

export const PATHS = [
  {
    title: 'إدارة الأعمال والمشاريع',
    desc: 'تعلم استراتيجيات الإدارة، التخطيط المالي، وإدارة فرق العمل بنجاح.',
    duration: '4 أشهر',
    icon: <Briefcase size={22} />,
    color: '#3b82f6'
  },
  {
    title: 'التصميم الجرافيكي وتجربة المستخدم',
    desc: 'احترف أدوات التصميم وبناء واجهات مستخدم جذابة وعملية.',
    duration: '6 أشهر',
    icon: <PenTool size={22} />,
    color: '#a855f7'
  },
  {
    title: 'التسويق الرقمي والمبيعات',
    desc: 'استراتيجيات التسويق الحديثة، إدارة الحملات الإعلانية، وتحليل البيانات.',
    duration: '3 أشهر',
    icon: <TrendingUp size={22} />,
    color: '#f59e0b'
  },
  {
    title: 'المهارات التقنية والبرمجة',
    desc: 'أساسيات التقنية، تطوير المواقع، والذكاء الاصطناعي للمبتدئين.',
    duration: '8 أشهر',
    icon: <Monitor size={22} />,
    color: '#22c55e'
  },
  {
    title: 'اللغات والتواصل الفعال',
    desc: 'طوّر مهاراتك في اللغة الإنجليزية والتواصل المهني في بيئة العمل.',
    duration: '5 أشهر',
    icon: <Globe size={22} />,
    color: '#ec4899'
  },
  {
    title: 'تطوير الذات والإنتاجية',
    desc: 'مهارات تنظيم الوقت، التفكير النقدي، والقيادة الشخصية.',
    duration: 'شهران',
    icon: <Target size={22} />,
    color: '#06b6d4'
  },
]

export default function PathsSection() {
  return (
    <section className="paths-section" id="paths" dir="rtl" style={{ marginTop: '40px' }}>
      <div className="section-inner">
        <div className="section-header">
          <span className="section-tag"><Target size={14} /> مجالات التعلم</span>
          <h2>استكشف مساراتنا التعليمية</h2>
          <p>اختر المجال الذي يناسب طموحك وابدأ رحلة التطور المهني والشخصي</p>
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
              <Link to="/auth" className="path-link">
                تصفح المسار <ArrowLeft size={14} />
              </Link>
            </div>
          ))}
        </div>
      </div>
    </section>
  )
}
