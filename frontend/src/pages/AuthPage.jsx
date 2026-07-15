import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import BackgroundAnimation from '../components/BackgroundAnimation'
import BrandingSide from '../components/BrandingSide'
import AuthForm from '../components/AuthForm'
import SuccessModal from '../components/SuccessModal'
import usePageTitle from '../hooks/usePageTitle'

export default function AuthPage() {
  usePageTitle('تسجيل الدخول')
  const [activeTab, setActiveTab] = useState('login')
  const [modal, setModal] = useState({ show: false, title: '', message: '' })
  const navigate = useNavigate()

  const showModal = (title, message) => {
    setModal({ show: true, title, message })
  }

  const closeModal = () => {
    setModal({ show: false, title: '', message: '' })
    navigate('/')
  }

  return (
    <>
      <BackgroundAnimation />
      <div className="container" dir="rtl">
        <button 
          onClick={() => navigate('/')} 
          style={{ 
            position: 'absolute', 
            top: '40px', 
            right: '40px', 
            zIndex: 10,
            display: 'flex',
            alignItems: 'center',
            gap: '8px',
            background: 'rgba(255, 255, 255, 0.03)',
            border: '1px solid rgba(255, 255, 255, 0.1)',
            color: 'var(--text-secondary)',
            padding: '10px 16px',
            borderRadius: '12px',
            cursor: 'pointer',
            fontFamily: '"Cairo", sans-serif',
            fontSize: '0.9rem',
            fontWeight: '600',
            transition: 'all 0.3s ease',
            backdropFilter: 'blur(10px)',
            WebkitBackdropFilter: 'blur(10px)',
          }}
          onMouseEnter={(e) => {
            e.currentTarget.style.background = 'rgba(255, 255, 255, 0.08)';
            e.currentTarget.style.color = 'var(--text-primary)';
            e.currentTarget.style.transform = 'translateY(-2px)';
            e.currentTarget.style.borderColor = 'rgba(255, 255, 255, 0.2)';
          }}
          onMouseLeave={(e) => {
            e.currentTarget.style.background = 'rgba(255, 255, 255, 0.03)';
            e.currentTarget.style.color = 'var(--text-secondary)';
            e.currentTarget.style.transform = 'translateY(0)';
            e.currentTarget.style.borderColor = 'rgba(255, 255, 255, 0.1)';
          }}
        >
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <path d="M5 12h14M12 5l7 7-7 7"/>
          </svg>
          <span>الرئيسية</span>
        </button>
        <BrandingSide />
        <div className="form-side">
          <AuthForm
            activeTab={activeTab}
            setActiveTab={setActiveTab}
            onSuccess={showModal}
          />
        </div>
      </div>
      <SuccessModal
        show={modal.show}
        title={modal.title}
        message={modal.message}
        onClose={closeModal}
      />
    </>
  )
}
