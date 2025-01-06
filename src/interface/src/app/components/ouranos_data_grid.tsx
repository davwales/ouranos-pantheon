// OuranosDataGrid.tsx

import React, { useState } from "react";
import {
    DataGrid,
    GridFilterModel,
    GridPaginationModel,
    GridSortModel,
    GridToolbar,
    GridColDef,
    GridRowIdGetter
} from "@mui/x-data-grid";
import OuranosGridModel from "../models/ouranos_grid_model";
import { SxProps } from "@mui/material";

interface OuranosDataGridProps<T> {
    columns: GridColDef[];
    getRowId: GridRowIdGetter<any>;
    rows: any;
    rowCount: number;
    loading: boolean;
    onRowClick?: (row: T) => void;
    initialModel?: OuranosGridModel;
    pageSizeOptions: number[];
    onGridModelChange?: (model: OuranosGridModel) => void,
    sx?: SxProps
}

export function OuranosDataGrid<T>(props: OuranosDataGridProps<T>) {
    const [gridModel, setGridModel] = useState<OuranosGridModel>({
        sortModel: props.initialModel?.sortModel || [],
        paginationModel: props.initialModel?.paginationModel || { page: 0, pageSize: props.pageSizeOptions[0] },
        filterModel: props.initialModel?.filterModel || { items: [] }
    })

    const handleSortModelChange = (model: GridSortModel) => {
        const updatedModel: OuranosGridModel = { ...gridModel, sortModel: model };
        setGridModel(updatedModel);
        props.onGridModelChange?.(updatedModel);
    };

    const handleFilterModelChange = (model: GridFilterModel) => {
        const updatedModel: OuranosGridModel = { ...gridModel, filterModel: model };
        setGridModel(updatedModel);
        props.onGridModelChange?.(updatedModel);
    };

    const handlePaginationModelChange = (model: GridPaginationModel) => {
        if (gridModel.paginationModel.pageSize != model.pageSize) {
            model.page = 0;
        }

        const updatedModel: OuranosGridModel = { ...gridModel, paginationModel: model };
        setGridModel(updatedModel);
        props.onGridModelChange?.(updatedModel);
    };

    return (
        <DataGrid
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
                ...props.sx,
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
