import { ResponsiveDialog } from "@/components/shared/responsive-dialog/responsive-dialog";
import { Button } from "@/components/ui/button";
import { useState } from "react";

export function ConfirmationButton({
  title,
  description,
  onConfirm,
  confirmProps = {
    variant: "destructive",
    children: "Confirm",
    size: "lg",
  },
  cancelProps = {
    variant: "outline",
    children: "Cancel",
    size: "lg",
  },
  ...props
}: React.ComponentProps<typeof Button> & {
  title: string;
  description: string;
  onConfirm: () => void;
  confirmProps?: React.ComponentProps<typeof Button>;
  cancelProps?: React.ComponentProps<typeof Button>;
}) {
  const [open, setOpen] = useState(false);

  const handleConfirm = () => {
    onConfirm();
    setOpen(false);
  };

  return (
    <ResponsiveDialog
      title={title}
      description={description}
      open={open}
      onOpenChange={setOpen}
      trigger={<Button {...props} onClick={() => setOpen(true)} />}
    >
      <div className="gap-2">
        <Button
          {...confirmProps}
          onClick={handleConfirm}
          className={`w-full ${confirmProps.className}`}
        />

        <Button
          {...cancelProps}
          onClick={() => setOpen(false)}
          className={`w-full mt-4 ${cancelProps.className}`}
        />
      </div>
    </ResponsiveDialog>
  );
}
