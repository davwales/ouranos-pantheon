// TalosDataGrid.tsx

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
import TalosGridModel from "../models/talos_grid_model";
import { SxProps } from "@mui/material";

interface TalosDataGridProps<T> {
    columns: GridColDef[];
    getRowId: GridRowIdGetter<any>;
    rows: any;
    rowCount: number;
    loading: boolean;
    onRowClick?: (row: T) => void;
    initialModel?: TalosGridModel;
    pageSizeOptions: number[];
    onGridModelChange?: (model: TalosGridModel) => void,
    sx?: SxProps
}

export function TalosDataGrid<T>(props: TalosDataGridProps<T>) {
    const [gridModel, setGridModel] = useState<TalosGridModel>({
        sortModel: props.initialModel?.sortModel || [],
        paginationModel: props.initialModel?.paginationModel || { page: 0, pageSize: props.pageSizeOptions[0] },
        filterModel: props.initialModel?.filterModel || { items: [] }
    })

    const handleSortModelChange = (model: GridSortModel) => {
        const updatedModel: TalosGridModel = { ...gridModel, sortModel: model };
        setGridModel(updatedModel);
        props.onGridModelChange?.(updatedModel);
    };

    const handleFilterModelChange = (model: GridFilterModel) => {
        const updatedModel: TalosGridModel = { ...gridModel, filterModel: model };
        setGridModel(updatedModel);
        props.onGridModelChange?.(updatedModel);
    };

    const handlePaginationModelChange = (model: GridPaginationModel) => {
        if (gridModel.paginationModel.pageSize != model.pageSize) {
            model.page = 0;
        }

        const updatedModel: TalosGridModel = { ...gridModel, paginationModel: model };
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
