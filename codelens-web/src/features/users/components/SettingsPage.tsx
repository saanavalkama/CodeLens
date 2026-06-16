import { useState } from "react"
import { useNavigate } from "react-router-dom"
import { KeyRound, Trash2, type LucideIcon } from "lucide-react"
import { useQueryClient } from "@tanstack/react-query"
import { Input } from "@/components/ui/input"
import { Button } from "@/components/ui/button"
import {
  Sidebar,
  SidebarContent,
  SidebarMenu,
  SidebarMenuItem,
  SidebarMenuButton,
  SidebarInset,
  SidebarProvider,
} from "@/components/ui/sidebar"
import { useCreateOrUpdateKey, useKeyStatus, useDeleteUser, useDeleteKey } from "../hooks/userHooks"
import { useAuthStore } from "@/store/authStore"

type Section = "api-key" | "delete-account"

const NAV_ITEMS: { id: Section; label: string; icon: LucideIcon }[] = [
  { id: "api-key", label: "OpenAI API Key", icon: KeyRound },
  { id: "delete-account", label: "Delete Account", icon: Trash2 },
]

export default function SettingsPage() {
  const [section, setSection] = useState<Section>("api-key")
  const [key, setKey] = useState("")
  const [show, setShow] = useState(false)
  const [confirmDelete, setConfirmDelete] = useState(false)

  const navigate = useNavigate()
  const qc = useQueryClient()
  const clearToken = useAuthStore((s) => s.clearToken)

  const { data: keyStatus, isPending: statusLoading } = useKeyStatus()
  const saveKey = useCreateOrUpdateKey()
  const deleteKey = useDeleteKey()
  const deleteUser = useDeleteUser()

  const hasKey: boolean = keyStatus?.hasKey ?? false

  const handleSaveKey = (e: { preventDefault(): void }) => {
    e.preventDefault()
    const trimmed = key.trim()
    if (!trimmed) return
    saveKey.mutate(trimmed, { onSuccess: () => setKey("") })
  }

  const handleDeleteUser = () => {
    deleteUser.mutate(undefined, {
      onSuccess: () => {
        clearToken()
        qc.clear()
        navigate("/")
      },
    })
  }

  return (
    <div className="dark h-[calc(100vh-3.5rem)] flex">
      <SidebarProvider className="flex-1 min-h-0" defaultOpen>
        <Sidebar collapsible="none" className="border-r border-white/10 w-56">
          <SidebarContent className="pt-6 px-2">
            <div className="px-3 mb-4">
              <h1 className="text-sm font-semibold text-sidebar-foreground">Settings</h1>
              <p className="text-xs text-sidebar-foreground/50 mt-0.5">Manage your account</p>
            </div>
            <SidebarMenu>
              {NAV_ITEMS.map(({ id, label, icon: Icon }) => (
                <SidebarMenuItem key={id}>
                  <SidebarMenuButton
                    isActive={section === id}
                    onClick={() => {
                      setSection(id)
                      setConfirmDelete(false)
                    }}
                    className={
                      id === "delete-account" && section === id
                        ? "text-red-400 data-[active=true]:text-red-400 data-[active=true]:bg-red-500/10"
                        : ""
                    }
                  >
                    <Icon className="size-4" />
                    <span>{label}</span>
                  </SidebarMenuButton>
                </SidebarMenuItem>
              ))}
            </SidebarMenu>
          </SidebarContent>
        </Sidebar>

        <SidebarInset className="bg-transparent overflow-y-auto">
          <div className="max-w-xl px-8 py-10">
            {section === "api-key" && (
              <section className="flex flex-col gap-6">
                <div>
                  <h2 className="text-base font-semibold text-white">OpenAI API Key</h2>
                  <p className="text-white/40 text-sm mt-1">Used for semantic search and code analysis.</p>
                </div>

                <div className="flex items-center gap-3">
                  {statusLoading ? (
                    <span className="text-white/30 text-xs">Checking…</span>
                  ) : hasKey ? (
                    <>
                      <span className="inline-flex items-center gap-1.5 rounded-full bg-emerald-500/10 border border-emerald-500/20 px-2.5 py-0.5 text-xs text-emerald-400">
                        <span className="w-1.5 h-1.5 rounded-full bg-emerald-400 inline-block" />
                        Key added
                      </span>
                      <Button
                        variant="ghost"
                        size="sm"
                        disabled={deleteKey.isPending}
                        onClick={() => deleteKey.mutate()}
                        className="text-red-400 hover:text-red-300 hover:bg-red-500/10 h-7 px-2 text-xs"
                      >
                        {deleteKey.isPending ? "Deleting…" : "Delete key"}
                      </Button>
                    </>
                  ) : (
                    <span className="inline-flex items-center gap-1.5 rounded-full bg-white/5 border border-white/10 px-2.5 py-0.5 text-xs text-white/40">
                      <span className="w-1.5 h-1.5 rounded-full bg-white/30 inline-block" />
                      Not added
                    </span>
                  )}
                </div>

                <form onSubmit={handleSaveKey} className="flex flex-col gap-3">
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
            )}

            {section === "delete-account" && (
              <section className="flex flex-col gap-6">
                <div>
                  <h2 className="text-base font-semibold text-white">Delete Account</h2>
                  <p className="text-white/40 text-sm mt-1">Permanently remove your account and all associated data.</p>
                </div>

                <div className="rounded-xl border border-red-500/20 bg-red-500/[0.04] p-5 flex flex-col gap-4">
                  <div className="flex flex-col gap-3">
                    <p className="text-sm font-medium text-red-400">This action is irreversible</p>
                    <p className="text-xs text-white/40">
                      All of the following will be <span className="text-white/60 font-medium">permanently and immediately erased</span> — there is no grace period and no way to recover your data after deletion.
                    </p>
                    <ul className="text-xs text-white/40 flex flex-col gap-1.5 list-disc list-inside">
                      <li>Your account and login access</li>
                      <li>All connected repositories and their indexed contents</li>
                      <li>Your stored OpenAI API key</li>
                      <li>All code analysis and search history</li>
                    </ul>
                  </div>

                  {!confirmDelete ? (
                    <Button
                      variant="outline"
                      onClick={() => setConfirmDelete(true)}
                      className="self-start border-red-500/30 text-red-400 hover:bg-red-500/10 hover:text-red-300 hover:border-red-500/50 bg-transparent"
                    >
                      Delete my account
                    </Button>
                  ) : (
                    <div className="flex flex-col gap-3">
                      <p className="text-xs text-white/60">Are you sure? This will immediately delete everything.</p>
                      <div className="flex gap-2">
                        <Button
                          onClick={handleDeleteUser}
                          disabled={deleteUser.isPending}
                          className="bg-red-600 hover:bg-red-500 text-white"
                        >
                          {deleteUser.isPending ? "Deleting…" : "Yes, delete my account"}
                        </Button>
                        <Button
                          variant="ghost"
                          onClick={() => setConfirmDelete(false)}
                          className="text-white/50 hover:text-white hover:bg-white/5"
                        >
                          Cancel
                        </Button>
                      </div>
                    </div>
                  )}
                </div>
              </section>
            )}
          </div>
        </SidebarInset>
      </SidebarProvider>
    </div>
  )
}
