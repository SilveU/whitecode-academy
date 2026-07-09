import { useState } from 'react'
import { Link, NavLink, Outlet, useNavigate } from 'react-router-dom'
import {
  LayoutDashboard,
  BookOpen,
  FolderTree,
  Layers,
  Users,
  GraduationCap,
  ClipboardList,
  LogOut,
  Menu,
  X,
  Home,
} from 'lucide-react'
import { useAuth } from '../../context/AuthContext'
import BrandLogo from '../common/BrandLogo'
import './DashboardLayout.css'

const NAV_ITEMS = [
  { to: '/dashboard', icon: LayoutDashboard, label: 'الرئيسية', end: true, roles: ['Admin', 'Instructor', 'User'] },
  { section: 'إدارة المحتوى' },
  { to: '/dashboard/courses', icon: BookOpen, label: 'الكورسات', roles: ['Admin', 'Instructor', 'User'] },
  { to: '/dashboard/departments', icon: FolderTree, label: 'الأقسام', roles: ['Admin'] },
  { to: '/dashboard/sections', icon: Layers, label: 'السكشنات', roles: ['Admin', 'Instructor'] },
  { section: 'إدارة المستخدمين' },
  { to: '/dashboard/instructors', icon: GraduationCap, label: 'المدربين', roles: ['Admin'] },
  { to: '/dashboard/students', icon: Users, label: 'الطلاب', roles: ['Admin', 'User'] },
  { to: '/dashboard/enrollments', icon: ClipboardList, label: 'التسجيلات', roles: ['Admin', 'Instructor', 'User'] },
]

export default function DashboardLayout() {
  const { user, roles, logout } = useAuth()
  const navigate = useNavigate()
  const [sidebarOpen, setSidebarOpen] = useState(false)

  const handleLogout = async () => {
    await logout()
    navigate('/')
  }

  const userInitials = user
    ? `${(user.firstName?.[0] || '')}${(user.lastName?.[0] || '')}`
    : '?'

  const userRoleLabel = roles.includes('Admin')
    ? 'مسؤول'
    : roles.includes('Instructor')
    ? 'مدرب'
    : 'طالب'

  // Filter nav items based on user roles
  const filteredItems = NAV_ITEMS.filter((item) => {
    if (item.section) return true
    return item.roles?.some((r) => roles.includes(r))
  })

  // Remove section labels that have no items after them
  const cleanedItems = filteredItems.filter((item, idx) => {
    if (!item.section) return true
    // Check if there's at least one non-section item after this one
    for (let i = idx + 1; i < filteredItems.length; i++) {
      if (!filteredItems[i].section) return true
      if (filteredItems[i].section) return false
    }
    return false
  })

  return (
    <div className="dashboard-wrapper">
      {/* Sidebar overlay for mobile */}
      <div
        className={`sidebar-overlay ${sidebarOpen ? 'open' : ''}`}
        onClick={() => setSidebarOpen(false)}
      />

      {/* Sidebar */}
      <aside className={`dashboard-sidebar ${sidebarOpen ? 'open' : ''}`}>
        <div className="sidebar-header">
          <Link to="/" className="sidebar-logo">
            <BrandLogo className="sidebar-logo-icon" />
            <span className="sidebar-logo-text">
              White <strong>Academy</strong>
            </span>
          </Link>
        </div>

        <nav className="sidebar-nav">
          {cleanedItems.map((item, i) =>
            item.section ? (
              <div key={`section-${i}`} className="sidebar-section-label">
                {item.section}
              </div>
            ) : (
              <NavLink
                key={item.to}
                to={item.to}
                end={item.end}
                className={({ isActive }) =>
                  `sidebar-link ${isActive ? 'active' : ''}`
                }
                onClick={() => setSidebarOpen(false)}
              >
                <item.icon size={18} className="sidebar-link-icon" />
                <span>{item.label}</span>
              </NavLink>
            )
          )}
        </nav>

        <div className="sidebar-footer">
          <div className="sidebar-user">
            <div className="sidebar-avatar">{userInitials}</div>
            <div className="sidebar-user-info">
              <div className="sidebar-user-name">
                {user?.firstName} {user?.lastName}
              </div>
              <div className="sidebar-user-role">{userRoleLabel}</div>
            </div>
            <button
              className="sidebar-logout-btn"
              onClick={handleLogout}
              title="تسجيل الخروج"
            >
              <LogOut size={16} />
            </button>
          </div>
        </div>
      </aside>

      {/* Main */}
      <div className="dashboard-main">
        <header className="dashboard-topbar">
          <div className="topbar-right">
            <button
              className="topbar-mobile-toggle"
              onClick={() => setSidebarOpen(!sidebarOpen)}
            >
              {sidebarOpen ? <X size={22} /> : <Menu size={22} />}
            </button>
            <span className="topbar-title">لوحة التحكم</span>
          </div>
          <div className="topbar-left">
            <Link to="/" className="topbar-home-link">
              <Home size={16} />
              <span>الصفحة الرئيسية</span>
            </Link>
          </div>
        </header>

        <div className="dashboard-content">
          <Outlet />
        </div>
      </div>
    </div>
  )
}
