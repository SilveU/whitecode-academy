import { useEffect, useRef } from 'react'

const CODE_SYMBOLS = ['</', '{}', '/>', '()', '=>', '&&', '||', '[]', '+=', '!=', '==', '**', '::', ';;', '##']

export default function BackgroundAnimation() {
  const particlesRef = useRef(null)

  useEffect(() => {
    const container = particlesRef.current
    if (!container) return

    for (let i = 0; i < 15; i++) {
      const particle = document.createElement('span')
      particle.className = 'code-particle'
      particle.textContent = CODE_SYMBOLS[Math.floor(Math.random() * CODE_SYMBOLS.length)]
      particle.style.left = `${Math.random() * 100}%`
      particle.style.animationDelay = `${Math.random() * 8}s`
      particle.style.animationDuration = `${6 + Math.random() * 6}s`
      particle.style.fontSize = `${10 + Math.random() * 8}px`
      container.appendChild(particle)
    }

    return () => {
      container.innerHTML = ''
    }
  }, [])

  return (
    <div className="bg-animation">
      <div className="floating-shape shape-1" />
      <div className="floating-shape shape-2" />
      <div className="floating-shape shape-3" />
      <div className="floating-shape shape-4" />
      <div className="floating-shape shape-5" />
      <div ref={particlesRef} />
    </div>
  )
}
