import PageLayout from '../components/common/PageLayout'

export default function TermsPage() {
  return (
    <PageLayout 
      title="Terms & Conditions" 
      subtitle="Please read these terms carefully before using the White Academy platform"
    >
      <div className="page-text-content">
        <h2>1. Acceptance of Terms</h2>
        <p>By accessing and using White Academy, you agree to comply with and be bound by these terms and conditions. If you do not agree to these terms, please do not use our platform.</p>
        
        <h2>2. Intellectual Property & Localized Content</h2>
        <p>Our curriculum consists of curated and localized educational courses where we provide translated subtitles, custom interactive quizzes, and practical assignments to maximize student outcomes. Original content copyrights remain with their respective creators, while our value lies in localization, tutoring, and interactive learning infrastructure.</p>

        <h2>3. User Accounts</h2>
        <p>You are responsible for maintaining the confidentiality of your account credentials and password, and you accept full responsibility for all activities occurring under your account.</p>

        <h2>4. Refund Policy</h2>
        <p>Users are eligible to request a full refund within 14 days of purchase, provided that they have not consumed or completed more than 20% of the course content.</p>

        <h2>5. Amendments to Terms</h2>
        <p>We reserve the right to modify these terms at any time. Users will be notified of any material changes via email or through platform announcements.</p>
      </div>
    </PageLayout>
  )
}
