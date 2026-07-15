import { useState, useEffect } from 'react'
import { BookOpen, FolderTree, Users, ClipboardList, TrendingUp, Sparkles } from 'lucide-react'
import { useAuth } from '../../context/AuthContext'
import { courseApi, departmentApi } from '../../services/api'
import usePageTitle from '../../hooks/usePageTitle'

export default function DashboardHome() {
  usePageTitle('Dashboard')
  const { user, isAdmin, isInstructor } = useAuth()
  const [stats, setStats] = useState({ courses: '—', departments: '—' })
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    async function loadStats() {
      try {
        const [coursesRes, deptsRes] = await Promise.allSettled([
          courseApi.list({ pageNumber: 1, pageSize: 1 }),
          isAdmin ? departmentApi.list({ pageNumber: 1, pageSize: 1 }) : Promise.resolve(null),
        ])

        const courseCount = coursesRes.status === 'fulfilled' && coursesRes.value?.ok
          ? (coursesRes.value.data?.totalCount ?? coursesRes.value.data?.length ?? '—')
          : '—'

        const deptCount = deptsRes.status === 'fulfilled' && deptsRes.value?.ok
          ? (deptsRes.value.data?.totalCount ?? deptsRes.value.data?.length ?? '—')
          : '—'

        setStats({ courses: courseCount, departments: deptCount })
      } catch {
        // ignore
      } finally {
        setLoading(false)
      }
    }
    loadStats()
  }, [isAdmin])

  const greeting = () => {
    const hour = new Date().getHours()
    if (hour < 12) return 'Good Morning'
    if (hour < 17) return 'Good Afternoon'
    return 'Good Evening'
  }

  return (
    <div>
      {/* Welcome */}
      <div style={{ marginBottom: '32px' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '6px' }}>
          <Sparkles size={20} style={{ color: 'var(--green-400)' }} />
          <h1 style={{ fontSize: '1.5rem', fontWeight: 800, color: 'var(--text-primary)' }}>
            {greeting()}, {user?.firstName || 'User'}!
          </h1>
        </div>
        <p style={{ fontSize: '0.9rem', color: 'var(--text-muted)' }}>
          Welcome to your dashboard. Here you can manage content and monitor platform activity.
        </p>
      </div>

      {/* Stats */}
      <div className="stats-grid">
        <div className="stat-card">
          <div className="stat-card-icon" style={{ background: 'rgba(34, 197, 94, 0.1)', color: 'var(--green-400)' }}>
            <BookOpen size={20} />
          </div>
          <div className="stat-card-value">{loading ? '...' : stats.courses}</div>
          <div className="stat-card-label">Courses</div>
        </div>

        {isAdmin && (
          <div className="stat-card">
            <div className="stat-card-icon" style={{ background: 'rgba(59, 130, 246, 0.1)', color: '#60a5fa' }}>
              <FolderTree size={20} />
            </div>
            <div className="stat-card-value">{loading ? '...' : stats.departments}</div>
            <div className="stat-card-label">Departments</div>
          </div>
        )}

        <div className="stat-card">
          <div className="stat-card-icon" style={{ background: 'rgba(168, 85, 247, 0.1)', color: '#c084fc' }}>
            <Users size={20} />
          </div>
          <div className="stat-card-value">—</div>
          <div className="stat-card-label">Students</div>
        </div>

        <div className="stat-card">
          <div className="stat-card-icon" style={{ background: 'rgba(245, 158, 11, 0.1)', color: '#fbbf24' }}>
            <ClipboardList size={20} />
          </div>
          <div className="stat-card-value">—</div>
          <div className="stat-card-label">Enrollments</div>
        </div>
      </div>

      {/* Quick Tips */}
      <div className="data-table-wrapper" style={{ padding: '24px' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '10px', marginBottom: '16px' }}>
          <TrendingUp size={18} style={{ color: 'var(--green-400)' }} />
          <h3 style={{ fontSize: '1rem', fontWeight: 700, color: 'var(--text-primary)' }}>Quick Tips</h3>
        </div>
        <div style={{ display: 'grid', gap: '12px' }}>
          {(isAdmin || isInstructor) && (
            <div style={{ display: 'flex', alignItems: 'center', gap: '12px', padding: '14px 16px', background: 'rgba(255,255,255,0.02)', borderRadius: '10px', border: '1px solid var(--border)' }}>
              <BookOpen size={16} style={{ color: 'var(--green-400)', flexShrink: 0 }} />
              <span style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
                You can add new courses from the "Courses" page → click "New Course"
              </span>
            </div>
          )}
          {isAdmin && (
            <div style={{ display: 'flex', alignItems: 'center', gap: '12px', padding: '14px 16px', background: 'rgba(255,255,255,0.02)', borderRadius: '10px', border: '1px solid var(--border)' }}>
              <FolderTree size={16} style={{ color: '#60a5fa', flexShrink: 0 }} />
              <span style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
                Create departments to organize courses and assign instructors to each department
              </span>
            </div>
          )}
          <div style={{ display: 'flex', alignItems: 'center', gap: '12px', padding: '14px 16px', background: 'rgba(255,255,255,0.02)', borderRadius: '10px', border: '1px solid var(--border)' }}>
            <ClipboardList size={16} style={{ color: '#fbbf24', flexShrink: 0 }} />
            <span style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
              Track student enrollments across all courses from the "Enrollments" page
            </span>
          </div>
        </div>
      </div>
    </div>
  )
}
