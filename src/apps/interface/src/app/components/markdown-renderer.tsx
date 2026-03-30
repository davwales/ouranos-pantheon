import { CodeBlock } from "@/app/components/code-block";
import { Typography } from "@/app/components/typography";
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

export function MarkdownRenderer({
  componentClassName,
  ...props
}: React.ComponentProps<typeof ReactMarkdown> & {
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
  >(
    () => ({
      h1: ({ node, className, ...props }) => (
        <Typography
          variant="h1"
          className={`mt-8 mb-2 ${className} ${componentClassName?.h1}`}
          {...props}
        />
      ),

      h2: ({ node, className, ...props }) => (
        <Typography
          variant="h2"
          className={`mt-8 mb-2 ${className} ${componentClassName?.h2}`}
          {...props}
        />
      ),

      h3: ({ node, className, ...props }) => (
        <Typography
          variant="h3"
          className={`mt-8 mb-2 ${className} ${componentClassName?.h3}`}
          {...props}
        />
      ),

      p: ({ className, ...props }) => (
        <Typography
          className={`${className} ${componentClassName?.p}`}
          {...props}
        />
      ),

      ul: ({ className, ...props }) => (
        <ul
          className={`list-disc pl-6 space-y-1 ${className} ${componentClassName?.ul}`}
          {...props}
        />
      ),

      ol: ({ className, ...props }) => (
        <ol
          className={`list-decimal pl-6 space-y-1 ${className} ${componentClassName?.ol}`}
          {...props}
        />
      ),

      li: ({ className, ...props }) => (
        <li
          className={`ml-2 ${className} ${componentClassName?.li}`}
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
    }),
    [
      componentClassName?.h1,
      componentClassName?.h2,
      componentClassName?.h3,
      componentClassName?.p,
      componentClassName?.ul,
      componentClassName?.ol,
      componentClassName?.li,
      componentClassName?.inlineCode,
      componentClassName?.blockCode,
    ],
  );

  return (
    <ReactMarkdown
      remarkPlugins={remarkPlugins}
      rehypePlugins={rehypePlugins}
      components={components}
      {...props}
    />
  );
}
