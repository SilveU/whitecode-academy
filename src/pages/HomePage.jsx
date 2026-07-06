import { useState, useEffect } from 'react'
import { Link } from 'react-router-dom'
import {
  Users,
  Award,
  BookOpen,
  Monitor,
  ArrowLeft,
  Star,
  Clock,
  Target,
  CheckCircle2,
  Menu,
  X,
  GraduationCap,
  MessageSquare,
  Briefcase,
  PenTool,
  TrendingUp,
  Flame,
  Search,
  Globe
} from 'lucide-react'
import './HomePage.css'

// User icon for avatars
function UserIcon({ size }) {
  return (
    <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
      <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" />
      <circle cx="12" cy="7" r="4" />
    </svg>
  )
}

// Global Logo Icon (Abstract W / Academy Icon)
function BrandLogo() {
  return (
    <svg viewBox="0 0 60 60" fill="none" xmlns="http://www.w3.org/2000/svg" className="nav-logo-icon">
      <rect x="2" y="2" width="56" height="56" rx="14" stroke="currentColor" strokeWidth="3" />
      <path d="M15 20L25 40L30 30L35 40L45 20" stroke="currentColor" strokeWidth="4" strokeLinecap="round" strokeLinejoin="round"/>
    </svg>
  )
}

// Navbar
function Navbar() {
  const [scrolled, setScrolled] = useState(false)
  const [mobileOpen, setMobileOpen] = useState(false)

  useEffect(() => {
    const handleScroll = () => setScrolled(window.scrollY > 20)
    window.addEventListener('scroll', handleScroll)
    return () => window.removeEventListener('scroll', handleScroll)
  }, [])

  return (
    <nav className={`navbar ${scrolled ? 'scrolled' : ''}`} dir="rtl">
      <div className="nav-inner">
        <Link to="/" className="nav-logo">
          <BrandLogo />
          <span className="nav-logo-text">White <strong>Academy</strong></span>
        </Link>

        <div className={`nav-links ${mobileOpen ? 'open' : ''}`}>
          <a href="#paths" onClick={() => setMobileOpen(false)}>المسارات التعليمية</a>
          <a href="#features" onClick={() => setMobileOpen(false)}>المميزات</a>
          <a href="#testimonials" onClick={() => setMobileOpen(false)}>آراء الطلاب</a>
          <div className="nav-cta-mobile">
            <Link to="/auth" className="nav-btn-primary" onClick={() => setMobileOpen(false)}>ابدأ الآن</Link>
          </div>
        </div>

        <div className="nav-actions">
          <Link to="/auth" className="nav-btn-outline">تسجيل الدخول</Link>
          <Link to="/auth" className="nav-btn-primary">ابدأ مجاناً</Link>
        </div>

        <button className="nav-mobile-toggle" onClick={() => setMobileOpen(!mobileOpen)}>
          {mobileOpen ? <X size={24} /> : <Menu size={24} />}
        </button>
      </div>
    </nav>
  )
}

// Welcome Banner (compact replacement for hero)
function WelcomeBanner() {
  return (
    <section className="welcome-banner" dir="rtl">
      <div className="section-inner">
        <div className="welcome-grid">
          <div className="welcome-main">
            <div className="welcome-tag">
              <Flame size={14} />
              <span>أكثر من 10,000 متعلم من جميع أنحاء العالم</span>
            </div>
            <h1>طوّر مهاراتك، <span className="text-gradient">واصنع مستقبلك</span></h1>
            <p>منصتك الشاملة لتعلم المهارات الحديثة في الإدارة، التصميم، التسويق، والتقنية. مسارات عملية بشهادات معتمدة.</p>
            <div className="welcome-actions">
              <Link to="/auth" className="btn-primary-lg">
                <span>ابدأ التعلم مجاناً</span>
                <ArrowLeft size={16} />
              </Link>
              <a href="#paths" className="btn-ghost-lg">
                <Search size={15} />
                <span>تصفح المسارات</span>
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
                <div className="mini-stat-label">مسار تعليمي</div>
              </div>
            </div>
            <div className="mini-stat-card">
              <div className="mini-stat-icon" style={{ color: '#3b82f6', background: 'rgba(59,130,246,0.1)' }}>
                <Users size={20} />
              </div>
              <div>
                <div className="mini-stat-value">10k+</div>
                <div className="mini-stat-label">طالب مسجّل</div>
              </div>
            </div>
            <div className="mini-stat-card">
              <div className="mini-stat-icon" style={{ color: '#f59e0b', background: 'rgba(245,158,11,0.1)' }}>
                <Star size={20} />
              </div>
              <div>
                <div className="mini-stat-value">4.9</div>
                <div className="mini-stat-label">تقييم الطلاب</div>
              </div>
            </div>
            <div className="mini-stat-card">
              <div className="mini-stat-icon" style={{ color: '#a855f7', background: 'rgba(168,85,247,0.1)' }}>
                <Award size={20} />
              </div>
              <div>
                <div className="mini-stat-value">15k+</div>
                <div className="mini-stat-label">شهادة مُصدرة</div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  )
}

// Learning Paths (Replacing Courses and specific tech content)
const PATHS = [
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

function PathsSection() {
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

// Features
const FEATURES = [
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

function FeaturesSection() {
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

// Testimonials
const TESTIMONIALS = [
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

function TestimonialsSection() {
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

// CTA
function CTASection() {
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

// Footer
function Footer() {
  return (
    <footer className="site-footer" dir="rtl">
      <div className="section-inner">
        <div className="footer-grid">
          <div className="footer-brand">
            <div className="footer-logo">
              <BrandLogo />
              <span className="nav-logo-text">White <strong>Academy</strong></span>
            </div>
            <p>منصة تعليمية رائدة تهدف إلى تمكين الأفراد بالمهارات والمعارف اللازمة للنجاح في العصر الحديث.</p>
          </div>
          <div className="footer-links-group">
            <h4>المنصة</h4>
            <a href="#paths">المسارات</a>
            <a href="#features">المميزات</a>
            <a href="#">المدونة</a>
            <a href="#">عن الأكاديمية</a>
          </div>
          <div className="footer-links-group">
            <h4>الدعم</h4>
            <a href="#">مركز المساعدة</a>
            <a href="#">الأسئلة الشائعة</a>
            <a href="#">تواصل معنا</a>
            <a href="#">الشروط والأحكام</a>
          </div>
          <div className="footer-links-group">
            <h4>تواصل معنا</h4>
            <a href="#">hello@whiteacademy.com</a>
            <a href="#">Twitter / X</a>
            <a href="#">LinkedIn</a>
            <a href="#">Instagram</a>
          </div>
        </div>
        <div className="footer-bottom">
          <p>&copy; 2026 White Academy. جميع الحقوق محفوظة.</p>
        </div>
      </div>
    </footer>
  )
}

// Main Page
export default function HomePage() {
  return (
    <div className="home-page">
      <Navbar />
      <WelcomeBanner />
      <PathsSection />
      <FeaturesSection />
      <TestimonialsSection />
      <CTASection />
      <Footer />
    </div>
  )
}
