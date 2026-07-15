import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom'
import './App.css'
import { AuthProvider, useAuth } from './context/AuthContext'
import AuthPage from './pages/AuthPage'
import HomePage from './pages/HomePage'
import BlogPage from './pages/BlogPage'
import AboutPage from './pages/AboutPage'
import HelpPage from './pages/HelpPage'
import FAQPage from './pages/FAQPage'
import ContactPage from './pages/ContactPage'
import TermsPage from './pages/TermsPage'
import PathsPage from './pages/PathsPage'
import VerifyEmailPage from './pages/VerifyEmailPage'
import PendingVerificationPage from './pages/PendingVerificationPage'
import ScrollToTopButton from './components/common/ScrollToTopButton'
import NotFoundPage from './pages/NotFoundPage'

// Dashboard
import DashboardLayout from './components/dashboard/DashboardLayout'
import DashboardHome from './pages/dashboard/DashboardHome'
import CoursesPage from './pages/dashboard/CoursesPage'
import DepartmentsPage from './pages/dashboard/DepartmentsPage'
import SectionsPage from './pages/dashboard/SectionsPage'
import InstructorsPage from './pages/dashboard/InstructorsPage'
import StudentsPage from './pages/dashboard/StudentsPage'
import EnrollmentsPage from './pages/dashboard/EnrollmentsPage'

// Protected Route wrapper
function ProtectedRoute({ children }) {
  const { isAuthenticated } = useAuth()
  if (!isAuthenticated) return <Navigate to="/auth" replace />
  return children
}

// Redirect if already logged in
function GuestRoute({ children }) {
  const { isAuthenticated } = useAuth()
  if (isAuthenticated) return <Navigate to="/dashboard" replace />
  return children
}

function AppRoutes() {
  return (
    <Routes>
      <Route path="/" element={<HomePage />} />
      <Route path="/auth" element={<GuestRoute><AuthPage /></GuestRoute>} />
      <Route path="/api/authentication/confirm-email" element={<VerifyEmailPage />} />
      <Route path="/pending-verification" element={<PendingVerificationPage />} />
      <Route path="/paths" element={<PathsPage />} />
      <Route path="/blog" element={<BlogPage />} />
      <Route path="/about" element={<AboutPage />} />
      <Route path="/help" element={<HelpPage />} />
      <Route path="/faq" element={<FAQPage />} />
      <Route path="/contact" element={<ContactPage />} />
      <Route path="/terms" element={<TermsPage />} />

      {/* Dashboard Routes */}
      <Route
        path="/dashboard"
        element={
          <ProtectedRoute>
            <DashboardLayout />
          </ProtectedRoute>
        }
      >
        <Route index element={<DashboardHome />} />
        <Route path="courses" element={<CoursesPage />} />
        <Route path="departments" element={<DepartmentsPage />} />
        <Route path="sections" element={<SectionsPage />} />
        <Route path="instructors" element={<InstructorsPage />} />
        <Route path="students" element={<StudentsPage />} />
        <Route path="enrollments" element={<EnrollmentsPage />} />
      </Route>

      {/* 404 Catch-All */}
      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  )
}

function App() {
  return (
    <Router>
      <AuthProvider>
        <AppRoutes />
        <ScrollToTopButton />
      </AuthProvider>
    </Router>
  )
}

export default App
