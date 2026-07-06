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
