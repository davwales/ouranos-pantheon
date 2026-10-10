import { CodeBlock } from "@/components/shared/code-block";
import { Typography } from "@/components/shared/typography";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { cn } from "@/lib/utils";
import React, { useMemo } from "react";
import ReactMarkdown from "react-markdown";
import rehypeSanitize from "rehype-sanitize";
import remarkBreaks from "remark-breaks";
import remarkGfm from "remark-gfm";

const remarkPlugins: React.ComponentProps<
  typeof ReactMarkdown
>["remarkPlugins"] = [remarkGfm, remarkBreaks];
const rehypePlugins: React.ComponentProps<
  typeof ReactMarkdown
>["rehypePlugins"] = [rehypeSanitize];

export type MarkdownVariant = "default" | "compact";

type BlockTag = "h1" | "h2" | "h3" | "h4" | "p" | "blockquote";

const compactHeading = "mt-4 mb-1 first:mt-0 font-semibold tracking-tight";

// Typography's variants are not merged with a passed className, so compact blocks render plain
// elements instead of overriding the page-sized Typography styles.
const compactClassName: Record<BlockTag, string> = {
  h1: cn(compactHeading, "text-base"),
  h2: cn(compactHeading, "text-base"),
  h3: cn(compactHeading, "text-sm"),
  h4: cn(compactHeading, "text-sm"),
  p: "leading-6 [&:not(:first-child)]:mt-2",
  blockquote: "mt-2 border-l-2 pl-3 italic",
};

const defaultClassName: Record<BlockTag, string> = {
  h1: "mt-8 mb-2",
  h2: "mt-8 mb-2",
  h3: "mt-8 mb-2",
  h4: "mt-6 mb-2",
  p: "",
  blockquote: "",
};

export function MarkdownRenderer({
  componentClassName,
  variant = "default",
  ...props
}: React.ComponentProps<typeof ReactMarkdown> & {
  variant?: MarkdownVariant;
  componentClassName?: {
    h1?: string;
    h2?: string;
    h3?: string;
    p?: string;
    ul?: string;
    ol?: string;
    li?: string;
    inlineCode?: string;
    blockCode?: string;
  };
}) {
  const components = useMemo<
    React.ComponentProps<typeof ReactMarkdown>["components"]
  >(() => {
    const block = (tag: BlockTag, override?: string) =>
      function MarkdownBlock({
        node,
        className,
        ...props
      }: React.HTMLAttributes<HTMLElement> & { node?: unknown }) {
        if (variant === "compact") {
          const Tag = tag;
          return (
            <Tag
              className={cn(compactClassName[tag], className, override)}
              {...props}
            />
          );
        }

        return (
          <Typography
            variant={tag === "p" ? "default" : tag}
            className={cn(defaultClassName[tag], className, override)}
            {...props}
          />
        );
      };
    const listClassName = variant === "compact" ? "pl-5 my-2" : "pl-6";

    return {
      h1: block("h1", componentClassName?.h1),
      h2: block("h2", componentClassName?.h2),
      h3: block("h3", componentClassName?.h3),
      h4: block("h4"),
      p: block("p", componentClassName?.p),
      blockquote: block("blockquote"),

      ul: ({ node, className, ...props }) => (
        <ul
          className={cn(
            "list-disc space-y-1",
            listClassName,
            className,
            componentClassName?.ul,
          )}
          {...props}
        />
      ),

      ol: ({ node, className, ...props }) => (
        <ol
          className={cn(
            "list-decimal space-y-1",
            listClassName,
            className,
            componentClassName?.ol,
          )}
          {...props}
        />
      ),

      li: ({ node, className, ...props }) => (
        <li
          className={cn("ml-2", className, componentClassName?.li)}
          {...props}
        />
      ),

      hr: ({ node, className, ...props }) => (
        <hr className={cn("my-3 border-border", className)} {...props} />
      ),

      a: ({ node, className, ...props }) => (
        <a
          className={cn(
            "font-medium text-primary underline underline-offset-4",
            className,
          )}
          target="_blank"
          rel="noreferrer"
          {...props}
        />
      ),

      table: ({ node, className, ...props }) => (
        <div className="my-3 rounded-md border">
          <Table className={cn("tabular-nums", className)} {...props} />
        </div>
      ),

      thead: ({ node, className, ...props }) => (
        <TableHeader className={cn("bg-muted/50", className)} {...props} />
      ),

      tbody: ({ node, ...props }) => <TableBody {...props} />,

      tr: ({ node, ...props }) => <TableRow {...props} />,

      th: ({ node, className, ...props }) => (
        <TableHead className={cn("h-8", className)} {...props} />
      ),

      td: ({ node, className, ...props }) => (
        <TableCell
          className={cn("px-2 py-1.5 align-top whitespace-normal", className)}
          {...props}
        />
      ),

      code({ node, inline, className, children, ...syntaxProps }: any) {
        const match = /language-(\w+)/.exec(className || "");

        return !inline && match ? (
          <CodeBlock
            language={match?.[1] || "text"}
            code={children}
            className={componentClassName?.blockCode}
            syntaxProps={syntaxProps}
          />
        ) : (
          <Typography
            variant="inlinecode"
            className={componentClassName?.inlineCode}
            {...syntaxProps}
          >
            {children}
          </Typography>
        );
      },
    };
  }, [
    variant,
    componentClassName?.h1,
    componentClassName?.h2,
    componentClassName?.h3,
    componentClassName?.p,
    componentClassName?.ul,
    componentClassName?.ol,
    componentClassName?.li,
    componentClassName?.inlineCode,
    componentClassName?.blockCode,
  ]);

  return (
    <ReactMarkdown
      remarkPlugins={remarkPlugins}
      rehypePlugins={rehypePlugins}
      components={components}
      {...props}
    />
  );
}
