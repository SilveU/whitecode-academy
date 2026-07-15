import PageLayout from '../components/common/PageLayout'

export default function BlogPage() {
  return (
    <PageLayout 
      title="Blog & Insights" 
      subtitle="Latest articles, industry news, and expert tips in tech and business"
    >
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))', gap: '20px' }}>
        {[1, 2, 3, 4, 5, 6].map((i) => (
          <div key={i} style={{ background: 'rgba(255,255,255,0.02)', padding: '24px', borderRadius: '16px', border: '1px solid var(--border)' }}>
            <div style={{ width: '100%', height: '160px', background: 'rgba(34,197,94,0.1)', borderRadius: '10px', marginBottom: '20px' }}></div>
            <span style={{ fontSize: '0.8rem', color: 'var(--green-400)', fontWeight: '600' }}>Career Growth</span>
            <h3 style={{ margin: '10px 0', fontSize: '1.2rem', color: 'var(--text-primary)' }}>How to Launch Your Tech Career in 2026</h3>
            <p style={{ fontSize: '0.9rem', color: 'var(--text-muted)', lineHeight: '1.6' }}>Practical action plans and insights from industry professionals to build a standout portfolio and land your dream job.</p>
          </div>
        ))}
      </div>
    </PageLayout>
  )
}
