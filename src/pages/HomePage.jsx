import { useState, useEffect } from 'react'
import { Link } from 'react-router-dom'
import {
  Code2,
  Users,
  Award,
  BookOpen,
  Monitor,
  Smartphone,
  Globe,
  Database,
  Layers,
  ArrowLeft,
  Play,
  Star,
  Clock,
  BarChart3,
  Zap,
  Target,
  CheckCircle2,
  Menu,
  X,
  ArrowUpRight,
  GraduationCap,
  MessageSquare,
  FileCode2,
  Cpu,
  Shield,
  Search,
  TrendingUp,
  Flame,
  Sparkles
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
          <div className="nav-logo-icon">
            <svg viewBox="0 0 60 60" fill="none">
              <rect x="2" y="2" width="56" height="56" rx="14" stroke="currentColor" strokeWidth="3" />
              <text x="10" y="40" fontFamily="monospace" fontSize="28" fontWeight="bold" fill="currentColor">&lt;/&gt;</text>
            </svg>
          </div>
          <span className="nav-logo-text">White Code <strong>Academy</strong></span>
        </Link>

        <div className={`nav-links ${mobileOpen ? 'open' : ''}`}>
          <a href="#courses" onClick={() => setMobileOpen(false)}>الدورات</a>
          <a href="#paths" onClick={() => setMobileOpen(false)}>المسارات</a>
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
              <span>أكثر من 5,000 طالب يتعلمون معنا</span>
            </div>
            <h1>تعلّم البرمجة، <span className="text-gradient">ابنِ مستقبلك</span></h1>
            <p>دورات عملية في تطوير الويب، الموبايل، والأمن السيبراني مع مشاريع حقيقية وشهادات معتمدة.</p>
            <div className="welcome-actions">
              <Link to="/auth" className="btn-primary-lg">
                <span>ابدأ التعلم مجاناً</span>
                <ArrowLeft size={16} />
              </Link>
              <a href="#courses" className="btn-ghost-lg">
                <Search size={15} />
                <span>تصفح الدورات</span>
              </a>
            </div>
          </div>
          <div className="welcome-stats-cards">
            <div className="mini-stat-card">
              <div className="mini-stat-icon" style={{ color: '#22c55e', background: 'rgba(34,197,94,0.1)' }}>
                <BookOpen size={20} />
              </div>
              <div>
                <div className="mini-stat-value">48</div>
                <div className="mini-stat-label">دورة تعليمية</div>
              </div>
            </div>
            <div className="mini-stat-card">
              <div className="mini-stat-icon" style={{ color: '#3b82f6', background: 'rgba(59,130,246,0.1)' }}>
                <Users size={20} />
              </div>
              <div>
                <div className="mini-stat-value">5,200+</div>
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
                <Code2 size={20} />
              </div>
              <div>
                <div className="mini-stat-value">200+</div>
                <div className="mini-stat-label">مشروع عملي</div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  )
}

// Trending / Popular Courses Strip
const POPULAR_TAGS = [
  { label: 'React', color: '#61dafb' },
  { label: 'Python', color: '#3776ab' },
  { label: 'Node.js', color: '#68a063' },
  { label: 'Flutter', color: '#02569b' },
  { label: 'SQL', color: '#e48e00' },
  { label: 'Docker', color: '#2496ed' },
  { label: 'JavaScript', color: '#f7df1e' },
  { label: 'TypeScript', color: '#3178c6' },
]

function TechStrip() {
  return (
    <section className="tech-strip" dir="rtl">
      <div className="section-inner">
        <div className="tech-strip-inner">
          <div className="tech-strip-label">
            <TrendingUp size={15} />
            <span>الأكثر طلباً</span>
          </div>
          <div className="tech-tags">
            {POPULAR_TAGS.map((tag, i) => (
              <span className="tech-tag" key={i} style={{ borderColor: `${tag.color}40`, color: tag.color }}>
                {tag.label}
              </span>
            ))}
          </div>
        </div>
      </div>
    </section>
  )
}

