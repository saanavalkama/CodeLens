import { useState } from "react"
import { Input } from "@/components/ui/input"
import { Button } from "@/components/ui/button"
import { useCreateOrUpdateKey, useKeyStatus } from "../hooks/userHooks"

export default function SettingsPage() {
  const [key, setKey] = useState("")
  const [show, setShow] = useState(false)

  const { data: keyStatus, isPending: statusLoading } = useKeyStatus()
  const saveKey = useCreateOrUpdateKey()

  const hasKey: boolean = keyStatus?.hasKey ?? false

  const handleSubmit = (e: { preventDefault(): void }) => {
    e.preventDefault()
    const trimmed = key.trim()
    if (!trimmed) return
    saveKey.mutate(trimmed, {
      onSuccess: () => setKey(""),
    })
  }

  return (
    <div className="max-w-xl mx-auto px-4 py-12 flex flex-col gap-8">
      <div>
        <h1 className="text-xl font-semibold text-white">Settings</h1>
        <p className="text-white/50 text-sm mt-1">Manage your account configuration.</p>
      </div>

      {/* API Key section */}
      <section className="rounded-xl border border-white/10 bg-white/[0.03] p-6 flex flex-col gap-5">
        <div className="flex items-center justify-between">
          <div>
            <h2 className="text-sm font-medium text-white">OpenAI API Key</h2>
            <p className="text-white/40 text-xs mt-0.5">Used for semantic search and code analysis.</p>
          </div>

          {statusLoading ? (
            <span className="text-white/30 text-xs">Checking…</span>
          ) : hasKey ? (
            <span className="inline-flex items-center gap-1.5 rounded-full bg-emerald-500/10 border border-emerald-500/20 px-2.5 py-0.5 text-xs text-emerald-400">
              <span className="w-1.5 h-1.5 rounded-full bg-emerald-400 inline-block" />
              Key added
            </span>
          ) : (
            <span className="inline-flex items-center gap-1.5 rounded-full bg-white/5 border border-white/10 px-2.5 py-0.5 text-xs text-white/40">
              <span className="w-1.5 h-1.5 rounded-full bg-white/30 inline-block" />
              Not added
            </span>
          )}
        </div>

        <form onSubmit={handleSubmit} className="flex flex-col gap-3">
          <div className="relative">
            <Input
              type={show ? "text" : "password"}
              value={key}
              onChange={(e) => setKey(e.target.value)}
              placeholder="sk-..."
              autoComplete="off"
              className="pr-16 bg-white/[0.05] border-white/10 text-white placeholder:text-white/25 focus-visible:ring-indigo-500/50 focus-visible:border-indigo-500/50"
            />
            <button
              type="button"
              onClick={() => setShow((v) => !v)}
              className="absolute right-3 top-1/2 -translate-y-1/2 text-white/30 hover:text-white/60 text-xs transition-colors"
            >
              {show ? "Hide" : "Show"}
            </button>
          </div>

          <Button
            type="submit"
            disabled={!key.trim() || saveKey.isPending}
            className="self-start bg-indigo-600 hover:bg-indigo-500 text-white"
          >
            {saveKey.isPending ? "Saving…" : hasKey ? "Update key" : "Add key"}
          </Button>
        </form>

        {/* Security note */}
        <div className="rounded-lg bg-white/[0.03] border border-white/[0.06] px-4 py-3 flex flex-col gap-1">
          <p className="text-white/50 text-xs font-medium uppercase tracking-wide">How your key is stored</p>
          <ul className="text-white/35 text-xs flex flex-col gap-1 mt-1 list-disc list-inside">
            <li>Encrypted at rest using AES-256 before being written to the database.</li>
            <li>Transmitted over HTTPS — never sent in plain text.</li>
            <li>Only decrypted server-side, at the moment it is needed for a request.</li>
            <li>Never logged, exposed in API responses, or shared with third parties.</li>
          </ul>
        </div>
      </section>
    </div>
  )
}
