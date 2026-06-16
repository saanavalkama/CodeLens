import { useState } from "react"
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from "@/components/ui/collapsible"
import { Prism as SyntaxHighlighter } from "react-syntax-highlighter"
import { vscDarkPlus } from "react-syntax-highlighter/dist/esm/styles/prism"
import { getLanguage } from "../utils/getLanguage"

interface Props {
  filePath: string
  content: string
  startLine: number
  endLine: number
  similarity: number
}

export default function CodeCollapsible({ filePath, content, startLine, endLine, similarity }: Props) {
  const [open, setOpen] = useState(false)

  const fileName = filePath?.split("/").pop() ?? filePath ?? "unknown"
  const similarityPct = Math.round(similarity * 100)

  return (
    <Collapsible open={open} onOpenChange={setOpen}>
      <CollapsibleTrigger className="w-full flex items-center justify-between gap-3 px-3 py-2 rounded-lg bg-white/[0.04] border border-white/[0.08] hover:bg-white/[0.07] transition-colors group text-left">
        <div className="flex items-center gap-2 min-w-0">
          <svg
            width="13"
            height="13"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
            strokeLinecap="round"
            strokeLinejoin="round"
            className={`shrink-0 text-white/30 transition-transform duration-200 ${open ? "rotate-90" : ""}`}
          >
            <polyline points="9 18 15 12 9 6" />
          </svg>
          <span className="text-xs text-white/70 font-mono truncate" title={filePath}>
            {fileName}
          </span>
          <span className="text-white/25 text-xs shrink-0">
            :{startLine}–{endLine}
          </span>
        </div>
        <span
          className={`shrink-0 text-xs font-medium tabular-nums px-1.5 py-0.5 rounded-md ${
            similarityPct >= 80
              ? "bg-emerald-500/10 text-emerald-400"
              : similarityPct >= 60
              ? "bg-amber-500/10 text-amber-400"
              : "bg-white/5 text-white/30"
          }`}
        >
          {similarityPct}%
        </span>
      </CollapsibleTrigger>

      <CollapsibleContent>
        <div className="mt-1 rounded-lg overflow-hidden border border-white/[0.08]">
          <div className="flex items-center justify-between px-3 py-1.5 bg-white/[0.03] border-b border-white/[0.06]">
            <span className="text-white/30 text-xs font-mono truncate">{filePath}</span>
            <span className="text-white/20 text-xs shrink-0 ml-2">
              {getLanguage(filePath)}
            </span>
          </div>
          <SyntaxHighlighter
            language={getLanguage(filePath)}
            style={vscDarkPlus}
            showLineNumbers
            startingLineNumber={startLine}
            customStyle={{
              margin: 0,
              borderRadius: 0,
              background: "rgba(255,255,255,0.02)",
              fontSize: "0.75rem",
              lineHeight: "1.6",
            }}
            lineNumberStyle={{ color: "rgba(255,255,255,0.15)", minWidth: "2.5em" }}
          >
            {content}
          </SyntaxHighlighter>
        </div>
      </CollapsibleContent>
    </Collapsible>
  )
}
