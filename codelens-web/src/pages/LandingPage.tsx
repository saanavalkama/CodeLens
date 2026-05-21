import { Canvas, useFrame } from '@react-three/fiber'
import { Points, PointMaterial, Float } from '@react-three/drei'
import { motion, useInView, useScroll, useTransform } from 'framer-motion'
import { useRef, useMemo, Suspense } from 'react'
import * as THREE from 'three'

// ── 3D Hero Scene ─────────────────────────────────────────────────────────────

function ParticleCloud() {
  const ref = useRef<THREE.Points>(null!)
  const positions = useMemo(() => {
    const count = 1200
    const arr = new Float32Array(count * 3)
    for (let i = 0; i < count; i++) {
      const theta = Math.random() * Math.PI * 2
      const phi = Math.acos(2 * Math.random() - 1)
      const r = 1.8 + Math.random() * 2.4
      arr[i * 3]     = r * Math.sin(phi) * Math.cos(theta)
      arr[i * 3 + 1] = r * Math.sin(phi) * Math.sin(theta)
      arr[i * 3 + 2] = r * Math.cos(phi)
    }
    return arr
  }, [])

  useFrame((_, delta) => {
    ref.current.rotation.y += delta * 0.035
    ref.current.rotation.x += delta * 0.01
  })

  return (
    <Points ref={ref} positions={positions} stride={3} frustumCulled={false}>
      <PointMaterial transparent color="#818cf8" size={0.013} sizeAttenuation depthWrite={false} opacity={0.6} />
    </Points>
  )
}

function CoreShape() {
  const mesh = useRef<THREE.Mesh>(null!)
  useFrame(({ clock }) => {
    mesh.current.rotation.y = clock.elapsedTime * 0.11
    mesh.current.rotation.z = clock.elapsedTime * 0.07
  })
  return (
    <Float speed={0.9} floatIntensity={0.4} rotationIntensity={0.15}>
      <mesh ref={mesh}>
        <icosahedronGeometry args={[1.1, 2]} />
        <meshBasicMaterial color="#6366f1" wireframe transparent opacity={0.16} />
      </mesh>
    </Float>
  )
}

// ── Semantic Search Visual ────────────────────────────────────────────────────

const CODE_LINES = [
  'function generateToken(user: User) {',
  '  const expires = Date.now() + 3600',
  '  return sign({ user, expires })',
  '}',
  '',
  'async function refreshSession() {',
  '  const tok = await getToken()',
  '  if (tok.expires < Date.now()) {',
  '    return refresh(tok)',
  '  }',
  '}',
]
const MATCH_LINES = [1, 7]
const LINE_H = 22
const PAD_TOP = 14
const GLASS_H = 64

function lensY(lineIdx: number) {
  return PAD_TOP + lineIdx * LINE_H + LINE_H / 2 - GLASS_H / 2
}

function SearchVisual() {
  const ref = useRef<HTMLDivElement>(null)
  const active = useInView(ref, { once: true, margin: '-80px' })

  const y1 = lensY(MATCH_LINES[0])
  const y2 = lensY(MATCH_LINES[1])

  return (
    <motion.div
      ref={ref}
      className="sv"
      initial={{ scale: 0.93 }}
      animate={active ? { scale: 1 } : {}}
      transition={{ duration: 0.9, ease: [0.16, 1, 0.3, 1] }}
    >
      {/* Search bar with typing animation */}
      <motion.div
        className="sv-bar"
        initial={{ opacity: 0, y: -8 }}
        animate={active ? { opacity: 1, y: 0 } : {}}
        transition={{ delay: 0.3, duration: 0.7 }}
      >
        <MagnifyIcon />
        <motion.span
          className="sv-query"
          style={{ display: 'inline-block', overflow: 'hidden', whiteSpace: 'nowrap' }}
          initial={{ width: 0 }}
          animate={active ? { width: '100%' } : {}}
          transition={{ delay: 0.7, duration: 1.8, ease: [0.4, 0, 0.2, 1] }}
        >
          where is token expiry checked?
        </motion.span>
      </motion.div>

      {/* Code block */}
      <div className="sv-code">
        {CODE_LINES.map((line, i) => (
          <motion.div
            key={i}
            className={`sv-line${MATCH_LINES.includes(i) ? ' sv-line--match' : ''}`}
            initial={{ opacity: 0 }}
            animate={active ? { opacity: 1 } : {}}
            transition={{ delay: 0.4 + i * 0.08, duration: 0.4 }}
          >
            <span className="sv-ln">{line ? i + 1 : ''}</span>
            <span>{line || ' '}</span>
          </motion.div>
        ))}

        {/* Lens — only mount when active so the loop starts fresh */}
        {active && (
          <motion.div
            className="sv-lens"
            initial={{ opacity: 0, y: y1 - 20 }}
            animate={{
              opacity: [0, 1,  1,    1,    1,    1,    1,    0],
              y:       [y1 - 20, y1, y1, y1 + 3, y2, y2, y2, y1 - 20],
              rotate:  [0, -2, 3, -2, -3, 3, -2, 0],
            }}
            transition={{
              duration: 8,
              times: [0, 0.1, 0.26, 0.38, 0.55, 0.7, 0.86, 1],
              repeat: Infinity,
              ease: 'easeInOut',
            }}
          />
        )}
      </div>

      {/* Results */}
      <motion.div
        className="sv-results"
        initial={{ opacity: 0, y: 6 }}
        animate={active ? { opacity: 1, y: 0 } : {}}
        transition={{ delay: 2.6, duration: 0.7 }}
      >
        <span className="sv-match-label">2 semantic matches</span>
        {MATCH_LINES.map((idx, i) => (
          <motion.div
            key={idx}
            className="sv-result"
            initial={{ opacity: 0, x: -8 }}
            animate={active ? { opacity: 1, x: 0 } : {}}
            transition={{ delay: 2.9 + i * 0.4, duration: 0.5 }}
          >
            <span className="sv-result-ln">:{idx + 1}</span>
            <code>{CODE_LINES[idx].trim()}</code>
          </motion.div>
        ))}
      </motion.div>
    </motion.div>
  )
}

