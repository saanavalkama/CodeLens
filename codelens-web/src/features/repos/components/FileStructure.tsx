import { useMemo } from "react"
import type { RepoFile } from "../../../types/types"
import { buildFileTree } from "../utils/toTree"
import {Tree} from 'react-arborist'

interface Props{
    files: RepoFile[]
}

export default function FileStructure({files}:Props){

    const tree = useMemo(()=>buildFileTree(files ?? []),[files])

    return(
        <Tree
            data={tree}
            width={300}
            height={700}
            rowHeight={22}
            indent={12}
          >
          {({ node, style, dragHandle }) => (
  <div
    style={{
        ...style,
         display: "flex",
        alignItems: "center",
        gap: "4px",
        whiteSpace: "nowrap",
        overflow: "hidden",
        textOverflow: "ellipsis",
    }}
    ref={dragHandle}
    >
        <span>{node.isLeaf ? "📄" : node.isOpen ? "📂" : "📁"}</span>
        <span>{node.data.name}</span>
    </div>
    )}
    </Tree>)
}