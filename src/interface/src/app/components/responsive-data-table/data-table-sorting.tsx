import { ExtendedColumnDef, SortArgs } from "@/app/components/responsive-data-table/types";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { SortEnumType } from "@/gql/graphql";
import { ArrowDown, ArrowUp, ArrowUpDown } from "lucide-react";

export function DataTableSorting({
    columns,
    sort,
    onSortChange,
    variant = 'desktop'
}: {
    columns: ExtendedColumnDef<any>[];
    sort?: SortArgs;
    onSortChange?: (sort: SortArgs) => void;
    variant?: 'desktop' | 'mobile';
}) {
    const getSortState = (columnId: string): SortEnumType | null => {
        if (!sort) return null;

        const parts = columnId.split('.');
        let current = sort;
        for (const part of parts) {
            if (!current || !current[part]) {
                return null;
            } else if (typeof current[part] === 'object') {
                current = current[part];
            } else {
                return current[part] as SortEnumType;
            }
        }

        return null;
    };

    const idToObject = (columnId: string, direction: SortEnumType): SortArgs => {
        const parts = columnId.split('.');
        const root: SortArgs = {};
        let current = root;

        for (let i = 0; i < parts.length - 1; i++) {
            current[parts[i]] = {};
            current = current[parts[i]] as SortArgs;
        }

        current[parts[parts.length - 1]] = direction;
        return root;
    }

    const getCurrentSortColumn = (): string => {
        if (!sort) return "";
        const [key] = Object.keys(sort);
        return key ?? "";
    };

    const handleSortChange = (columnId: string, direction?: SortEnumType) => {
        if (!onSortChange) return;

        if (!columnId) {
            onSortChange({});
            return;
        }

        if (direction) {
            onSortChange(idToObject(columnId, direction));
        } else {
            const currentDirection = getSortState(columnId);
            const newDirection = !currentDirection ? SortEnumType.Asc : currentDirection === SortEnumType.Asc ? SortEnumType.Desc : null;

            if (newDirection) {
                onSortChange(idToObject(columnId, newDirection));
            } else {
                onSortChange({});
            }
        }
    };

    if (variant === 'mobile') {
        return (
            <div className="flex items-center gap-2 mt-2 w-full">
                <h6>Sort By</h6>
                <Select
                    value={getCurrentSortColumn()}
                    onValueChange={(value) => handleSortChange(value)}
                >
                    <SelectTrigger className="flex-grow max-w-1/2">
                        <SelectValue placeholder="Sort by column" />
                    </SelectTrigger>
                    <SelectContent>
                        {columns.map((column) => {
                            if (!column.id) return null;
                            return (
                                <SelectItem key={column.id} value={column.id}>
                                    {typeof column.header === 'string' ? column.header : column.id}
                                </SelectItem>
                            )
                        })}
                    </SelectContent>
                </Select>

                <Select
                    value={getSortState(getCurrentSortColumn()) ?? ""}
                    onValueChange={(value) => handleSortChange(getCurrentSortColumn(), value as SortEnumType)}
                    disabled={!getCurrentSortColumn()}
                >
                    <SelectTrigger className="flex-grow max-w-1/2">
                        <SelectValue placeholder="Direction" />
                    </SelectTrigger>
                    <SelectContent>
                        <SelectItem value={SortEnumType.Asc}>Ascending</SelectItem>
                        <SelectItem value={SortEnumType.Desc}>Descending</SelectItem>
                    </SelectContent>
                </Select>
            </div>
        );
    }

    return (
        <>
            {columns.map((column) => {
                const direction = getSortState(column.id ?? '');
                const getSortIcon = () => {
                    if (!direction) {
                        return <ArrowUpDown className="opacity-0 hover:opacity-100 h-4 w-4" />;
                    }
                    return direction === SortEnumType.Asc ?
                        <ArrowUp className="h-4 w-4" /> :
                        <ArrowDown className="h-4 w-4" />;
                };

                return (
                    <div
                        key={column.id}
                        className="flex items-center gap-2 cursor-pointer select-none"
                        onClick={() => handleSortChange(column.id ?? '')}
                    >
                        {getSortIcon()}
                    </div>
                );
            })}
        </>
    );
} 