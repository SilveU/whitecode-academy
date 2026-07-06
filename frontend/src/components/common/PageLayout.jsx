import { useNavigate } from 'react-router-dom'
import { ArrowRight } from 'lucide-react'
import Navbar from './Navbar'
import Footer from './Footer'

export default function PageLayout({ children, title, subtitle }) {
  const navigate = useNavigate()

  return (
    <div className="page-layout">
      <Navbar />
      
      <div className="page-header" dir="rtl">
        <div className="section-inner">
          <button onClick={() => navigate(-1)} className="btn-back">
            <ArrowRight size={18} />
            <span>رجوع</span>
          </button>
          
          <div className="page-title-content">
            <h1>{title}</h1>
            {subtitle && <p>{subtitle}</p>}
          </div>
        </div>
      </div>

      <main className="page-content" dir="rtl">
        <div className="section-inner">
          {children}
        </div>
      </main>

      <Footer />
    </div>
  )
}
