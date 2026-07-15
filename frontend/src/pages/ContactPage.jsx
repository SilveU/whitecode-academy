import { useState } from 'react'
import PageLayout from '../components/common/PageLayout'
import { Send, CheckCircle2 } from 'lucide-react'

export default function ContactPage() {
  const [form, setForm] = useState({ name: '', email: '', message: '' })
  const [submitted, setSubmitted] = useState(false)

  const handleSubmit = (e) => {
    e.preventDefault()
    if (!form.name || !form.email || !form.message) return
    // TODO: integrate with backend contact endpoint
    setSubmitted(true)
    setTimeout(() => setSubmitted(false), 4000)
    setForm({ name: '', email: '', message: '' })
  }

  return (
    <PageLayout 
      title="Contact Us" 
      subtitle="We would love to hear from you and answer your inquiries"
    >
      <div className="page-text-content" style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))', gap: '40px' }}>
        <div>
          <h2>Contact Information</h2>
          <p><strong>Email Address:</strong><br /> hello@whiteacademy.com</p>
          <p><strong>Phone Number:</strong><br /> +971 50 123 4567</p>
          <p><strong>Address:</strong><br /> Dubai, United Arab Emirates, Innovation Park.</p>
        </div>
        <div style={{ background: 'rgba(255,255,255,0.02)', padding: '30px', borderRadius: '16px', border: '1px solid var(--border)' }}>
          <h3 style={{ marginBottom: '20px', color: 'var(--text-primary)' }}>Send a Message</h3>

          {submitted && (
            <div style={{ display: 'flex', alignItems: 'center', gap: '8px', padding: '12px 16px', marginBottom: '16px', background: 'rgba(34, 197, 94, 0.08)', border: '1px solid rgba(34, 197, 94, 0.2)', borderRadius: '10px', color: 'var(--green-400)', fontSize: '0.85rem' }}>
              <CheckCircle2 size={16} />
              Your message has been sent successfully! We will get back to you soon.
            </div>
          )}

          <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '15px' }}>
            <input
              type="text"
              placeholder="Full Name"
              value={form.name}
              onChange={(e) => setForm(f => ({ ...f, name: e.target.value }))}
              required
              style={{ padding: '12px', background: 'var(--gray-900)', border: '1px solid var(--border)', borderRadius: '8px', color: 'white', fontFamily: "'Cairo', sans-serif" }}
            />
            <input
              type="email"
              placeholder="Email Address"
              value={form.email}
              onChange={(e) => setForm(f => ({ ...f, email: e.target.value }))}
              required
              style={{ padding: '12px', background: 'var(--gray-900)', border: '1px solid var(--border)', borderRadius: '8px', color: 'white', fontFamily: "'Cairo', sans-serif" }}
            />
            <textarea
              placeholder="Your Message"
              rows="4"
              value={form.message}
              onChange={(e) => setForm(f => ({ ...f, message: e.target.value }))}
              required
              style={{ padding: '12px', background: 'var(--gray-900)', border: '1px solid var(--border)', borderRadius: '8px', color: 'white', fontFamily: "'Cairo', sans-serif", resize: 'vertical' }}
            ></textarea>
            <button
              type="submit"
              style={{
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                gap: '8px',
                padding: '12px',
                background: 'linear-gradient(135deg, var(--green-600), var(--green-500))',
                border: 'none',
                borderRadius: '8px',
                color: 'white',
                fontWeight: 'bold',
                fontFamily: "'Cairo', sans-serif",
                cursor: 'pointer',
                transition: 'all 0.2s ease',
              }}
            >
              <Send size={16} />
              Send Message
            </button>
          </form>
        </div>
      </div>
    </PageLayout>
  )
}