// ── PR Review Visual ──────────────────────────────────────────────────────────

const PR_DIFF = [
  { type: 'ctx', code: ' function validateInput(data) {' },
  { type: 'del', code: '-  if (data.length > 0) {' },
  { type: 'add', code: '+  if (data?.length && data.length > 0) {' },
  { type: 'ctx', code: '     return process(data)' },
  { type: 'del', code: '-  }' },
  { type: 'add', code: '+  }' },
  { type: 'add', code: '+  throw new Error("Invalid input")' },
  { type: 'ctx', code: ' }' },
]

function PRVisual() {
  const ref = useRef<HTMLDivElement>(null)
  const active = useInView(ref, { once: true, margin: '-80px' })

  return (
    <div ref={ref} className="pr-v">
      <motion.div
        className="pr-header"
        initial={{ opacity: 0, y: -8 }}
        animate={active ? { opacity: 1, y: 0 } : {}}
        transition={{ delay: 0.15, duration: 0.45 }}
      >
        <span className="pr-badge">PR #142</span>
        <span className="pr-title-text">fix: improve input validation</span>
        <span className="pr-files">1 file changed</span>
      </motion.div>

      <div className="pr-diff">
        {PR_DIFF.map((line, i) => (
          <motion.div
            key={i}
            className={`pr-line pr-line--${line.type}`}
            initial={{ opacity: 0, x: line.type === 'add' ? 8 : line.type === 'del' ? -8 : 0 }}
            animate={active ? { opacity: 1, x: 0 } : {}}
            transition={{ delay: 0.35 + i * 0.07, duration: 0.28 }}
          >
            <code>{line.code}</code>
          </motion.div>
        ))}
      </div>

      <motion.div
        className="pr-comment"
        initial={{ opacity: 0, y: 8 }}
        animate={active ? { opacity: 1, y: 0 } : {}}
        transition={{ delay: 1.5, duration: 0.5 }}
      >
        <div className="pr-comment-label">CodeLens review</div>
        <p>
          Null-safe access on <code>data?.length</code> prevents crash when input is{' '}
          <code>undefined</code>. Missing error throw added.
        </p>
      </motion.div>
    </div>
  )
}

// ── Feature Section wrapper ───────────────────────────────────────────────────

function FeatureSection({
  eyebrow, title, description, visual, reverse = false,
}: {
  eyebrow: string
  title: string
  description: string
  visual: React.ReactNode
  reverse?: boolean
}) {
  const ref = useRef<HTMLDivElement>(null)
  const inView = useInView(ref, { once: true, margin: '-80px' })

  return (
    <section className="fs">
      <div ref={ref} className={`fs-inner${reverse ? ' fs-r' : ''}`}>
        <motion.div
          className="fs-text"
          initial={{ opacity: 0, x: reverse ? 44 : -44 }}
          animate={inView ? { opacity: 1, x: 0 } : {}}
          transition={{ duration: 0.7, ease: 'easeOut' }}
        >
          <p className="section-eyebrow">{eyebrow}</p>
          <h2 className="fs-title">{title}</h2>
          <p className="fs-desc">{description}</p>
        </motion.div>

        <motion.div
          className="fs-visual-wrap"
          initial={{ opacity: 0, x: reverse ? -44 : 44 }}
          animate={inView ? { opacity: 1, x: 0 } : {}}
          transition={{ duration: 0.7, ease: 'easeOut', delay: 0.15 }}
        >
          {visual}
        </motion.div>
      </div>
    </section>
  )
}

// ── Icons ─────────────────────────────────────────────────────────────────────

