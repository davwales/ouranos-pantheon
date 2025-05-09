import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Plus } from "lucide-react";
import { ExtendedColumnDef, FilterCondition, FilterOperator, OPERATOR_LABELS } from "./types";

interface FilterInputProps {
    columns: ExtendedColumnDef<any>[];
    value: FilterCondition;
    onChange: (value: FilterCondition) => void;
    onRemove: () => void;
}

function FilterInput({ columns, value, onChange, onRemove }: FilterInputProps) {
    const filterableColumns = columns.filter(col => col.filterConfig);
    const currentColumn = columns.find(col => col.id === value.field);

    const handleColumnChange = (columnId: string) => {
        const column = columns.find(col => col.id === columnId);
        if (!column?.filterConfig) return;

        onChange({
            field: columnId,
            operator: column.filterConfig.operators[0],
            value: ''
        });
    };

    const handleOperatorChange = (operator: FilterOperator) => {
        onChange({ ...value, operator });
    };

    const handleValueChange = (newValue: string) => {
        let parsedValue: any = newValue;

        switch (currentColumn?.filterConfig?.type) {
            case 'number':
                parsedValue = parseFloat(newValue);
                break;
            case 'boolean':
                parsedValue = newValue === 'true';
                break;
            case 'date':
                parsedValue = new Date(newValue).toISOString();
                break;
        }

        onChange({ ...value, value: parsedValue });
    };

    if (!currentColumn?.filterConfig) return null;

    return (
        <div className="flex flex-col md:flex-row gap-2 my-2 rounded-lg">
            <Select value={value.field} onValueChange={handleColumnChange}>
                <SelectTrigger className="w-full md:w-48">
                    <SelectValue placeholder="Column" />
                </SelectTrigger>
                <SelectContent>
                    {filterableColumns.map((col) => (
                        <SelectItem key={col.id} value={col.id as string}>
                            {col.header as string}
                        </SelectItem>
                    ))}
                </SelectContent>
            </Select>

            <Select value={value.operator} onValueChange={handleOperatorChange}>
                <SelectTrigger className="w-full md:w-48">
                    <SelectValue placeholder="Operator" />
                </SelectTrigger>
                <SelectContent>
                    {currentColumn.filterConfig.operators.map((op: FilterOperator) => (
                        <SelectItem key={op} value={op}>
                            {OPERATOR_LABELS[op]}
                        </SelectItem>
                    ))}
                </SelectContent>
            </Select>

            {currentColumn.filterConfig.type === 'enum' ? (
                <Select value={value.value} onValueChange={handleValueChange}>
                    <SelectTrigger className="w-full md:w-48">
                        <SelectValue placeholder="Value" />
                    </SelectTrigger>
                    <SelectContent>
                        {currentColumn.filterConfig.enumValues?.map((val: string) => (
                            <SelectItem key={val} value={val}>
                                {val}
                            </SelectItem>
                        ))}
                    </SelectContent>
                </Select>
            ) : (
                <Input
                    type={currentColumn.filterConfig.type === 'date' ? 'date' : 'text'}
                    value={value.value}
                    onChange={(e) => handleValueChange(e.target.value)}
                    className="w-full md:w-48"
                />
            )}

            <Button
                variant="destructive"
                onClick={onRemove}
                className="self-end w-full md:w-auto md:self-center"
            >
                Remove
            </Button>
        </div>
    );
}

export function DataTableFiltering<TData>({
    onFilterChange,
    columns,
    filter,
    ...props
}: React.ComponentProps<"div"> & {
    onFilterChange?: (filter: Record<string, any>) => void;
    columns: ExtendedColumnDef<TData>[];
    filter: Record<string, any>;
}) {
    const filterConditions = (() => {
        const conditions: FilterCondition[] = [];

        const processNestedObject = (obj: Record<string, any>, currentPath: string[] = []) => {
            Object.entries(obj).forEach(([key, value]) => {
                if (typeof value === 'object' && value !== null && !Array.isArray(value)) {
                    if (Object.keys(value).some(k => k in OPERATOR_LABELS)) {
                        Object.entries(value).forEach(([operator, operatorValue]) => {
                            if (operator in OPERATOR_LABELS) {
                                conditions.push({
                                    field: [...currentPath, key].join('.'),
                                    operator: operator as FilterOperator,
                                    value: operatorValue
                                });
                            }
                        });
                    } else {
                        processNestedObject(value, [...currentPath, key]);
                    }
                }
            });
        };

        processNestedObject(filter);
        return conditions;
    })();

    const handleAddFilter = () => {
        const filterableColumns = columns.filter(col => col.filterConfig);
        if (filterableColumns.length === 0) return;

        const newFilter: FilterCondition = {
            field: filterableColumns[0].id as string,
            operator: filterableColumns[0].filterConfig!.operators[0],
            value: '',
        };

        updateFilter([...filterConditions, newFilter]);
    };

    const handleFilterChange = (index: number, updatedFilter: FilterCondition) => {
        const updatedFilters = [...filterConditions];
        updatedFilters[index] = updatedFilter;
        updateFilter(updatedFilters);
    };

    const handleRemoveFilter = (index: number) => {
        const updatedFilters = filterConditions.filter((_, i) => i !== index);
        updateFilter(updatedFilters);
    };

    const updateFilter = (conditions: FilterCondition[]) => {
        const filterObject: Record<string, any> = {};

        conditions.forEach(condition => {
            const column = columns.find(col => col.id === condition.field);
            if (!column?.filterConfig) return;

            const fieldPath = condition.field.split('.');
            let currentLevel = filterObject;

            for (let i = 0; i < fieldPath.length - 1; i++) {
                const part = fieldPath[i];
                if (!currentLevel[part]) {
                    currentLevel[part] = {};
                }
                currentLevel = currentLevel[part];
            }

            const lastPart = fieldPath[fieldPath.length - 1];
            if (!currentLevel[lastPart]) {
                currentLevel[lastPart] = {};
            }
            currentLevel[lastPart][condition.operator] = condition.value;
        });

        if (onFilterChange) {
            onFilterChange(filterObject);
        }
    };

    return (
        <div {...props}>
            <Button
                onClick={handleAddFilter}
                className="w-full md:w-auto"
                disabled={!columns.some(col => col.filterConfig)}
            >
                Add Filter <Plus className="ml-2 h-4 w-4" />
            </Button>

            <div className="flex flex-col mt-2">
                {filterConditions.map((filter, index) => (
                    <FilterInput
                        key={index}
                        columns={columns}
                        value={filter}
                        onChange={(updatedFilter) => handleFilterChange(index, updatedFilter)}
                        onRemove={() => handleRemoveFilter(index)}
                    />
                ))}
            </div>
        </div>
    );
}