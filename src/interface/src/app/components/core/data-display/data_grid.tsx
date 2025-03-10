import { convertToSx } from "@/app/components/core/mui_style_resolvers";
import { StyleProps } from "@/app/components/core/style_props";
import { GridSortItem, GridToolbar, DataGrid as MuiDataGrid } from "@mui/x-data-grid";
import { useState } from "react";

export interface GridColDef {
    field: string;
    headerName?: string;
    type?: 'string' | 'number';
    flex?: number;
    valueFormatter?: (x: any) => any
    valueGetter?: (x: any) => string
}

export type GridSortModel = GridSortItem[];

export interface GridPaginationModel {
    page: number;
    pageSize: number;
}

export interface GridFilterModel {
    items: {
        field: string;
        operator: string;
        id?: number | string;
        value?: any;
    }[];
}

export interface GridModel {
    sortModel: GridSortModel,
    filterModel: GridFilterModel,
    paginationModel: GridPaginationModel
};

export function getFieldType(
    columns: GridColDef[],
    field: string
): string | undefined {
    const column = columns.find((col) => col.field === field);
    return column?.type;
}

interface DataGridProps<T> {
    columns: GridColDef[];
    getRowId: (x: any) => any;
    rows: any;
    rowCount: number;
    loading: boolean;
    onRowClick?: (row: T) => void;
    initialModel?: GridModel;
    pageSizeOptions: number[];
    onGridModelChange?: (model: GridModel) => void,
    styling?: StyleProps
}

export function DataGrid<T>(props: DataGridProps<T>) {
    const [gridModel, setGridModel] = useState<GridModel>({
        sortModel: props.initialModel?.sortModel || [],
        paginationModel: props.initialModel?.paginationModel || { page: 0, pageSize: props.pageSizeOptions[0] },
        filterModel: props.initialModel?.filterModel || { items: [] }
    })

    const handleSortModelChange = (model: GridSortModel) => {
        const updatedModel: GridModel = { ...gridModel, sortModel: model };
        setGridModel(updatedModel);
        props.onGridModelChange?.(updatedModel);
    };

    const handleFilterModelChange = (model: GridFilterModel) => {
        const updatedModel: GridModel = { ...gridModel, filterModel: model };
        setGridModel(updatedModel);
        props.onGridModelChange?.(updatedModel);
    };

    const handlePaginationModelChange = (model: GridPaginationModel) => {
        if (gridModel.paginationModel.pageSize != model.pageSize) {
            model.page = 0;
        }

        const updatedModel: GridModel = { ...gridModel, paginationModel: model };
        setGridModel(updatedModel);
        props.onGridModelChange?.(updatedModel);
    };

    return (
        <MuiDataGrid
            rows={props.rows}
            columns={props.columns}
            getRowId={props.getRowId}
            rowCount={props.rowCount}
            loading={props.loading}
            sortingMode="server"
            sortModel={gridModel.sortModel}
            onSortModelChange={handleSortModelChange}
            paginationMode="server"
            paginationModel={gridModel.paginationModel}
            onPaginationModelChange={handlePaginationModelChange}
            pageSizeOptions={props.pageSizeOptions}
            filterMode="server"
            filterModel={gridModel.filterModel}
            onFilterModelChange={handleFilterModelChange}
            onRowClick={(params) => props.onRowClick?.(params.row)}
            slots={{ toolbar: GridToolbar }}
            rowSelection={false}
            autoHeight
            sx={{
                ...(props.styling && convertToSx(props.styling)),
                '.MuiDataGrid-cell:focus': {
                    outline: 'none'
                },
                '& .MuiDataGrid-row:hover': {
                    cursor: "pointer"
                }
            }}
        />
    );
}