function GitHubIcon() {
  return (
    <svg width="18" height="18" viewBox="0 0 24 24" fill="currentColor" aria-hidden>
      <path d="M12 0C5.37 0 0 5.37 0 12c0 5.31 3.435 9.795 8.205 11.385.6.105.825-.255.825-.57 0-.285-.015-1.23-.015-2.235-3.015.555-3.795-.735-4.035-1.41-.135-.345-.72-1.41-1.23-1.695-.42-.225-1.02-.78-.015-.795.945-.015 1.62.87 1.845 1.23 1.08 1.815 2.805 1.305 3.495.99.105-.78.42-1.305.765-1.605-2.67-.3-5.46-1.335-5.46-5.925 0-1.305.465-2.385 1.23-3.225-.12-.3-.54-1.53.12-3.18 0 0 1.005-.315 3.3 1.23.96-.27 1.98-.405 3-.405s2.04.135 3 .405c2.295-1.56 3.3-1.23 3.3-1.23.66 1.65.24 2.88.12 3.18.765.84 1.23 1.905 1.23 3.225 0 4.605-2.805 5.625-5.475 5.925.435.375.81 1.095.81 2.22 0 1.605-.015 2.895-.015 3.3 0 .315.225.69.825.57A12.02 12.02 0 0 0 24 12c0-6.63-5.37-12-12-12z" />
    </svg>
  )
}

function MagnifyIcon() {
  return (
    <svg className="sv-bar-icon" width="14" height="14" viewBox="0 0 20 20" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden>
      <circle cx="8.5" cy="8.5" r="5.5" />
      <path d="m13 13 3.5 3.5" strokeLinecap="round" />
    </svg>
  )
}

// ── Landing Page ──────────────────────────────────────────────────────────────

const heroStagger = { visible: { transition: { staggerChildren: 0.13 } } }
const fadeUp = { hidden: { opacity: 0, y: 24 }, visible: { opacity: 1, y: 0 } }

export default function LandingPage() {
  const heroRef = useRef<HTMLElement>(null)
  const { scrollYProgress } = useScroll({
    target: heroRef,
    offset: ['start start', 'end start'],
  })
  const heroScale = useTransform(scrollYProgress, [0, 1], [1, 1.22])
  const heroOpacity = useTransform(scrollYProgress, [0, 0.75], [1, 0])

  return (
    <div className="landing-root">

      {/* ── Hero ── */}
      <section className="hero" ref={heroRef}>
        <div className="hero-canvas" aria-hidden>
          <Canvas camera={{ position: [0, 0, 5.5], fov: 55 }} gl={{ antialias: true }}>
            <Suspense fallback={null}>
              <ParticleCloud />
              <CoreShape />
            </Suspense>
          </Canvas>
        </div>

        <motion.div className="hero-content" style={{ scale: heroScale, opacity: heroOpacity }} initial="hidden" animate="visible" variants={heroStagger}>
          <motion.p variants={fadeUp} transition={{ duration: 0.5 }} className="eyebrow">
            Codebase Intelligence
          </motion.p>
          <motion.h1 variants={fadeUp} transition={{ duration: 0.65 }} className="hero-title">
            Code<span>Lens</span>
          </motion.h1>
          <motion.p variants={fadeUp} transition={{ duration: 0.65 }} className="hero-sub">
            Ask anything about your codebase.<br />Get precise answers, instantly.
          </motion.p>
          <motion.a variants={fadeUp} transition={{ duration: 0.65 }} href="/login" className="hero-cta">
            <GitHubIcon />
            Sign in with GitHub
          </motion.a>
        </motion.div>

        <div className="hero-fade" aria-hidden />
      </section>

      {/* ── Semantic Search ── */}
      <FeatureSection
        eyebrow="Semantic Search"
        title="Ask anything about your code"
        description="CodeLens understands your codebase semantically. Find functions, logic, and patterns using plain English — no grep patterns or file paths to memorize."
        visual={<SearchVisual />}
      />

      {/* ── PR Reviews ── */}
      <FeatureSection
        eyebrow="PR Reviews"
        title="Intelligent pull request analysis"
        description="Get smart PR feedback grounded in your full codebase context. CodeLens reads the diff and surfaces potential issues, missing handling, and improvements before merge."
        visual={<PRVisual />}
        reverse
      />

      {/* ── Footer CTA ── */}
      <section className="footer-cta">
        <motion.h2
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true }}
          transition={{ duration: 0.6 }}
        >
          Start exploring your code
        </motion.h2>
        <motion.a
          href="/login"
          className="hero-cta hero-cta--outline"
          initial={{ opacity: 0 }}
          whileInView={{ opacity: 1 }}
          viewport={{ once: true }}
          transition={{ duration: 0.6, delay: 0.2 }}
        >
          Get started free →
        </motion.a>
      </section>

    </div>
  )
}
