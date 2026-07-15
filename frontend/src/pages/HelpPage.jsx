import PageLayout from '../components/common/PageLayout'

export default function HelpPage() {
  return (
    <PageLayout 
      title="Help Center" 
      subtitle="We are here to support your learning journey. How can we assist you today?"
    >
      <div className="page-text-content">
        <h2>Quick Start Guides</h2>
        <ul>
          <li>How to create and verify your new account.</li>
          <li>How to enroll in a learning track and manage payments.</li>
          <li>How to download and verify your completion certificates.</li>
          <li>Account settings and password reset procedures.</li>
        </ul>

        <h2>Live Technical Support</h2>
        <p>If you encounter any technical issues on the platform, please reach out via live chat or submit a technical support ticket through our contact page.</p>
        
        <h2>Operating Hours</h2>
        <p>Our support team is available 7 days a week from 9:00 AM to 9:00 PM UTC.</p>
      </div>
    </PageLayout>
  )
}
