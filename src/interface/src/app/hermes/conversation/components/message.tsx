import { MarkdownRenderer } from "@/app/components/markdown-renderer";
import { Typography } from "@/app/components/typography";
import { Role } from "@/lib/api/hermes";

export function Message({
  name,
  role,
  content,
  ...props
}: React.ComponentProps<"div"> & {
  name: string;
  role: Role;
  content: string;
}) {
  return (
    <div {...props}>
      <div
        className={`py-2 px-4 border rounded-2xl ${
          role == Role.User ? "bg-accent/30" : ""
        }`}
      >
        <MarkdownRenderer componentClassName={{ blockCode: "my-4" }}>{content}</MarkdownRenderer>
      </div>
      <Typography
        variant="muted"
        className={`mx-2.5 my-1 ${role == Role.User && "text-right"}`}
      >
        {name}
      </Typography>
    </div>
  );
}
