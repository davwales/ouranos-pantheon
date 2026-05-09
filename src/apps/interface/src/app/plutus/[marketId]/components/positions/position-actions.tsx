import { Button } from "@/components/ui/button";
import { type Position } from "@/lib/api/plutus";
import { Link2, Pencil, Plus, X } from "lucide-react";

export type PositionActionsVariant = "open" | "closed";

export function PositionActions({
  position,
  variant,
  onEdit,
  onClose,
  onLink,
  onCreateSell,
}: {
  position: Position;
  variant: PositionActionsVariant;
  onEdit?: (position: Position) => void;
  onClose?: (position: Position) => void;
  onLink?: (position: Position) => void;
  onCreateSell?: (position: Position) => void;
}) {
  if (variant === "open") {
    return (
      <div className="flex items-center gap-1">
        {onEdit && (
          <Button variant="ghost" size="sm" onClick={() => onEdit(position)}>
            <Pencil className="w-4 h-4" />
          </Button>
        )}
        {onClose && (
          <Button variant="ghost" size="sm" onClick={() => onClose(position)}>
            <X className="w-4 h-4" />
          </Button>
        )}
      </div>
    );
  }

  const showCreateSell =
    onCreateSell && position.side === "Buy" && position.status === "Bought";
  const showLink =
    onLink && position.side === "Sell" && !position.linkedBuyPositionId;

  if (!showCreateSell && !showLink) {
    return null;
  }

  return (
    <div className="flex items-center gap-1">
      {showCreateSell && (
        <Button
          variant="ghost"
          size="sm"
          onClick={() => onCreateSell(position)}
        >
          <Plus className="w-4 h-4" />
        </Button>
      )}
      {showLink && (
        <Button variant="ghost" size="sm" onClick={() => onLink(position)}>
          <Link2 className="w-4 h-4" />
        </Button>
      )}
    </div>
  );
}
