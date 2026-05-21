import { motion } from 'framer-motion'
import { authService } from '../services/authService'

function GitHubIcon() {
  return (
    <svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor" aria-hidden>
      <path d="M12 0C5.37 0 0 5.37 0 12c0 5.31 3.435 9.795 8.205 11.385.6.105.825-.255.825-.57 0-.285-.015-1.23-.015-2.235-3.015.555-3.795-.735-4.035-1.41-.135-.345-.72-1.41-1.23-1.695-.42-.225-1.02-.78-.015-.795.945-.015 1.62.87 1.845 1.23 1.08 1.815 2.805 1.305 3.495.99.105-.78.42-1.305.765-1.605-2.67-.3-5.46-1.335-5.46-5.925 0-1.305.465-2.385 1.23-3.225-.12-.3-.54-1.53.12-3.18 0 0 1.005-.315 3.3 1.23.96-.27 1.98-.405 3-.405s2.04.135 3 .405c2.295-1.56 3.3-1.23 3.3-1.23.66 1.65.24 2.88.12 3.18.765.84 1.23 1.905 1.23 3.225 0 4.605-2.805 5.625-5.475 5.925.435.375.81 1.095.81 2.22 0 1.605-.015 2.895-.015 3.3 0 .315.225.69.825.57A12.02 12.02 0 0 0 24 12c0-6.63-5.37-12-12-12z" />
    </svg>
  )
}

const perms = [
  {
    icon: (
      <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" aria-hidden>
        <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" />
        <circle cx="12" cy="7" r="4" />
      </svg>
    ),
    label: 'Read your public profile',
    sub: 'Used to identify your account',
  },
  {
    icon: (
      <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" aria-hidden>
        <path d="M9 19c-5 1.5-5-2.5-7-3m14 6v-3.87a3.37 3.37 0 0 0-.94-2.61c3.14-.35 6.44-1.54 6.44-7A5.44 5.44 0 0 0 20 4.77 5.07 5.07 0 0 0 19.91 1S18.73.65 16 2.48a13.38 13.38 0 0 0-7 0C6.27.65 5.09 1 5.09 1A5.07 5.07 0 0 0 5 4.77a5.44 5.44 0 0 0-1.5 3.78c0 5.42 3.3 6.61 6.44 7A3.37 3.37 0 0 0 9 18.13V22" />
      </svg>
    ),
    label: 'Access your repositories',
    sub: 'So you can import repos to analyse',
  },
  {
    icon: (
      <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" aria-hidden>
        <circle cx="18" cy="18" r="3" /><circle cx="6" cy="6" r="3" />
        <path d="M13 6h3a2 2 0 0 1 2 2v7" /><path d="M11 18H8a2 2 0 0 1-2-2V9" />
      </svg>
    ),
    label: 'Read pull requests',
    sub: 'Needed for PR review analysis',
  },
]

export default function Login() {
  return (
    <div className="min-h-svh flex items-center justify-center px-6 pt-24 pb-10">
      <motion.div
        className="w-full max-w-3xl bg-[#0c0c18] border border-[#1e1e2e] rounded-2xl p-10 flex flex-col items-center text-center gap-6"
        initial={{ opacity: 0, y: 24, scale: 0.97 }}
        animate={{ opacity: 1, y: 0, scale: 1 }}
        transition={{ duration: 0.55, ease: [0.16, 1, 0.3, 1] }}
      >
        {/* Wordmark */}
        <span className="text-base font-bold tracking-tight text-white">
          Code<span className="text-indigo-500">Lens</span>
        </span>

        {/* Heading */}
        <div className="flex flex-col gap-2">
          <h1 className="text-2xl font-semibold tracking-tight text-white">
            Sign in to continue
          </h1>
          <p className="text-sm text-zinc-500 leading-relaxed">
            We use GitHub to verify your identity and let you import repositories for analysis.
          </p>
        </div>

        {/* Permissions */}
        <ul className="w-full flex gap-3">
          {perms.map((p) => (
            <li
              key={p.label}
              className="flex flex-col items-center gap-2 px-5 py-6 text-center flex-1 border border-[#1e1e2e] rounded-xl"
            >
              <span className="text-indigo-400 flex">{p.icon}</span>
              <div className="flex flex-col gap-1">
                <span className="text-sm font-medium text-zinc-200">{p.label}</span>
                <span className="text-xs text-zinc-500">{p.sub}</span>
              </div>
            </li>
          ))}
        </ul>

        {/* GitHub button */}
        <button
          onClick={() => authService.connect()}
          className="flex items-center justify-center gap-2.5 bg-white text-zinc-900 text-sm font-semibold py-4 px-4 rounded-lg hover:bg-zinc-200 transition-all hover:-translate-y-px cursor-pointer"
        >
          <GitHubIcon />
          Continue with GitHub
        </button>

        {/* Footnote */}
        <p className="text-xs text-zinc-700 leading-relaxed">
          We only request the minimum permissions needed.
          <br />
          Your code never leaves your repositories.
        </p>
      </motion.div>
    </div>
  )
}