// Courses
const COURSES = [
  {
    icon: <Globe size={24} />,
    title: 'تطوير الويب الكامل',
    desc: 'HTML, CSS, JavaScript, React, Node.js',
    level: 'مبتدئ - متقدم',
    duration: '60 ساعة',
    students: '2,100',
    color: '#22c55e',
    featured: true
  },
  {
    icon: <Smartphone size={24} />,
    title: 'تطوير تطبيقات الموبايل',
    desc: 'React Native, Flutter, Swift',
    level: 'متوسط',
    duration: '45 ساعة',
    students: '1,300',
    color: '#3b82f6',
    featured: true
  },
  {
    icon: <Database size={24} />,
    title: 'قواعد البيانات والباك إند',
    desc: 'SQL, MongoDB, PostgreSQL, APIs',
    level: 'متوسط - متقدم',
    duration: '40 ساعة',
    students: '980',
    color: '#a855f7',
    featured: false
  },
  {
    icon: <Cpu size={24} />,
    title: 'Python والتحليل البرمجي',
    desc: 'Python, Data Analysis, Automation',
    level: 'مبتدئ',
    duration: '35 ساعة',
    students: '1,850',
    color: '#f59e0b',
    featured: true
  },
  {
    icon: <Shield size={24} />,
    title: 'الأمن السيبراني',
    desc: 'Ethical Hacking, Pentesting, Security',
    level: 'متقدم',
    duration: '50 ساعة',
    students: '720',
    color: '#ef4444',
    featured: false
  },
  {
    icon: <Layers size={24} />,
    title: 'DevOps وإدارة السيرفرات',
    desc: 'Docker, Kubernetes, CI/CD, AWS',
    level: 'متقدم',
    duration: '38 ساعة',
    students: '650',
    color: '#06b6d4',
    featured: false
  },
]

function CoursesSection() {
  return (
    <section className="courses-section" id="courses" dir="rtl">
      <div className="section-inner">
        <div className="section-header">
          <span className="section-tag"><BookOpen size={14} /> الدورات التعليمية</span>
          <h2>اختر مسارك التعليمي</h2>
          <p>مسارات تعليمية مصمّمة بعناية لتأخذك من المبتدئ إلى المحترف</p>
        </div>
        <div className="courses-grid">
          {COURSES.map((course, i) => (
            <div className={`course-card ${course.featured ? 'featured' : ''}`} key={i}>
              {course.featured && (
                <div className="course-badge">
                  <Sparkles size={11} />
                  <span>مميّز</span>
                </div>
              )}
              <div className="course-card-icon" style={{ color: course.color, background: `${course.color}15` }}>
                {course.icon}
              </div>
              <h3>{course.title}</h3>
              <p className="course-card-desc">{course.desc}</p>
              <div className="course-card-meta">
                <span><Clock size={13} /> {course.duration}</span>
                <span><BarChart3 size={13} /> {course.level}</span>
              </div>
              <div className="course-card-footer">
                <span className="course-students"><Users size={13} /> {course.students} طالب</span>
                <Link to="/auth" className="course-card-link">
                  سجّل الآن <ArrowUpRight size={14} />
                </Link>
              </div>
            </div>
          ))}
        </div>
      </div>
    </section>
  )
}

// Learning Paths
const PATHS = [
  {
    title: 'مطور واجهات أمامية',
    techs: 'HTML → CSS → JS → React',
    duration: '6 أشهر',
    icon: <Monitor size={22} />,
    color: '#22c55e'
  },
  {
    title: 'مطور Full-Stack',
    techs: 'Frontend → Backend → Database → Deploy',
    duration: '9 أشهر',
    icon: <Layers size={22} />,
    color: '#3b82f6'
  },
  {
    title: 'مطور موبايل',
    techs: 'Dart → Flutter → APIs → Publishing',
    duration: '5 أشهر',
    icon: <Smartphone size={22} />,
    color: '#a855f7'
  },
]

function PathsSection() {
  return (
    <section className="paths-section" id="paths" dir="rtl">
      <div className="section-inner">
        <div className="section-header">
          <span className="section-tag"><Target size={14} /> المسارات التعليمية</span>
          <h2>اختر مسارك المهني</h2>
          <p>خطط تعليمية مرتبة خطوة بخطوة توصلك لهدفك</p>
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
              <div className="path-techs">{path.techs}</div>
              <Link to="/auth" className="path-link">
                ابدأ المسار <ArrowLeft size={14} />
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
    icon: <Monitor size={24} />,
    title: 'تعلّم عملي',
    desc: 'طبّق ما تتعلمه من خلال مشاريع حقيقية ومحاكاة لبيئات العمل الفعلية.'
  },
  {
    icon: <GraduationCap size={24} />,
    title: 'شهادات معتمدة',
    desc: 'احصل على شهادة إتمام لكل دورة يمكنك مشاركتها في سيرتك الذاتية.'
  },
  {
    icon: <MessageSquare size={24} />,
    title: 'دعم مباشر',
    desc: 'فريق دعم فني متخصص يساعدك في حل أي مشكلة خلال رحلة التعلم.'
  },
  {
    icon: <FileCode2 size={24} />,
    title: 'مشاريع تطبيقية',
    desc: 'أكثر من 200 مشروع عملي جاهز للتطبيق في مختلف المجالات.'
  },
  {
    icon: <Target size={24} />,
    title: 'مسارات واضحة',
    desc: 'خطة تعليمية مرتبة خطوة بخطوة توصلك لهدفك المهني.'
  },
  {
    icon: <Users size={24} />,
    title: 'مجتمع نشط',
    desc: 'انضم لمجتمع يضم آلاف المطورين العرب للتعاون وتبادل الخبرات.'
  },
]

