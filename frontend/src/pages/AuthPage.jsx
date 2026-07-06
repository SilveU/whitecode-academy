import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import BackgroundAnimation from '../components/BackgroundAnimation'
import BrandingSide from '../components/BrandingSide'
import AuthForm from '../components/AuthForm'
import SuccessModal from '../components/SuccessModal'

export default function AuthPage() {
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
          className="btn-back"
          style={{ position: 'absolute', top: '30px', right: '30px', zIndex: 10 }}
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
