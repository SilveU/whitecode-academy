import { Link } from 'react-router-dom'
import { Home, ArrowLeft } from 'lucide-react'
import BackgroundAnimation from '../components/BackgroundAnimation'

export default function NotFoundPage() {
  return (
    <>
      <BackgroundAnimation />
      <div style={{
        position: 'relative',
        zIndex: 1,
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        minHeight: '100vh',
        textAlign: 'center',
        padding: '20px',
        direction: 'ltr',
      }}>
        <div style={{
          fontSize: '8rem',
          fontWeight: 900,
          lineHeight: 1,
          background: 'linear-gradient(135deg, var(--green-400), var(--green-600))',
          WebkitBackgroundClip: 'text',
          WebkitTextFillColor: 'transparent',
          marginBottom: '16px',
        }}>
          404
        </div>
        <h1 style={{ fontSize: '1.8rem', fontWeight: 800, color: 'var(--text-primary)', marginBottom: '12px' }}>
          Page Not Found
        </h1>
        <p style={{ fontSize: '1rem', color: 'var(--text-muted)', marginBottom: '32px', maxWidth: '400px', lineHeight: 1.7 }}>
          The page you are looking for does not exist or has been moved. You can return to the home page.
        </p>
        <div style={{ display: 'flex', gap: '12px', flexWrap: 'wrap', justifyContent: 'center' }}>
          <Link
            to="/"
            style={{
              display: 'inline-flex',
              alignItems: 'center',
              gap: '8px',
              padding: '12px 24px',
              background: 'linear-gradient(135deg, var(--green-600), var(--green-500))',
              border: 'none',
              borderRadius: '12px',
              color: 'white',
              fontFamily: "'Cairo', sans-serif",
              fontSize: '0.9rem',
              fontWeight: 600,
              textDecoration: 'none',
              transition: 'all 0.3s ease',
            }}
          >
            <Home size={16} />
            Home Page
          </Link>
          <Link
            to="/auth?tab=login"
            style={{
              display: 'inline-flex',
              alignItems: 'center',
              gap: '8px',
              padding: '12px 24px',
              background: 'rgba(255, 255, 255, 0.04)',
              border: '1px solid rgba(255, 255, 255, 0.1)',
              borderRadius: '12px',
              color: 'var(--text-secondary)',
              fontFamily: "'Cairo', sans-serif",
              fontSize: '0.9rem',
              fontWeight: 500,
              textDecoration: 'none',
              transition: 'all 0.3s ease',
            }}
          >
            <ArrowLeft size={16} />
            Log In
          </Link>
        </div>
      </div>
    </>
  )
}
