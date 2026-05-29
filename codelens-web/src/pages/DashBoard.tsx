import RepoList from "../features/repos/components/RepoList"

const introCards = [
    {
        icon: (
            <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.75" strokeLinecap="round" strokeLinejoin="round">
                <path d="M22 19a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h5l2 3h9a2 2 0 0 1 2 2z" />
            </svg>
        ),
        title: "Open a workspace",
        desc: "Click Connect GitHub to import your repos, then select one from the sidebar to open it as a workspace.",
    },
    {
        icon: (
            <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.75" strokeLinecap="round" strokeLinejoin="round">
                <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z" />
            </svg>
        ),
        title: "Ask in plain English",
        desc: "Type any question about your codebase — find functions, understand logic, trace bugs — and get cited answers.",
    },
    {
        icon: (
            <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.75" strokeLinecap="round" strokeLinejoin="round">
                <circle cx="12" cy="12" r="10" />
                <polyline points="12 6 12 12 16 14" />
            </svg>
        ),
        title: "Free plan",
        desc: "5 queries per day included. Perfect for exploring CodeLens and getting familiar with your codebase.",
        badge: "Free",
    },
    {
        icon: (
            <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.75" strokeLinecap="round" strokeLinejoin="round">
                <polygon points="13 2 3 14 12 14 11 22 21 10 12 10 13 2" />
            </svg>
        ),
        title: "Pro plan",
        desc: "Unlimited queries, priority support, and advanced analytics. No caps, no slowdowns.",
        badge: "Pro",
    },
]

export default function DashBoard() {
    return (
        <div className="flex h-[calc(100svh-56px)]">
            <RepoList />
            <div className="flex-1 overflow-auto flex items-center justify-center p-10">
                <div className="w-full max-w-2xl">
                    <h1 className="text-3xl font-semibold tracking-tight text-white mb-3">
                        Get started
                    </h1>
                    <p className="text-sm text-[#71717a] leading-relaxed mb-10">
                        Connect your GitHub repositories and start asking questions about your code in plain English.
                    </p>
                    <div className="grid grid-cols-2 gap-4">
                        {introCards.map(card => (
                            <div
                                key={card.title}
                                className="relative bg-[#0a0a14] border border-[#1e1e2e] rounded-xl p-6 flex flex-col gap-4"
                            >
                                {card.badge && (
                                    <span className={`absolute top-5 right-5 text-[10px] font-semibold tracking-wide px-2.5 py-0.5 rounded-full ${
                                        card.badge === 'Pro'
                                            ? 'bg-[#6366f1]/15 text-[#818cf8]'
                                            : 'bg-[#27272a] text-[#71717a]'
                                    }`}>
                                        {card.badge}
                                    </span>
                                )}
                                <span className="text-[#6366f1]">{card.icon}</span>
                                <div>
                                    <h3 className="text-base font-semibold text-white mb-2">{card.title}</h3>
                                    <p className="text-sm text-[#71717a] leading-relaxed">{card.desc}</p>
                                </div>
                            </div>
                        ))}
                    </div>
                </div>
            </div>
        </div>
    )
}
