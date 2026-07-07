import { Card, CardContent } from "@/components/ui/card";
import { ResponsiveDialog } from "@/components/shared/responsive-dialog/responsive-dialog";

export type VersionHistoryDialogProps = {
  recipeId: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
};

export function VersionHistoryDialog({
  recipeId,
  open,
  onOpenChange,
}: VersionHistoryDialogProps) {
  return (
    <ResponsiveDialog
      title="Version History"
      description={`Recipe ${recipeId}`}
      trigger={null}
      open={open}
      onOpenChange={onOpenChange}
    >
      <CardContent className="p-6 text-sm text-muted-foreground">
        The version history timeline will appear here once the
        <code className="mx-1">
          GET /api/hestia/recipes/{recipeId}/history
        </code>
        endpoint is implemented. Currently only the original creation event
        exists.
      </CardContent>
    </ResponsiveDialog>
  );
}