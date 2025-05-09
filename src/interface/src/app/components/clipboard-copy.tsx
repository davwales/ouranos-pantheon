import { Check } from 'lucide-react';
import React, { useState } from 'react';

export default function ClipboardCopy({
    value,
    confirmation,
    children,
    successDuration = 1000,
    className,
    ...props
}: React.ComponentProps<"div"> & {
    value: string | number;
    confirmation?: React.ReactNode;
    children: React.ReactNode;
    successDuration?: number;
}) {
    const [copied, setCopied] = useState(false);

    const handleCopy = async () => {
        try {
            await navigator.clipboard.writeText(value.toString());
            setCopied(true);

            setTimeout(() => {
                setCopied(false);
            }, successDuration);
        } catch (err) {
            console.error('Failed to copy:', err);
        }
    };

    const copyConfirmation = confirmation || (
        <div className="flex gap-1 items-center">
            Copied
            <Check
                className="text-green-500"
                size={16}
                strokeWidth={2.5}
            />
        </div>
    );

    return (
        <div {...props} onClick={handleCopy} className={`hover:cursor-pointer ${className}`}>
            {copied ? copyConfirmation : children}
        </div>
    );
};