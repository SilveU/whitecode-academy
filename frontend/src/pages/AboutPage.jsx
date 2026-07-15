import PageLayout from '../components/common/PageLayout'

export default function AboutPage() {
  return (
    <PageLayout 
      title="About Us" 
      subtitle="Discover our vision and mission to empower the next generation of tech leaders"
    >
      <div className="page-text-content">
        <h2>Our Vision</h2>
        <p>We strive to be the premier practical learning hub bridging the gap between traditional education and rapidly evolving industry demands.</p>
        
        <h2>Our Mission</h2>
        <ul>
          <li>Deliver world-class, up-to-date educational curricula.</li>
          <li>Provide an interactive, collaborative, and supportive environment for every student.</li>
          <li>Focus on hands-on practical execution and building real-world projects.</li>
          <li>Issue verified completion certificates that strengthen students' professional portfolios.</li>
        </ul>

        <h2>Our Story</h2>
        <p>White Academy was born out of the belief that career-ready skills are acquired through continuous practice and execution rather than mere theory. Since our launch, we have empowered thousands of ambitious learners to transition into successful careers across engineering, design, management, and marketing.</p>
      </div>
    </PageLayout>
  )
}
