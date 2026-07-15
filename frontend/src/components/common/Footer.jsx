import { Link } from 'react-router-dom'
import BrandLogo from './BrandLogo'

export default function Footer() {
  return (
    <footer className="site-footer" dir="ltr">
      <div className="section-inner">
        <div className="footer-grid">
          <div className="footer-brand">
            <div className="footer-logo">
              <BrandLogo />
              <span className="nav-logo-text">White <strong>Academy</strong></span>
            </div>
            <p>A leading educational platform empowering individuals with the modern technical skills needed to thrive in the tech industry.</p>
            <div style={{ marginTop: '16px', padding: '12px', background: 'rgba(255,255,255,0.02)', borderRadius: '8px', border: '1px solid var(--border)' }}>
              <p style={{ fontSize: '0.75rem', color: 'var(--text-muted)', margin: 0, lineHeight: 1.6 }}>
                <strong>Disclaimer:</strong> The courses and content provided on this platform are curated and localized for maximum practical benefit with interactive quizzes and real-world assignments. Original copyrights belong to their respective creators.
              </p>
            </div>
          </div>
          <div className="footer-links-group">
            <h4>Platform</h4>
            <Link to="/paths">Learning Paths</Link>
            <Link to="/about">Features</Link>
            <Link to="/blog">Blog</Link>
            <Link to="/about">About Us</Link>
          </div>
          <div className="footer-links-group">
            <h4>Support</h4>
            <Link to="/help">Help Center</Link>
            <Link to="/faq">FAQ</Link>
            <Link to="/contact">Contact Us</Link>
            <Link to="/terms">Terms & Conditions</Link>
          </div>
          <div className="footer-links-group">
            <h4>Contact Us</h4>
            <a href="mailto:hello@whiteacademy.com">hello@whiteacademy.com</a>
            <a href="https://x.com" target="_blank" rel="noreferrer">Twitter / X</a>
            <a href="https://linkedin.com" target="_blank" rel="noreferrer">LinkedIn</a>
            <a href="https://instagram.com" target="_blank" rel="noreferrer">Instagram</a>
          </div>
        </div>
        <div className="footer-bottom">
          <p>&copy; 2026 White Academy. All rights reserved.</p>
        </div>
      </div>
    </footer>
  )
}
