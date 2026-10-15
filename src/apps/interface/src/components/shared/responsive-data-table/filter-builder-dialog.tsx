import { ResponsiveDialog } from "@/components/shared/responsive-dialog";
import { Button } from "@/components/ui/button";
import { SlidersHorizontal } from "lucide-react";
import { FilterGroupBuilder } from "./filter-group-builder";
import { type ExtendedColumnDef, type FilterGroup } from "./types";

type FilterBuilderDialogProps = {
  group: FilterGroup;
  columns: ExtendedColumnDef<any>[];
  onChange: (group: FilterGroup) => void;
};

export function FilterBuilderDialog({
  group,
  columns,
  onChange,
}: FilterBuilderDialogProps) {
  return (
    <ResponsiveDialog
      title="Filters"
      description="Build filters visually. Changes apply immediately and update the search box."
      trigger={
        <Button type="button" variant="outline" size="sm" className="h-9">
          <SlidersHorizontal className="h-4 w-4" />
          <span className="hidden sm:inline">Filters</span>
        </Button>
      }
    >
      <div className="max-h-[70vh] overflow-y-auto">
        <FilterGroupBuilder
          group={group}
          columns={columns}
          depth={0}
          onChange={onChange}
        />
      </div>
    </ResponsiveDialog>
  );
}