function FeaturesSection() {
  return (
    <section className="features-section" id="features" dir="rtl">
      <div className="section-inner">
        <div className="section-header">
          <span className="section-tag"><Zap size={14} /> لماذا نحن</span>
          <h2>ما يميّز White Code Academy</h2>
          <p>بيئة تعليمية متكاملة مبنية على أفضل الممارسات في التعليم التقني</p>
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
    name: 'أحمد عبدالله',
    role: 'مطور واجهات أمامية',
    text: 'بفضل White Code Academy قدرت أتعلم React واشتغلت في شركة تقنية خلال 6 أشهر. المحتوى عملي ومباشر.',
    rating: 5
  },
  {
    name: 'سارة محمد',
    role: 'مطورة تطبيقات موبايل',
    text: 'الدورات منظمة بشكل ممتاز والمشاريع التطبيقية ساعدتني أبني Portfolio قوي. الدعم الفني سريع ومحترف.',
    rating: 5
  },
  {
    name: 'خالد العمري',
    role: 'مهندس DevOps',
    text: 'من أفضل المنصات العربية لتعلم البرمجة. المحتوى محدّث والمجتمع نشط جداً ويساعد في حل المشاكل.',
    rating: 5
  },
]

function TestimonialsSection() {
  return (
    <section className="testimonials-section" id="testimonials" dir="rtl">
      <div className="section-inner">
        <div className="section-header">
          <span className="section-tag"><MessageSquare size={14} /> آراء الطلاب</span>
          <h2>ماذا يقول طلابنا</h2>
          <p>تجارب حقيقية من طلاب غيّرت مساراتهم المهنية</p>
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
            <Code2 size={32} />
          </div>
          <h2>جاهز تبدأ رحلتك؟</h2>
          <p>سجّل الآن مجاناً وابدأ أول دورة لك اليوم. لا تحتاج خبرة سابقة.</p>
          <div className="cta-actions">
            <Link to="/auth" className="btn-primary-lg">
              <span>أنشئ حسابك المجاني</span>
              <ArrowLeft size={18} />
            </Link>
          </div>
          <div className="cta-features">
            <span><CheckCircle2 size={15} /> بدون بطاقة ائتمان</span>
            <span><CheckCircle2 size={15} /> دورات مجانية متاحة</span>
            <span><CheckCircle2 size={15} /> إلغاء في أي وقت</span>
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
              <div className="nav-logo-icon">
                <svg viewBox="0 0 60 60" fill="none">
                  <rect x="2" y="2" width="56" height="56" rx="14" stroke="currentColor" strokeWidth="3" />
                  <text x="10" y="40" fontFamily="monospace" fontSize="28" fontWeight="bold" fill="currentColor">&lt;/&gt;</text>
                </svg>
              </div>
              <span className="nav-logo-text">White Code <strong>Academy</strong></span>
            </div>
            <p>منصة عربية متخصصة في تعليم البرمجة وتطوير المهارات التقنية بمحتوى عملي ومحدّث.</p>
          </div>
          <div className="footer-links-group">
            <h4>المنصة</h4>
            <a href="#courses">الدورات</a>
            <a href="#features">المميزات</a>
            <a href="#paths">المسارات التعليمية</a>
            <a href="#">الأسعار</a>
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
            <a href="#">info@whitecodeacademy.com</a>
            <a href="#">Twitter / X</a>
            <a href="#">LinkedIn</a>
            <a href="#">YouTube</a>
          </div>
        </div>
        <div className="footer-bottom">
          <p>&copy; 2026 White Code Academy. جميع الحقوق محفوظة.</p>
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
      <TechStrip />
      <CoursesSection />
      <PathsSection />
      <FeaturesSection />
      <TestimonialsSection />
      <CTASection />
      <Footer />
    </div>
  )
}
