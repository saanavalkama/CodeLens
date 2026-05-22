import { useFetchRepos, useRepos } from "../hooks/repoHooks"

function FolderIcon() {
    return (
        <svg width="15" height="15" viewBox="0 0 24 24" fill="currentColor" className="shrink-0 text-[#6366f1]">
            <path d="M10 4H4c-1.11 0-2 .89-2 2v12c0 1.097.903 2 2 2h16c1.097 0 2-.903 2-2V8c0-1.11-.89-2-2-2h-8l-2-2z" />
        </svg>
    )
}

export default function RepoList() {
    const { data: repos, isPending, isError } = useRepos()
    const { mutate, isPending: isFetchPending, isError: isFetchError } = useFetchRepos()

    return (
        <aside className="w-72 shrink-0 border-r border-[#111120] flex flex-col p-5">
            <p className="text-[10px] font-semibold tracking-[0.15em] uppercase text-[#3f3f5c] px-1 mb-4">
                Workspaces
            </p>

            {(isError || isFetchError) && (
                <p className="text-xs text-red-400 px-1 mb-3">Something went wrong</p>
            )}

            {isPending ? (
                <p className="text-xs text-[#3f3f5c] px-1">Loading…</p>
            ) : !repos || repos.length === 0 ? (
                <div className="flex flex-col gap-3">
                    <p className="text-xs text-[#52525b] px-1 leading-relaxed">
                        No repos connected yet.
                    </p>
                    <button
                        className="flex items-center justify-center rounded-lg border border-[#6366f1]/40 bg-[#6366f1]/10 text-[#818cf8] text-sm font-medium py-2.5 px-3 hover:bg-[#6366f1]/20 transition-colors disabled:opacity-50 cursor-pointer"
                        disabled={isFetchPending}
                        onClick={() => mutate()}
                    >
                        {isFetchPending ? 'Connecting…' : 'Connect GitHub'}
                    </button>
                </div>
            ) : (
                <>
                    <ul className="flex flex-col gap-0.5 flex-1 overflow-y-auto">
                        {repos.map(repo => (
                            <li
                                key={repo.id}
                                className="flex items-center gap-2.5 px-2.5 py-2 rounded-md hover:bg-[#111120] cursor-pointer text-sm text-[#a1a1aa] hover:text-white transition-colors"
                            >
                                <FolderIcon />
                                <span className="truncate">{repo.name}</span>
                            </li>
                        ))}
                    </ul>
                    <button
                        className="mt-4 flex items-center justify-center rounded-lg border border-[#1e1e2e] text-[#52525b] text-sm font-medium py-2.5 px-3 hover:text-[#a1a1aa] hover:border-[#2a2a3e] transition-colors disabled:opacity-50 cursor-pointer"
                        disabled={isFetchPending}
                        onClick={() => mutate()}
                    >
                        {isFetchPending ? 'Syncing…' : 'Sync repos'}
                    </button>
                </>
            )}
        </aside>
    )
}
