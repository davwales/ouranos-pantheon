import { Textarea } from "@/components/ui/textarea";
import React from "react";

export default function AutosizeTextarea(
  props: React.ComponentProps<typeof Textarea>
) {
  return (
    <Textarea
      ref={(textarea) => {
        if (textarea) {
          textarea.style.height = "0px";
          textarea.style.height = textarea.scrollHeight + "px";
        }
      }}
      {...props}
    />
  );
}
