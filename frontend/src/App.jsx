import { BrowserRouter as Router, Routes, Route } from 'react-router-dom'
import './App.css'
import AuthPage from './pages/AuthPage'
import HomePage from './pages/HomePage'
import BlogPage from './pages/BlogPage'
import AboutPage from './pages/AboutPage'
import HelpPage from './pages/HelpPage'
import FAQPage from './pages/FAQPage'
import ContactPage from './pages/ContactPage'
import TermsPage from './pages/TermsPage'
import PathsPage from './pages/PathsPage'

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/auth" element={<AuthPage />} />
        <Route path="/paths" element={<PathsPage />} />
        <Route path="/blog" element={<BlogPage />} />
        <Route path="/about" element={<AboutPage />} />
        <Route path="/help" element={<HelpPage />} />
        <Route path="/faq" element={<FAQPage />} />
        <Route path="/contact" element={<ContactPage />} />
        <Route path="/terms" element={<TermsPage />} />
      </Routes>
    </Router>
  )
}

export default App
