import { GridColDef } from "@mui/x-data-grid";

export function getFieldType(
    columns: GridColDef[],
    field: string
): string | undefined {
    const column = columns.find((col) => col.field === field);
    return column?.type;
}