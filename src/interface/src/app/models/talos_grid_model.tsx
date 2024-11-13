import { GridFilterModel, GridPaginationModel, GridSortModel } from "@mui/x-data-grid";

export default interface TalosGridModel {
    sortModel: GridSortModel,
    filterModel: GridFilterModel,
    paginationModel: GridPaginationModel
};