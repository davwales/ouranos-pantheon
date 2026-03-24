import { cn } from "@/lib/utils";
import { Check } from "lucide-react";
import React, { useEffect, useState } from "react";

export default function ClipboardCopy({
  value,
  confirmation,
  children,
  successDuration = 1000,
  hideIfNotSupported = false,
  className,
  ...props
}: React.ComponentProps<"div"> & {
  value: string | number;
  confirmation?: React.ReactNode;
  children: React.ReactNode;
  successDuration?: number;
  hideIfNotSupported?: boolean;
}) {
  const [copied, setCopied] = useState(false);
  const [clipboardSupported, setClipboardSupported] = useState(false);

  useEffect(() => {
    setClipboardSupported(!!navigator.clipboard);
  }, []);

  if (!clipboardSupported) {
    if (hideIfNotSupported) {
      return null;
    }

    return <div {...props}>{children}</div>;
  }

  const handleCopy = async () => {
    try {
      await navigator.clipboard.writeText(value.toString());
      setCopied(true);

      setTimeout(() => {
        setCopied(false);
      }, successDuration);
    } catch (err) {
      console.error("Failed to copy:", err);
    }
  };

  const copyConfirmation = confirmation || (
    <div className="flex gap-1 items-center">
      Copied
      <Check className="text-green-500" size={16} strokeWidth={2.5} />
    </div>
  );

  return (
    <div
      {...props}
      onClick={handleCopy}
      className={cn("hover:cursor-pointer", className)}
    >
      {copied ? copyConfirmation : children}
    </div>
  );
}
