import { useNavigate } from 'react-router-dom'
import { ArrowLeft } from 'lucide-react'
import Navbar from './Navbar'
import Footer from './Footer'
import usePageTitle from '../../hooks/usePageTitle'
import './PageLayout.css'

export default function PageLayout({ children, title, subtitle }) {
  usePageTitle(title)
  const navigate = useNavigate()

  return (
    <div className="page-layout">
      <Navbar />
      
      <div className="page-header" dir="ltr">
        <div className="section-inner">
          <button onClick={() => navigate(-1)} className="btn-back">
            <ArrowLeft size={18} />
            <span>Back</span>
          </button>
          
          <div className="page-title-content">
            <h1>{title}</h1>
            {subtitle && <p>{subtitle}</p>}
          </div>
        </div>
      </div>

      <main className="page-content" dir="ltr">
        <div className="section-inner">
          {children}
        </div>
      </main>

      <Footer />
    </div>
  )
}
