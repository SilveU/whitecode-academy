import PageLayout from '../components/common/PageLayout'

export default function FAQPage() {
  return (
    <PageLayout 
      title="Frequently Asked Questions" 
      subtitle="Quick answers to the most common questions from our learners"
    >
      <div className="page-text-content">
        <h2>Do I need prior experience to enroll in these tracks?</h2>
        <p>No, most of our tracks are designed from the ground up to take you from complete beginner to advanced professional proficiency.</p>
        
        <h2>Will I receive a certificate upon completion?</h2>
        <p>Yes! Once you complete all course modules and practical assignments, you will receive a verified completion certificate suitable for your resume and LinkedIn profile.</p>

        <h2>How do I interact with instructors and mentors?</h2>
        <p>Each learning path features a dedicated discussion forum and community group where you can ask questions, and instructors or teaching assistants will respond within 24 hours.</p>

        <h2>Can I get a refund if the course isn't a good fit?</h2>
        <p>Absolutely. We offer a full 14-day money-back guarantee from the date of enrollment if the content does not meet your expectations.</p>
      </div>
    </PageLayout>
  )
}
