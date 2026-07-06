import PageLayout from '../components/common/PageLayout'

export default function ContactPage() {
  return (
    <PageLayout 
      title="تواصل معنا" 
      subtitle="يسعدنا الرد على استفساراتك واستقبال اقتراحاتك"
    >
      <div className="page-text-content" style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '40px' }}>
        <div>
          <h2>معلومات التواصل</h2>
          <p><strong>البريد الإلكتروني:</strong><br /> hello@whiteacademy.com</p>
          <p><strong>رقم الهاتف:</strong><br /> +971 50 123 4567</p>
          <p><strong>العنوان:</strong><br /> دبي، الإمارات العربية المتحدة، مجمع الابتكار.</p>
        </div>
        <div style={{ background: 'rgba(255,255,255,0.02)', padding: '30px', borderRadius: '16px', border: '1px solid var(--border)' }}>
          <h3 style={{ marginBottom: '20px', color: 'var(--text-primary)' }}>أرسل رسالة</h3>
          <form style={{ display: 'flex', flexDirection: 'column', gap: '15px' }}>
            <input type="text" placeholder="الاسم الكامل" style={{ padding: '12px', background: 'var(--gray-900)', border: '1px solid var(--border)', borderRadius: '8px', color: 'white' }} />
            <input type="email" placeholder="البريد الإلكتروني" style={{ padding: '12px', background: 'var(--gray-900)', border: '1px solid var(--border)', borderRadius: '8px', color: 'white' }} />
            <textarea placeholder="رسالتك" rows="4" style={{ padding: '12px', background: 'var(--gray-900)', border: '1px solid var(--border)', borderRadius: '8px', color: 'white' }}></textarea>
            <button type="button" style={{ padding: '12px', background: 'var(--green-500)', border: 'none', borderRadius: '8px', color: 'white', fontWeight: 'bold', cursor: 'pointer' }}>إرسال</button>
          </form>
        </div>
      </div>
    </PageLayout>
  )
}
