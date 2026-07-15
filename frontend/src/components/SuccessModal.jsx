import { useEffect } from 'react'
import { CheckCircle } from 'lucide-react'

export default function SuccessModal({ show, title, message, onClose }) {
  useEffect(() => {
    const handleEscape = (e) => {
      if (e.key === 'Escape') onClose()
    }
    if (show) {
      document.addEventListener('keydown', handleEscape)
    }
    return () => document.removeEventListener('keydown', handleEscape)
  }, [show, onClose])

  const handleOverlayClick = (e) => {
    if (e.target === e.currentTarget) onClose()
  }

  return (
    <div className={`modal-overlay ${show ? 'active' : ''}`} onClick={handleOverlayClick}>
      <div className="modal-content">
        <div className="success-animation">
          {show && (
            <svg className="checkmark-svg" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 52 52">
              <circle className="checkmark-circle" cx="26" cy="26" r="25" fill="none" />
              <path className="checkmark-check" fill="none" d="M14.1 27.2l7.1 7.2 16.7-16.8" />
            </svg>
          )}
        </div>
        <h3>{title}</h3>
        <p>{message}</p>
        <button className="modal-btn" onClick={onClose}>Continue</button>
      </div>
    </div>
  )
}
