import { useState, useRef, useEffect } from "react"
import { useParams } from "react-router-dom"
import { Button } from "../components/ui/button"
import { Textarea } from "../components/ui/textarea"
import { ScrollArea } from "../components/ui/scroll-area"
import { Separator } from "../components/ui/separator"
import { UseConversations, useCreateConversations } from "../features/chat/hooks/conversationHooks"
import { useMessages, useSendMessage } from "../features/chat/hooks/messageHooks"

export default function WorkSpace() {
  const params = useParams()
  const repoId = params.id!

  const [activeConversationId, setActiveConversationId] = useState<string | null>(null)
  const [input, setInput] = useState("")
  const bottomRef = useRef<HTMLDivElement>(null)

  const conversationsQuery = UseConversations(repoId)
  const createConversation = useCreateConversations()
  const messagesQuery = useMessages(repoId, activeConversationId ?? "")
  const sendMessage = useSendMessage()

  useEffect(() => {
    if (!activeConversationId && conversationsQuery.data && conversationsQuery.data.length > 0) {
      setActiveConversationId(conversationsQuery.data[0].id)
    }
  }, [conversationsQuery.data, activeConversationId])

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: "smooth" })
  }, [messagesQuery.data])

  if (!params.id) return <p>Something went wrong</p>

  const handleNewConversation = () => {
    createConversation.mutate(repoId, {
      onSuccess: (data) => setActiveConversationId(data.id),
    })
  }

  const handleSend = () => {
    const trimmed = input.trim()
    if (!trimmed || !activeConversationId) return
    sendMessage.mutate({ repoId, conversationId: activeConversationId, message: trimmed })
    setInput("")
  }

  const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault()
      handleSend()
    }
  }

  const conversations = conversationsQuery.data ?? []
  const messages = messagesQuery.data ?? []
  const hasConversations = conversations.length > 0

  return (
    <div className="flex h-[calc(100vh-3.5rem)]">
      {/* Sidebar */}
      <aside className="w-64 flex flex-col border-r border-white/10 bg-white/[0.03] shrink-0">
        <div className="p-3">
          <Button
            variant="outline"
            onClick={handleNewConversation}
            disabled={createConversation.isPending}
            className="w-full justify-start gap-2 border-white/10 bg-transparent text-white/70 hover:text-white hover:bg-white/10"
          >
            <span className="text-lg leading-none">+</span>
            {createConversation.isPending ? "Creating…" : "New conversation"}
          </Button>
        </div>

        <Separator className="bg-white/10" />

        <ScrollArea className="flex-1">
          <div className="p-2 flex flex-col gap-1">
            {conversationsQuery.isPending && (
              <p className="text-white/30 text-xs px-3 py-2">Loading…</p>
            )}
            {conversationsQuery.isError && (
              <p className="text-red-400/70 text-xs px-3 py-2">Failed to load</p>
            )}
            {conversations.map((conv) => (
              <button
                key={conv.id}
                onClick={() => setActiveConversationId(conv.id)}
                className={`w-full text-left px-3 py-2 rounded-md text-sm transition-colors truncate ${
                  activeConversationId === conv.id
                    ? "bg-white/10 text-white"
                    : "text-white/50 hover:text-white/80 hover:bg-white/5"
                }`}
              >
                {conv.title}
              </button>
            ))}
          </div>
        </ScrollArea>
      </aside>

      {/* Chat area */}
      <div className="flex flex-col flex-1 min-w-0">
        {!hasConversations && !conversationsQuery.isPending ? (
          <div className="flex flex-col flex-1 items-center justify-center gap-4 text-center px-4">
            <p className="text-white/40 text-sm">No conversations yet</p>
            <Button
              onClick={handleNewConversation}
              disabled={createConversation.isPending}
              className="bg-indigo-600 hover:bg-indigo-500 text-white"
            >
              {createConversation.isPending ? "Creating…" : "Start a conversation"}
            </Button>
          </div>
        ) : (
          <>
            {/* Messages */}
            <ScrollArea className="flex-1">
              <div className="max-w-2xl mx-auto px-4 py-6 flex flex-col gap-6">
                {messagesQuery.isPending && activeConversationId && (
                  <p className="text-white/30 text-sm text-center">Loading messages…</p>
                )}
                {messages.map((msg, i) => (
                  <div
                    key={i}
                    className={`flex gap-3 ${msg.role === "user" ? "justify-end" : "justify-start"}`}
                  >
                    {msg.role !== "user" && (
                      <div className="shrink-0 w-7 h-7 rounded-full bg-indigo-500/20 border border-indigo-500/40 flex items-center justify-center text-xs text-indigo-300 mt-0.5">
                        AI
                      </div>
                    )}
                    <div
                      className={`max-w-[80%] px-4 py-2.5 rounded-2xl text-sm leading-relaxed ${
                        msg.role === "user"
                          ? "bg-indigo-600/80 text-white rounded-br-sm"
                          : "bg-white/[0.06] text-white/90 rounded-bl-sm"
                      }`}
                    >
                      {msg.content}
                    </div>
                    {msg.role === "user" && (
                      <div className="shrink-0 w-7 h-7 rounded-full bg-white/10 border border-white/20 flex items-center justify-center text-xs text-white/60 mt-0.5">
                        U
                      </div>
                    )}
                  </div>
                ))}
                {sendMessage.isPending && (
                  <div className="flex gap-3 justify-start">
                    <div className="shrink-0 w-7 h-7 rounded-full bg-indigo-500/20 border border-indigo-500/40 flex items-center justify-center text-xs text-indigo-300 mt-0.5">
                      AI
                    </div>
                    <div className="px-4 py-2.5 rounded-2xl rounded-bl-sm bg-white/[0.06] text-white/40 text-sm">
                      Thinking…
                    </div>
                  </div>
                )}
                <div ref={bottomRef} />
              </div>
            </ScrollArea>

            {/* Input */}
            <div className="border-t border-white/10 bg-white/[0.02] px-4 py-3">
              <div className="max-w-2xl mx-auto flex gap-2 items-end">
                <Textarea
                  value={input}
                  onChange={(e) => setInput(e.target.value)}
                  onKeyDown={handleKeyDown}
                  placeholder="Ask about your codebase…"
                  rows={3}
                  disabled={!activeConversationId || sendMessage.isPending}
                  className="resize-none flex-1 bg-white/[0.06] border-white/10 text-white placeholder:text-white/30 focus-visible:ring-indigo-500/50 focus-visible:border-indigo-500/50 min-h-[80px] max-h-40 disabled:opacity-40"
                />
                <Button
                  onClick={handleSend}
                  disabled={!input.trim() || !activeConversationId || sendMessage.isPending}
                  className="bg-indigo-600 hover:bg-indigo-500 text-white shrink-0 h-10"
                >
                  Send
                </Button>
              </div>
              <p className="text-white/20 text-xs text-center mt-2">Enter to send · Shift+Enter for new line</p>
            </div>
          </>
        )}
      </div>
    </div>
  )
}
