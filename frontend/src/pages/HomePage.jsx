import Navbar from '../components/common/Navbar'
import Footer from '../components/common/Footer'
import WelcomeBanner from '../components/home/WelcomeBanner'
import PathsSection from '../components/home/PathsSection'
import FeaturesSection from '../components/home/FeaturesSection'
import TestimonialsSection from '../components/home/TestimonialsSection'
import CTASection from '../components/home/CTASection'
import './HomePage.css'

export default function HomePage() {
  return (
    <div className="home-page">
      <Navbar />
      <WelcomeBanner />
      <PathsSection />
      <FeaturesSection />
      <TestimonialsSection />
      <CTASection />
      <Footer />
    </div>
  )
}
